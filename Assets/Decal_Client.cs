using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
//using UnityEditor.PackageManager.Requests;
using UnityEngine;
using UnityEngine.Networking;


[System.Serializable]
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

[System.Serializable]
public struct Packet
{
    public DecalInfo[] decalData;
    public string timeStamp;

    public Packet(DecalInfo[] decalData, string timeStamp)
    {
        this.decalData = decalData;
        this.timeStamp = timeStamp;
    }
}


public class Decal_Client : MonoBehaviour
{
    public float decalInterval;
    public GameObject[] trailDecals;
    public List<DecalInfo> placedDecals = new List<DecalInfo>();
    DecalInfo lastDecal => placedDecals[placedDecals.Count - 1];

    private void Start()
    {
        StartCoroutine(GetRequest("http://localhost"));
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
                ReceiveTrailSet(GetDecalPacketFromBytes(webRequest.downloadHandler.data));
            }
        }
    }

    public void SubmitTrailData()
    {
        StartCoroutine(Upload());
    }
    float deltaTime;
    string onPost = "untried";
    void OnGUI()
    {
        deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
        int w = Screen.width, h = Screen.height;

        GUIStyle style = new GUIStyle();

        Rect rect = new Rect(0, 0, w, h * 2 / 100);
        style.alignment = TextAnchor.UpperCenter;
        style.fontSize = h * 5 / 100;
        style.normal.textColor = Color.red;



        GUI.Label(rect, onPost, style);
    }

    IEnumerator Upload()
    {
        onPost = "Trying";
        string json = DecalDataToJson();
        Debug.Log("Pre_send JSON:");
        Debug.Log(json);

        using (UnityWebRequest www = UnityWebRequest.Put("http://localhost", json))
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
                Debug.Log("Form upload complete!");
                onPost = "upload complete!";
            }
        }
    }

    Packet GetDecalPacketFromBytes(byte[] serverData)
    {
        string json = Encoding.UTF8.GetString(serverData);
        Packet packet = JsonUtility.FromJson<Packet>("{"+json);
        return packet;
    }

    string DecalDataToJson()
    {
        Packet packet = new Packet(placedDecals.ToArray(), DateTime.Now.ToString());
        string json = JsonUtility.ToJson(packet);
        return json;
    }

    public void ReceiveTrailSet(Packet decalPacket)
    {
        foreach (var item in decalPacket.decalData)
        {
            PlaceDecal(item.decalPrefabIndex, item.position, item.rotation);
        }
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
        PlaceDecal(index, trailPos, rot, true);
    }

    public void PlaceDecal(int prefabIndex, Vector3 position, Quaternion rot, bool live = false)
    {
        GameObject _trailPiece = Instantiate(trailDecals[prefabIndex], position, rot);

        if (live)
            placedDecals.Add(new DecalInfo(prefabIndex, position, rot));
    }
}
