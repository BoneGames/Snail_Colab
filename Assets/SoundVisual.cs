using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering.PostProcessing;


public class SoundVisual : MonoBehaviour
{
    public bool randomStartTime;
    public Color zoneCol;
    public AudioSource aS;
    Renderer rend;
    public StatAdjustComponents volDisplayComponents;
    //public UiAdjust volDisplay;

    public ColorGrading cG;
    public PostProcessProfile ppV;
    Collider col;
    Snail_Controller snail;
    public bool snailInside;
    UI_Manager ui;

    public string id;
    // Start is called before the first frame update
    void Start()
    {
        zoneCol = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f), .4f);
        id = transform.parent.name + "_" + transform.name;
        ui = FindObjectOfType<UI_Manager>();
        snail = FindObjectOfType<Snail_Controller>();
        col = GetComponent<Collider>();
        cG = GetCG();

        aS = GetComponent<AudioSource>();
        if (randomStartTime)
        {
            SetRandomStartTime();
        }

        rend = GetComponent<Renderer>();

        SetSize();
        SetCol(true);
    }

    ColorGrading GetCG()
    {
        ColorGrading _cg;
        foreach (var item in ppV.settings)
        {
            _cg = item as ColorGrading;
            if (_cg != null)
            {
                return _cg;
            }
        }
        return null;
    }

    public void InitVolDisplay(StatAdjustComponents _components)
    {
        volDisplayComponents = _components;
        SetVolumeDisplay();
    }

    public void SetVolumeDisplay()
    {
        if (volDisplayComponents)
        {
            volDisplayComponents.value.text = aS.volume.ToString();
        }
        else
        {
            Debug.Log(this.name + " does not have its adjustment components initialised");
        }
    }

    void SetRandomStartTime()
    {
        float startTime = Random.Range(0, aS.clip.length);
        aS.time = startTime;
    }

    void SetSize()
    {
        float radius = aS.maxDistance;
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
        if (!snailInside)
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
        if (other.tag == "Snail")
        {
            // Debug.Log(id + ", Soundscape UI added");
            snailInside = true;
            ui.AddNewSoundScape(this);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Snail")
        {
            // Debug.Log(id + ", Soundscape UI removed");
            snailInside = false;
            ui.RemoveActiveSS(this);
        }
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
