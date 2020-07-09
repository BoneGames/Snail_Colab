using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEditor;
using UnityEngine;
using NaughtyAttributes;

public class Click_To_Plant : MonoBehaviour
{
    public List<GameObject> plants = new List<GameObject>();
    public List<GameObject> placedPlants = new List<GameObject>();
    public int currentPlantindex;
    public KeyCode scaleKey;
    public bool randomRotation;
    GameObject lastPlant => placedPlants[placedPlants.Count - 1];
    // Start is called before the first frame update
    void Start()
    {
        currentPlantindex = 0;
        placedPlants.Clear();
    }

    //[Button]
    //void GetList()
    //{
    //    plants = placedPlants;
    //}

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            // Debug.LogError("click");
            PlacePlant(false);
        }
        else if (Input.GetMouseButtonDown(1))
        {
            Destroy(lastPlant);
            placedPlants.RemoveAt(placedPlants.Count - 1);
            // Debug.Log("destroy");
        }
        else if (Input.GetMouseButtonDown(2))
        {
            PlacePlant(true);
        }

        if(Input.GetKey(scaleKey))
        {
            if(placedPlants.Count > 0)
            lastPlant.transform.localScale *= (1f + Input.mouseScrollDelta.y/5f);
        }
    }

    GameObject GetPlant(bool newPlant)
    {
        if (newPlant)
        {
            int newPlantIndex = Random.Range(0, plants.Count);
            // assure new plant
            while (newPlantIndex == currentPlantindex)
            {
                newPlantIndex = Random.Range(0, plants.Count);
            }
            currentPlantindex = newPlantIndex;
        }

        return plants[currentPlantindex];
    }

    Transform GetPlantParent(RaycastHit hit)
    {
        Transform rockParent = hit.transform.parent;
        if(rockParent.tag != "RockParent")
        {
            rockParent = rockParent.parent;
        }
        Debug.Log(rockParent.name);
        foreach (Transform item in rockParent)
        {
            if(item.name.Contains("PlantParent"))
            {
                return item;
            }
        }
        Debug.LogError("Could not find plant parent on " + hit.transform.name);
        return null;
    }

    public void PlacePlant(bool newPlant)
    {
        
        //HandleUtility.GUIPointToWorldRay(Event.current.mousePosition)
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);// HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 100f, 1<<8 | 1<<9))
        {
            Vector3 plantPos = hit.point;
            GameObject plantModel = GetPlant(newPlant);
            Transform plantParent = GetPlantParent(hit);
            GameObject go = Instantiate(plantModel, plantPos, Quaternion.identity, plantParent);
            go.transform.up = GetPlantRot(hit.normal);
            placedPlants.Add(go);

            if(randomRotation)
            {
                go.transform.eulerAngles = new Vector3(transform.eulerAngles.x, Random.Range(0f, 360f), transform.eulerAngles.z);
            }
        }
        else
        {
            Debug.Log("no hit");
        }
    }
    public float lerper;
    Vector3 GetPlantRot(Vector3 normal)
    {
        return Vector3.Lerp(normal, Vector3.up, lerper);

    }
}
