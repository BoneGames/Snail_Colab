using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

public class BubbleFlight : MonoBehaviour
{

    public bool snailRider => transform.childCount > 1;


    public GameObject popFX;
    Rigidbody rigid;
    Collider col;
    [HideInInspector]
    public Transform snailSitSpot;
    // public Vector3 extents;


    // spiral flight
    // variables
    public bool applyNoise, applyMoreNoise, beyondSnailRideRange;
    public float xZSpeed, ySpeed, maxRadius, radiusGrowthRate1, radiusGrowthRate2;
    float spiralRadius;

    // init values
    float x = 0;
    float z = 0;
    float tXZ = 0;
    float tY = 0;
    float growingRadius = 0;
    float xNoiseSeed;
    float zNoiseSeed;


    private void Start()
    {
        col = GetComponent<Collider>();
        snailSitSpot = transform.GetChild(0);
        xNoiseSeed = Random.Range(0f, 1000f);
        zNoiseSeed = Random.Range(0f, 1000f);
        rigid = GetComponent<Rigidbody>();
        GameObject _pop = Instantiate(popFX, transform.position + Vector3.up, Quaternion.identity);
        _pop.name = "BubblePopFX";
        beyondSnailRideRange = false;
    }

    public void OnSpawn(BubbleSpawn _spawner)
    {
        // create name
        this.name = "Bubble_" + _spawner.bubbleIndex;

        // set speeds
        xZSpeed = _spawner.bubbleSpiralSpeed;
        ySpeed = _spawner.riseSpeed;
    }

    // Update is called once per frame
    void Update()
    {
        SpiralUp();
    }

    void DestroyBubble()
    {
        //// bubble pop FX
        //GameObject _pop = Instantiate(popFX, transform.position, Quaternion.identity);
        //_pop.name = "Death_POP";
        //// remove snail
        //if (snailRider)
        //{
        //    transform.GetComponentInChildren<Snail_Controller>().transform.parent = null;
        //}
        // destroy bubble
        Destroy(gameObject);
    }

    void SpiralUp()
    {
        // current spiral radisu
        spiralRadius = new Vector3(transform.localPosition.x, 0, transform.localPosition.z).magnitude;

        // circle equation
        x = growingRadius * Mathf.Cos(tXZ);
        z = growingRadius * Mathf.Sin(tXZ);

        tXZ += Time.deltaTime * xZSpeed;
        tY += Time.deltaTime * ySpeed;

        // increment radius
        if ((spiralRadius < maxRadius && growingRadius < maxRadius))
        {
            growingRadius += Time.deltaTime * radiusGrowthRate1;

        }
        else if(beyondSnailRideRange)
        {
            growingRadius += Time.deltaTime * radiusGrowthRate2;
        }
        // apply noise
        if (applyNoise)
        {
            float xNoise = Mathf.Clamp(Mathf.PerlinNoise(xNoiseSeed + tXZ, 0), 0.1f, 1f);
            x *= xNoise;
        }
        if (applyMoreNoise)
        {
            float zNoise = Mathf.Clamp(Mathf.PerlinNoise(zNoiseSeed + tXZ, 0), 0.1f, 1f);
            z *= applyMoreNoise ? zNoise : 1;
        }

        // apply transformation
        transform.localPosition = new Vector3(x, (tY * ySpeed) - 2f, z);
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.transform.tag == "Coral")
        {
            if (snailRider)
            {
                // Debug.Log("Jump Snail");
                GetComponentInChildren<Snail_Controller>().JumpOffBubble();
            }

            col.enabled = false;
            Destroy(rigid);
            beyondSnailRideRange = true;
        }
        else if (other.transform.tag == "DestroyBubble")
        {
            DestroyBubble();
        }
    }
}
