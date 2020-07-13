using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using FullSerializer;

[Serializable]
public struct DecalInfo
{
    public int decalPrefabIndex;
    public Vector3 position;
    public Quaternion rotation;

    public DecalInfo(int decalPrefabIndex, Vector3 position, Quaternion rotation)
    {
        this.decalPrefabIndex = decalPrefabIndex;
        this.position = position;
        this.rotation = rotation;
    }
}

[Serializable]
public struct Packet
{
    public DecalInfo[] decalInfo;
    public string timeStamp;

    public Packet(DecalInfo[] decalData, string timeStamp)
    {
        this.decalInfo = decalData;
        this.timeStamp = timeStamp;
    }
}


public class Decal_Client : MonoBehaviour
{
    public bool loadDecals;
    public float decalInterval;
    public GameObject[] trailDecals;
    public List<DecalInfo> placedDecals = new List<DecalInfo>();
    DecalInfo lastDecal => placedDecals[placedDecals.Count - 1];
    public string serverDest;
    fsSerializer serializer;
    public Camera cam;

    public bool debug;

    private void Awake()
    {
        //cam.cullingMask = 0;
    }

    private void Start()
    {
        decalCol = decalMaterial.color;
        if (loadDecals)
        {
            StartCoroutine(GetRequest(serverDest));
            serializer = new fsSerializer();
        }
    }


    IEnumerator GetRequest(string serverDest)
    {
        UnityWebRequest.ClearCookieCache();
        using (UnityWebRequest webRequest = UnityWebRequest.Get(serverDest))
        {
            webRequest.SetRequestHeader("Access-Control-Allow-Credentials", "true");
            webRequest.SetRequestHeader("Access-Control-Allow-Headers", "Accept, Content-Type, X-Access-Token, X-Application-Name, X-Request-Sent-Time");
            webRequest.SetRequestHeader("Access-Control-Allow-Methods", "GET, POST, PUT, OPTIONS");
            webRequest.SetRequestHeader("Access-Control-Allow-Origin", "*");
            // Request and wait for the desired page.
            yield return webRequest.SendWebRequest();

            if (webRequest.isNetworkError)
            {
                if (debug)
                    Debug.LogError("Network Failed To Connect: " + webRequest.error);
            }
            else
            {
                ReceiveTrailSet(GetDecalPacketsFromBytes(webRequest.downloadHandler.data));
            }
        }
    }

    Packet[] Deserialize(string jsonPacketStrings)
    {
        // step 1: parse the JSON data
        fsData data = fsJsonParser.Parse(jsonPacketStrings);

        // step 2: deserialize the data
        string[] deserialized = default;
        var result = serializer.TryDeserialize(data, ref deserialized).AssertSuccessWithoutWarnings();

        Debug.Log(result.FormattedMessages);

        List<Packet> packetList = new List<Packet>();

        foreach (var str in deserialized)
        {
            try
            {
                packetList.Add(JsonUtility.FromJson<Packet>(str));

            }
            catch
            {
                Debug.LogError("Can't deserialise this jsoninto Packet:");
                Debug.Log(str);
            }
        }

        return packetList.ToArray();
    }

    public void SubmitTrailData()
    {
        if (loadDecals)
        {
            StartCoroutine(Upload());
        }
        else
        {
            Debug.Log("Networked decals are curently disabled. Enable 'Load Decals' to re-enable (but can you handle all that server sheeeeeeet..?)");
        }
    }

    IEnumerator Upload()
    {
        string json = DecalDataToJson();
        if (debug)
        {
            Debug.Log("Pre Send Json:");
            Debug.Log(json);
        }

        using (UnityWebRequest www = UnityWebRequest.Put(serverDest, json))
        {
            www.method = UnityWebRequest.kHttpVerbPOST;
            www.SetRequestHeader("Content-Type", "application/json");
            www.SetRequestHeader("Accept", "application/json");

            www.chunkedTransfer = false;
            yield return www.SendWebRequest();

            if (www.isNetworkError || www.isHttpError)
            {
                if (debug)
                    Debug.Log(www.error);
            }
            else
            {
                if (debug)
                    Debug.Log("Decal Upload complete!");
            }
        }
    }

