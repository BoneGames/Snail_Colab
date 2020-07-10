using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PopDestroyBubble : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Snail")
        {
            GetComponentInParent<BubbleFlight>().DestroyBubble();
        }
    }
}
