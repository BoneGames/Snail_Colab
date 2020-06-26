using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_Manager : MonoBehaviour
{
    public Snail_Controller snail;
    public BubbleSpawn bubbleSpawn;

    public Text moveText;
    public Text rotateText;
    public Text bubbleSpeedText;
    public Text bubbleIntervalText;
    public Text bubbleRiseSpeedText;
    public SoundVisual[] soundVisuals;
    public bool soundZones;
    public Image lerpCamButton, soundZoneButton;

    // Start is called before the first frame update
    void Start()
    {
        soundZones = true;
        soundVisuals = FindObjectsOfType<SoundVisual>();
        snail = FindObjectOfType<Snail_Controller>();
        SetDisplays();
    }

    public void ColorSoundZones()
    {
        soundZones = !soundZones;
        if (soundZones)
        {
            foreach (var item in soundVisuals)
            {
                item.enabled = true;
                item.SetCol(true);
            }
        }
        else
        {
            foreach (var item in soundVisuals)
            {
                item.SetColorFilter(false);
                item.SetCol(false);
                item.enabled = false;
            }
        }
        SetDisplays();
    }

    public void LerpOrSnapCam()
    {
        snail.camSwitchLerp = !snail.camSwitchLerp;
        SetDisplays();
    }

    void SetDisplays()
    {
        moveText.text = snail.moveSpeed.ToString();
        bubbleRiseSpeedText.text = bubbleSpawn.riseSpeed.ToString();
        rotateText.text = snail.rotateSpeed.ToString();
        bubbleSpeedText.text = bubbleSpawn.bubbleSpeed.ToString();
        bubbleIntervalText.text = bubbleSpawn.spawnInterval.ToString();
        lerpCamButton.color = snail.camSwitchLerp ? new Color(Color.red.r, Color.red.g, Color.red.b, 0.5f) : new Color(Color.green.r, Color.green.g, Color.green.b, 0.5f);
        soundZoneButton.color = soundZones ? new Color(Color.green.r, Color.green.g, Color.green.b, 0.5f) : new Color(Color.red.r, Color.red.g, Color.red.b, 0.5f);
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
            x.riseSpeed += change;
        }
        SetDisplays();
    }

    public void SetBubbleSpeed(float change)
    {
        // set speed for future bubbles to be spawned
        bubbleSpawn.bubbleSpeed += change;
        // set speed for all bubble already in flight
        foreach(var x in FindObjectsOfType<BubbleFlight>())
        {
            x.speed += change;
        }
        SetDisplays();
    }
}