    Packet[] GetDecalPacketsFromBytes(byte[] serverData)
    {
        // has the server sent any actual data? (not just a single byte of '123')
        if (serverData.Length > 2)
        {
            string json = Encoding.UTF8.GetString(serverData);
            if (debug)
            {
                Debug.Log("Json decoded from server byte[]:");
                Debug.Log(json);
            }
            // Packet[] packet = JsonUtility.FromJson<Packet[]>(@json);
            Packet[] packetArray = Deserialize(json);
            if (debug)
            {
                Debug.Log("<>");
                Debug.Log("Packet[] after decoding from Json:");
                Debug.Log(packetArray);
            }
            return packetArray;
        }
        else
            return new Packet[] { new Packet() };
    }

    string DecalDataToJson()
    {
        Packet packet = new Packet(placedDecals.ToArray(), DateTime.Now.ToString());
        string json = JsonUtility.ToJson(packet);
        return json;
    }

    public void ReceiveTrailSet(Packet[] decalPackets)
    {
        if (decalPackets[0].decalInfo == null)
        {
            if (debug)
            {
                Debug.Log("No Data Received from server");
                Debug.Log("You are the first player or something fucked up...");
            }
        }
        else
        {
            foreach (Packet packet in decalPackets)
            {
                float decalAlpha = GetTimeToAlphaRation(packet.timeStamp);
                if (decalAlpha == 0)
                    continue;

                foreach (DecalInfo decal in packet.decalInfo)
                {
                    PlaceDecal(decal.decalPrefabIndex, decal.position, decal.rotation, decalAlpha);
                }
            }
        }
        //cam.cullingMask = -1;
    }

    public float trailDuration;

    float GetTimeToAlphaRation(string timeStamp)
    {
        DateTime timeCreated = DateTime.Parse(timeStamp);
        // Debug.LogError("Time Created: " + timeCreated);
        DateTime currentTime = DateTime.Now;
        // Debug.LogError("Current Time: " + currentTime);
        TimeSpan interval = currentTime.Subtract(timeCreated);
        float hoursPassed = (float)interval.TotalHours;
        float alpha = Mathf.Clamp((-(1f / trailDuration) * hoursPassed + 1f), 0, 1);

        if (alpha != 0)
        {
            int alphaLength = alpha.ToString().Length;
            alpha = alphaLength < 4 ? float.Parse(alpha.ToString().Substring(0, alphaLength)) : float.Parse(alpha.ToString().Substring(0, 4));
        }

        //if (debug)
        // Debug.Log("Set Decal Alpha to " + alpha + ", based on " + hoursPassed + " since being laid down");
        return alpha;
    }

    public void CreateTrail(Vector3 trailPos, Vector3 snailForward)
    {
        if (placedDecals.Count > 0)
        {
            float dist = Vector3.Distance(lastDecal.position, trailPos);
            if (dist < decalInterval)
            {
                return;
            }
        }
        int index = UnityEngine.Random.Range(0, trailDecals.Length);
        Quaternion rot = Quaternion.LookRotation(Vector3.up, snailForward);
        PlaceDecal(index, trailPos, rot, 1, true);
    }

    public Material decalMaterial;
    Color decalCol;

    public void PlaceDecal(int prefabIndex, Vector3 position, Quaternion rot, float alpha, bool live = false)
    {
        GameObject _trailPiece = Instantiate(trailDecals[prefabIndex], position, rot, transform);

        ////_trailPiece.GetComponent<Renderer>().material.color = new Color(decalCol.r, decalCol.g, decalCol.b, alpha);
        //Material matInstance = Instantiate(decalMaterial);

        //foreach (var item in matInstance.GetTexturePropertyNames())
        //{
        //    Debug.LogError(item);
        //}
        //matInstance.SetColor("_MainTex", new Color(decalCol.r, decalCol.g, decalCol.b, alpha));

        ////Debug.LogError("Props: " +matInstance.GetTexturePropertyNames());

        //_trailPiece.GetComponent<Renderer>().material.SetColor("_BaseColor", new Color(decalCol.r, decalCol.g, decalCol.b, alpha));



        if (live)
            placedDecals.Add(new DecalInfo(prefabIndex, position, rot));
    }
}
