using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

public class Snail_Go_Anim : MonoBehaviour
{
    // Start is called before the first frame update
    public float baseFps;
    public float currentFps;
    float delay;
    public float timer;
    List<GameObject> frames = new List<GameObject>();
    int currentFrame = 0;

    public Snail_Controller controller;

    private void Awake()
    {
        controller = FindObjectOfType<Snail_Controller>();
    }

    void Start()
    {
        currentFps = 0;
        GetAllFrames();
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
            currentFrame = 0;
        }
        SetFrame();
    }


    bool stopMovecycleActive = false;
    bool movingLastFrame;
    // Update is called once per frame
    void Update()
    {
        //delay = 1f / fps;
        
        timer -= Time.deltaTime * currentFps;
        if (timer <= 0)
        {
            IncrementFrame();
            timer = 1;
        }

        // slow down entry point
        if (!controller.moving && movingLastFrame)
        {
            StopAllCoroutines();

           StartCoroutine(SmoothMoveCycle(true));
        }
        // speed up entry point
        else if (controller.moving && !movingLastFrame)
        {
            StopAllCoroutines();
           StartCoroutine(SmoothMoveCycle(false));
        }

        movingLastFrame = controller.moving;
    }
    // bool stoppingActive, startingActive;
    public float transitionRate;
    IEnumerator SmoothMoveCycle(bool stopping)
    {
        //Debug.LogError("Cycle entered");
        float start = currentFps;
        float end = stopping ? 0 : baseFps;
        float timer = stopping? 1- currentFps/baseFps : currentFps / baseFps;

        while (timer <= 1)
        {
            currentFps = Mathf.Lerp(start, end, timer);

            timer += Time.deltaTime * transitionRate;

            yield return null;
        }

        //Debug.LogError("cycle exited, took: " + timer + " seconds");
    }


    void AllOff()
    {
        foreach (Transform t in transform)
        {
            t.gameObject.SetActive(false);
        }
    }
}
