using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class SoundVisual : MonoBehaviour
{
    public bool randomStartTime;
    public Color zoneCol;
    public float radius;
    public AudioSource aS;
    public Renderer rend;
    public ColorGrading cG;
    public PostProcessProfile ppV;
    Collider col;
    Snail_Controller snail;
    // Start is called before the first frame update
    void Start()
    {
        snail = FindObjectOfType<Snail_Controller>();
        col = GetComponent<Collider>();
        foreach (var item in ppV.settings)
        {
            cG = item as ColorGrading;
            if (cG != null)
            {
                break;
            }
        }
        aS = GetComponent<AudioSource>();
        if(randomStartTime)
        {
            SetRandomStartTime();
        }

        rend = GetComponent<Renderer>();

        SetSize();
        SetCol(true);
    }

    void SetRandomStartTime()
    {
        float startTime = Random.Range(0,aS.clip.length);
        aS.time = startTime;
    }

    void SetSize()
    {
        radius = aS.maxDistance;
        transform.localScale = new Vector3(radius * 2, radius * 2, radius * 2);
    }

    public void SetCol(bool on)
    {
        if (on)
        {
            rend.material.color = zoneCol;
        }
        else
        {
            rend.material.color = new Color(0, 0, 0, 0);
        }
    }

    bool ColliderInside()
    {
        foreach (var colCorner in snail.colliderCorners)
        {
            if (col.bounds.Contains(colCorner))
            {
                continue;
            }
            else
            {
                return false;
            }
        }
        return true;
    }

    private void Update()
    {
        if (!entered)
            return;

        if (ColliderInside())
        {
            SetColorFilter(true);
        }
        else
        {
            SetColorFilter(false);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        //if(other.transform.tag == "Snail")
        //{
        //    cG.colorFilter.value = zoneCol;
        //    cG.colorFilter.overrideState = true;
        //    Debug.Log("ColorSet");
        //}
        entered = true;
    }
    bool entered;
    private void OnTriggerExit(Collider other)
    {
        entered = false;
        //if (cG.colorFilter.value == zoneCol)
        //{
        //    cG.colorFilter.overrideState = false;
        //    cG.colorFilter.
        //    Debug.Log("out of color filter");
        //}
    }

    public void SetColorFilter(bool on)
    {
        if (on)
        {
            cG.colorFilter.value = zoneCol;
            if (!cG.colorFilter.overrideState)
                Debug.Log("Color Filter Enabled");
            cG.colorFilter.overrideState = true;
        }
        else
        {
            if (cG.colorFilter.overrideState)
                Debug.Log("Color Filter disabled");
            cG.colorFilter.overrideState = false;
        }
    }

}
