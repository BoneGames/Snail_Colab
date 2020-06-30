using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

public class SpiralTest : MonoBehaviour
{
    public bool spiralling, applyNoise, applyMoreNoise;
    public float xZSpeed, ySpeed, maxRadius, radiusGrowthRate;
    float spiralRadius;

    public void StartSpiral()
    {
        StartCoroutine(Spiral());
    }

    IEnumerator Spiral()
    {
        float x = 0;
        float z = 0;
        float t = 0;

        float growingRadius = 0;

        float xNoiseSeed = Random.Range(0f, 1000f); //GetZeroNoiseSeed();
        float zNoiseSeed = Random.Range(0f, 1000f);

        while (spiralling)
        {
            // current spiral radisu
            spiralRadius = new Vector3(transform.localPosition.x, 0, transform.localPosition.z).magnitude;

            // circle equation
            x = growingRadius * Mathf.Cos(t);
            z = growingRadius * Mathf.Sin(t);

            t += Time.deltaTime * xZSpeed;

            // increment radius
            if (spiralRadius < maxRadius && growingRadius < maxRadius)
                growingRadius += Time.deltaTime * radiusGrowthRate;

            // apply noise
            if (applyNoise)
            {
                float xNoise = Mathf.Clamp(Mathf.PerlinNoise(xNoiseSeed + t, 0), 0.1f, 1f);
                x *= xNoise;
            }
            if (applyMoreNoise)
            {
                float zNoise = Mathf.Clamp(Mathf.PerlinNoise(zNoiseSeed + t, 0), 0.1f, 1f);
                z *= applyMoreNoise ? zNoise : 1;
            }

            // apply transformation
            transform.localPosition = new Vector3(x, t * ySpeed, z);

            yield return null;
        }
    }
}
