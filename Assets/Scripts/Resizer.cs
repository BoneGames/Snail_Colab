using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
using System.Linq;

public class Resizer : MonoBehaviour
{
    RectTransform rect;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    [Button]
    void UpdateSize()
    {
        rect = GetComponent<RectTransform>();
        List<Vector3> points = new List<Vector3>();

        foreach (RectTransform rT in rect)
        {
            // create array container
            Vector3[] cornersArray = new Vector3[4];
            // add child transform corners to array
            rT.GetWorldCorners(cornersArray);
            // add array points to list
            points.AddRange(cornersArray.ToList());
        }

        int tries = 0;

        while(!Contains(points))
        {
            rect.sizeDelta += new Vector2(0, 1);
            tries++;
            if(tries > 200)
            {
                Debug.Log("While loop tried out");
                break;
            }
        }
    }

    bool Contains(List<Vector3> points)
    {
        foreach (var item in points)
        {
            if(!rect.rect.Contains(item))
            {
                return false;
            }
        }
        return true;
    }
}
