using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BubbleSpawn : MonoBehaviour
{
    public GameObject bubble;
    public float spawnInterval_Base, bubbleSpeed_Base, riseSpeed_Base;
    public float spawnInterval, timer, bubbleSpeed, riseSpeed;
    public Vector2 sizeRange;
    public Transform a, b, c, landingZone, lZoneTransformPoint, bubbleParent;
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
        GameObject _b = Instantiate(bubble, transform.position + Vector3.down, Quaternion.identity);
        _b.GetComponent<BubbleFlight>().OnSpawn(this);
        _b.transform.parent = bubbleParent;
        bubbleIndex++;
        timer = 0;
    }

    public void ResetValues()
    {
        spawnInterval = spawnInterval_Base;
        bubbleSpeed = bubbleSpeed_Base;
        riseSpeed = riseSpeed_Base;
    }
}
