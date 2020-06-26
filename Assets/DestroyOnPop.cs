using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyOnPop : MonoBehaviour
{
    public float lifeTime;
    float timer = 0;
    // Start is called before the first frame update
    void Start()
    {
        timer = lifeTime;
    }

    // Update is called once per frame
    void Update()
    {
        timer -= Time.deltaTime;
        if(timer < 0)
        {
            Destroy(gameObject);
        }
    }
}
