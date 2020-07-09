using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class billboard : MonoBehaviour
{
    Transform snail;
    // Start is called before the first frame update
    void Start()
    {
        snail = FindObjectOfType<Snail_Controller>().transform;
    }

    // Update is called once per frame
    void Update()
    {
        Quaternion lookRot = Quaternion.LookRotation(snail.position- transform.position, Vector3.up);
        Vector3 lookAtSnail = new Vector3(0, lookRot.eulerAngles.y - 180, 0);
        transform.eulerAngles = lookAtSnail;
    }
}
