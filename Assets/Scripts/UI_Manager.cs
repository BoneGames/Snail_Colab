﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;

public class UI_Manager : MonoBehaviour
{

    public Snail_Controller snail;
    public BubbleSpawn bubbleSpawn;

    public Text ambientVolText;

    public Text moveText;
    public Text rotateText;
    public Text bubbleSpeedText;
    public Text bubbleIntervalText;
    public Text bubbleRiseSpeedText;
    public Image lerpCamButton, seeSoundZoneButton, seeVolAdjustButton;

    public bool allVolumesVisible;
    public GameObject statAdjust;
    public Transform volAdjustParent;
    public bool showSoundZones;
    public SoundVisual[] allSoundscapes;
    public List<SoundVisual> activeSoundscapes = new List<SoundVisual>();

    public AudioSource ambientVol;

    public VerticalLayoutGroup volLayout;


    void Start()
    {
        allVolumesVisible = false;
        //showSoundZones = true;
        allSoundscapes = FindObjectsOfType<SoundVisual>();
        snail = FindObjectOfType<Snail_Controller>();
        ShowColorSoundZones();
        //SetDisplays();
    }

    public void SeeAllSoundScapeVolumes()
    {
        if(allVolumesVisible)
        {
            volLayout.childForceExpandHeight = false;
            volLayout.childControlHeight = false;
        }

        if (!allVolumesVisible)
        {
            foreach (var item in allSoundscapes)
            {
                AddNewSoundScape(item);
            }
        }
        else
        {
            foreach (var item in allSoundscapes)
            {
                if(Vector3.Distance(item.transform.position, snail.transform.position) > item.aS.maxDistance)
                {
                    RemoveActiveSS(item);
                }
            }
        }

        if (!allVolumesVisible)
        {
            volLayout.childForceExpandHeight = true;
            volLayout.childControlHeight = true;
        }
        // volLayout.

        allVolumesVisible = !allVolumesVisible;
        SetDisplays();
    }

    public void AdjustAmbientVol(float change)
    {
        ambientVol.volume += change;
        SetDisplays();
    }

    public void AdjustAllVol(float adjustment)
    {
        foreach (var item in allSoundscapes)
        {
            AdjustVolume(item, adjustment);
        }
    }

    public void ResetAllVolumes()
    {
        foreach (var item in allSoundscapes)
        {
            AdjustVolume(item, 0, true);
        }
    }

    public void AddNewSoundScape(SoundVisual ss)
    {
        if (!activeSoundscapes.Contains(ss))
        {
            CreateVolAdjustUI(ss);
        }
        else
        {
            Debug.Log(ss.id + " already has a vol adjust UI");
        }
    }

    public void RemoveActiveSS(SoundVisual ss)
    {
        if (activeSoundscapes.Contains(ss))
        {
            activeSoundscapes.Remove(ss);
            Destroy(ss.volDisplayComponents.gameObject);
        }
        else
        {
            Debug.Log("tried to remove " + ss.name + ", but it was not in the collection");
        }
    }


    public void CreateVolAdjustUI(SoundVisual ss)
    {
        // create value UI prefab
        GameObject volAdjuster = Instantiate(statAdjust, volAdjustParent);

        StatAdjustComponents components = volAdjuster.GetComponent<StatAdjustComponents>();
        // set title text
        components.title.text = ss.id;

        Button down = components.down;
        Button up = components.up;

        // link buttons with correct component - addlistener with SoundVisual as paramater
        down.onClick.AddListener(() => AdjustVolume(ss, -0.05f));
        up.onClick.AddListener(() => AdjustVolume(ss, 0.05f));

        ss.InitVolDisplay(components);

        activeSoundscapes.Add(ss);
    }

    public void AdjustVolume(SoundVisual ss, float adjustment, bool reset = false)
    {
        if (reset)
        {
            ss.aS.volume = 1;
            ss.SetVolumeDisplay();
            return;
        }

        Debug.Log("adjust Vol");
        ss.aS.volume += adjustment;
        //ss.SetVolumeDisplay();
        SetDisplays();
    }

    void ShowColorSoundZones()
    {
        if (showSoundZones)
        {
            foreach (var item in allSoundscapes)
            {
                item.enabled = true;
                item.SetCol(true);
            }
        }
        else
        {
            foreach (var item in allSoundscapes)
            {
                item.SetColorFilter(false);
                item.SetCol(false);
                item.enabled = false;
            }
        }
        SetDisplays();
    }

    public void SwitchColorSoundZones()
    {
        showSoundZones = !showSoundZones;
        ShowColorSoundZones();
    }

    public void LerpOrSnapCam()
    {
        snail.camSwitchLerp = !snail.camSwitchLerp;
        SetDisplays();
    }

    void SetDisplays()
    {
        foreach (var item in activeSoundscapes)
        {
            item.SetVolumeDisplay();
        }
        moveText.text = snail.moveSpeed.ToString();
        bubbleRiseSpeedText.text = bubbleSpawn.riseSpeed.ToString();
        rotateText.text = snail.rotateSpeed.ToString();
        bubbleSpeedText.text = bubbleSpawn.bubbleSpiralSpeed.ToString();
        bubbleIntervalText.text = bubbleSpawn.spawnInterval.ToString();
        lerpCamButton.color = snail.camSwitchLerp ? new Color(Color.red.r, Color.red.g, Color.red.b, 0.5f) : new Color(Color.green.r, Color.green.g, Color.green.b, 0.5f);
        seeSoundZoneButton.color = showSoundZones ? new Color(Color.green.r, Color.green.g, Color.green.b, 0.5f) : new Color(Color.red.r, Color.red.g, Color.red.b, 0.5f);
        seeVolAdjustButton.color = allVolumesVisible ? new Color(Color.green.r, Color.green.g, Color.green.b, 0.5f) : new Color(Color.red.r, Color.red.g, Color.red.b, 0.5f);
        ambientVolText.text = ambientVol.volume.ToString();

    }

    public void SetMoveSpeed(float change)
    {
        snail.SetMoveSpeed(change);
        SetDisplays();
    }

    public void SetRotationSpeed(float change)
    {
        snail.SetRotationSpeed(change);
        SetDisplays();
    }

    public void ResetSnail()
    {
        snail.ResetPos();
        bubbleSpawn.ResetValues();
        SetDisplays();
    }

    public void SetBubbleInterval(float change)
    {
        bubbleSpawn.spawnInterval += change;

        SetDisplays();
    }

    public void SetBubbleRiseSpeed(float change)
    {
        bubbleSpawn.riseSpeed += change;
        foreach (var x in FindObjectsOfType<BubbleFlight>())
        {
            x.ySpeed += change;
        }
        SetDisplays();
    }

    public void SetBubbleSpeed(float change)
    {
        // set speed for future bubbles to be spawned
        bubbleSpawn.bubbleSpiralSpeed += change;
        // set speed for all bubble already in flight
        foreach (var x in FindObjectsOfType<BubbleFlight>())
        {
            x.xZSpeed += change;
        }
        SetDisplays();
    }
}
