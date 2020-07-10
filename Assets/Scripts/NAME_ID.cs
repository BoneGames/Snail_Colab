using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

public class NAME_ID : MonoBehaviour
{
    public string _name;
    // Start is called before the first frame update
    [Button]
    public void GetName()
    {
        _name = transform.name;
    }

    public void RenamePlantParent()
    {
        foreach (Transform item in transform)
        {
            if(item.name.Contains("PlantParent"))
            {
                item.name = "PlantParent_" + transform.name;
            }
        }
    }
}
