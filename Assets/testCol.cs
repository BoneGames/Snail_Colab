using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

public class testCol : MonoBehaviour
{
    //public Material mat;
    public string property;
    public float x;
    // Start is called before the first frame update
    //[Button]
    //void SetCols()
    //{
    //    foreach (var item in mat.GetTexturePropertyNames())
    //    {
    //        GameObject _x = Instantiate(gameObject, transform.position + new Vector3(Random.Range(1, 
    //            5), 0, Random.Range(1, 5)), Quaternion.identity);
    //        Material m = Instantiate(mat);
    //        m.SetColor(item, new Color(x, x, x, x));

    //        _x.GetComponent<Renderer>().material = m;
    //    }
        
    //}

    [Button]
    void SetCol()
    {
        Material mat = GetComponent<Renderer>().material;
        mat.SetColor(property, new Color(x, x, x, x));
    }

}
