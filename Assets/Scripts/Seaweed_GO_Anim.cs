using System.Collections.Generic;
using UnityEngine;

public class Seaweed_GO_Anim : MonoBehaviour
{
    // Start is called before the first frame update
    public float fps;
    float delay;
    float timer;
    List<GameObject> frames = new List<GameObject>();
    int currentFrame = 0;
    void Start()
    {
        GetAllFrames();
        delay = 1f / fps;
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

    // Update is called once per frame
    void Update()
    {
        timer -= Time.deltaTime;
        if (timer <= 0)
        {
            IncrementFrame();
            timer = delay;
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
