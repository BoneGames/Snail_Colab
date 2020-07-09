using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BubbleSpawn : MonoBehaviour
{
    public GameObject bubble;
    public float spawnInterval_Base, bubbleSpiralSpeed_Base, riseSpeed_Base;
    [HideInInspector]
    public float spawnInterval, timer, bubbleSpiralSpeed, riseSpeed;
    public int bubbleIndex;

    // Start is called before the first frame update
    void Start()
    {
        ResetValues();

        bubbleIndex = 1;
    }

    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;
        if (timer > spawnInterval)
            SpawnBubble();
    }

    void SpawnBubble()
    {
        Vector3 spawnPos = transform.position + (Vector3.down * 100f);
        GameObject _b = Instantiate(bubble, spawnPos, Quaternion.identity);
        _b.GetComponent<BubbleFlight>().OnSpawn(this);
        _b.transform.parent = transform;
        bubbleIndex++;
        timer = 0;
    }

    public void ResetValues()
    {
        spawnInterval = spawnInterval_Base;
        bubbleSpiralSpeed = bubbleSpiralSpeed_Base;
        riseSpeed = riseSpeed_Base;
    }
}
