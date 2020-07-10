using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
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
    public float decalInterval;
    public GameObject[] trailDecals;
    public List<DecalInfo> placedDecals = new List<DecalInfo>();
    DecalInfo lastDecal => placedDecals[placedDecals.Count - 1];
    public string serverIP;
    fsSerializer serializer;

    private void Start()
    {
        decalCol = decalMaterial.color;
        StartCoroutine(GetRequest(serverIP));
        serializer = new fsSerializer();
        
    }

    IEnumerator GetRequest(string uri)
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Get(uri))
        {
            // Request and wait for the desired page.
            yield return webRequest.SendWebRequest();

            if (webRequest.isNetworkError)
            {
                Debug.LogError("Network Failed To Connect: " + webRequest.error);
            }
            else
            {
                ReceiveTrailSet(GetDecalPacketsFromBytes(webRequest.downloadHandler.data));
            }
        }
    }

    /* public fsResult TrySerialize(Type storageType, object instance, out fsData data)
     {
         return TrySerialize(storageType, null, instance, out data);*/

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
            packetList.Add(JsonUtility.FromJson<Packet>(str));
        }

        return packetList.ToArray();
    }

    public void SubmitTrailData()
    {
        StartCoroutine(Upload());
    }
    float deltaTime;
    string onPost = "untried";
    //void OnGUI()
    //{
    //    deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
    //    int w = Screen.width, h = Screen.height;

    //    GUIStyle style = new GUIStyle();

    //    Rect rect = new Rect(0, 0, w, h * 2 / 100);
    //    style.alignment = TextAnchor.UpperCenter;
    //    style.fontSize = h * 5 / 100;
    //    style.normal.textColor = Color.red;



    //    GUI.Label(rect, onPost, style);
    //}

    IEnumerator Upload()
    {
        onPost = "Uploading Decal Data...";
        string json = DecalDataToJson();
        Debug.Log("Pre Send Json:");
        Debug.Log(json);

        using (UnityWebRequest www = UnityWebRequest.Put(serverIP, json))
        {
            www.method = UnityWebRequest.kHttpVerbPOST;
            www.SetRequestHeader("Content-Type", "application/json");
            www.SetRequestHeader("Accept", "application/json");

            www.chunkedTransfer = false;
            yield return www.SendWebRequest();

            if (www.isNetworkError || www.isHttpError)
            {
                Debug.Log(www.error);
                onPost = www.error;
            }
            else
            {
                Debug.Log("Decal Upload complete!");
                onPost = "Decal Upload complete!";
            }
        }
    }

    Packet[] GetDecalPacketsFromBytes(byte[] serverData)
    {
        // has the server sent any actual data? (not just a single byte of '123')
        if (serverData.Length > 2)
        {
            string json = Encoding.UTF8.GetString(serverData);
            Debug.Log("Json decoded from server byte[]:");
            Debug.Log(json);
            // Packet[] packet = JsonUtility.FromJson<Packet[]>(@json);
            Packet[] packetArray = Deserialize(json);
            Debug.Log("<>");
            Debug.Log("Packet[] after decoding from Json:");
            Debug.Log(packetArray);
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
            Debug.Log("No Data Received from server");
            Debug.Log("You are the first player or something fucked up...");
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
    }

    public float trailDuration;

    float GetTimeToAlphaRation(string timeStamp)
    {
        DateTime timeCreated = DateTime.Parse(timeStamp);
        DateTime currentTime = DateTime.Now;
        TimeSpan interval = currentTime - timeCreated;
        float hoursPassed = (float)interval.TotalHours;
        float alpha = Mathf.Clamp((-(1f / trailDuration) * hoursPassed + 1f), 0, 1);
        Debug.Log("Set Decal Alpha to " + alpha + ", based on " + hoursPassed + " since being laid down");
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
        int index = UnityEngine.Random.Range(0, trailDecals.Length - 1);
        Quaternion rot = Quaternion.LookRotation(Vector3.up, snailForward);
        PlaceDecal(index, trailPos, rot, 1, true);
    }

    public Material decalMaterial;
    Color decalCol;

    public void PlaceDecal(int prefabIndex, Vector3 position, Quaternion rot, float alpha, bool live = false)
    {
        GameObject _trailPiece = Instantiate(trailDecals[prefabIndex], position, rot);

        _trailPiece.GetComponent<Renderer>().material.color = new Vector4(decalCol.r, decalCol.g, decalCol.b, alpha);

        if (live)
            placedDecals.Add(new DecalInfo(prefabIndex, position, rot));
    }
}
