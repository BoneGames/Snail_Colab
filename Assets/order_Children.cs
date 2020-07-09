using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

public class order_Children : MonoBehaviour
{
    [Button]
    void Rename()
    {
        int rockNumber = 0;
        foreach (Transform item in transform)
        {
            if (item != transform)
            {
                item.name = rockNumber.ToString();
                GameObject go = new GameObject();
                go.transform.parent = item;
                go.transform.localPosition = new Vector3();
                go.name = "RockParent_" + item.name;
                rockNumber++;
            }
        }
    }

    [Button]
    void RenamePlantParents()
    {
        foreach (Transform item in transform)
        {
            if (item != transform)
            {
                item.GetComponent<NAME_ID>().RenamePlantParent();
            }
        }
    }
}
