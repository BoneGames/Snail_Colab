using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

public class Snail_Go_Anim_2 : MonoBehaviour
{
    // Start is called before the first frame update
    public Snail_Anim_Controller animController;
    public int maxFps, minFps, standardFPS;
    public int currentFps;
    public bool stopAtEndOfCycle;
    public float timer;
    List<GameObject> frames = new List<GameObject>();
    int currentFrame = 0;

    public bool fastIn, fastOut, slowIn, slowOut;



    public Snail_Controller controller;

    private void Awake()
    {
        maxFps = currentFps;
        controller = FindObjectOfType<Snail_Controller>();
        animController = FindObjectOfType<Snail_Anim_Controller>();
        GetAllFrames();
    }

    public void PrepareToStop()
    {
        stopAtEndOfCycle = true;
        if(slowOut)
        {
            SetSpeed(minFps);
        }
        else if(fastOut)
        {
            SetSpeed(maxFps);
        }
        else
        {
            SetSpeed(standardFPS);
        }
    }

    void SetSpeed(int speed)
    {
        currentFps = speed;
    }

    private void OnEnable()
    {
        if (fastIn)
        {
            SetSpeed(maxFps);
        }
        else if(slowIn)
        {
            SetSpeed(minFps);
        }
        else
        {
            SetSpeed(standardFPS);
        }

        currentFrame = 0;
        stopAtEndOfCycle = false;
        timer = 1;
        SetFrame();
    }

    void GetAllFrames()
    {
        foreach (Transform item in transform)
        {
            if (item != transform)
            {
                frames.Add(item.gameObject);
            }
        }
    }

    void SetFrame()
    {
        AllOff();
        if(currentFrame == 0)
        {
            //Debug.Break();
        }
        frames[currentFrame].SetActive(true);
        // print("Active: " + currentFrame);
    }

    void IncrementFrame()
    {
        if (currentFrame < (frames.Count - 1))
        {
            currentFrame++;
        }
        else
        {
           // Debug.Break();
            if (stopAtEndOfCycle)
            {
                animController.OnTransition();
                return;
            }
            RestartCycle();
        }
        SetFrame();
    }

    void RestartCycle()
    {
        currentFrame = 0;
        SetSpeed(standardFPS);
    }


    // Update is called once per frame
    void LateUpdate()
    {
        //delay = 1f / fps;

        if (!animController.animate)
            return;

        timer -= Time.deltaTime * currentFps;
        if (timer <= 0)
        {
            IncrementFrame();
            timer = 1;
        }
    }

    void AllOff()
    {
        foreach (Transform t in transform)
        {
            t.gameObject.SetActive(false);
        }
    }
}
