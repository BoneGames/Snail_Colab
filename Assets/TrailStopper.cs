using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrailStopper : MonoBehaviour
{
    public TrailRenderer trail;
    public Transform otherTrailRend, a, b;
    public bool rightSide;
    public float angleLimit;
    public Vector3 lastPos;
    public float aAngle, bAngle;
    // Start is called before the first frame update
    void Awake()
    {
        trail = GetComponent<TrailRenderer>();
        foreach (var item in FindObjectsOfType<TrailRenderer>())
        {
            if (item != trail)
            {
                otherTrailRend = item.transform;
                break;
            }
        }

        foreach (Transform item in transform)
        {
            if(item.name == "a")
            {
                a = item;
            }
            else if(item.name == "b")
            {
                b = item;
            }
        }

        aAngle = Vector3.Angle((otherTrailRend.position - transform.position).normalized, (a.position - transform.position).normalized);
        bAngle = Vector3.Angle((otherTrailRend.position - transform.position).normalized, (b.position - transform.position).normalized);

    }
    Vector3 dirToOther, moveDir;
    // Update is called once per frame
    void Update()
    {
        Vector3 currPos = transform.position;

        dirToOther = (otherTrailRend.position - currPos).normalized;
        moveDir = (lastPos - transform.position).normalized;

        float bearing = Vector3.Angle(dirToOther, moveDir);

        // moving forwards
        bool use_A_Angle = Vector3.Distance(lastPos, a.position) < Vector3.Distance(currPos, a.position);


        angleLimit = use_A_Angle ? aAngle : bAngle;



        trail.emitting = bearing < angleLimit ? false : true;
        // if(angle > angleLimit)
        //Xvelocity = rigid.velocity.x;
        //if (rightSide)
        //{
        //    trail.emitting = Xvelocity > 0 ? true : false;
        //}
        //else
        //{
        //    trail.emitting = Xvelocity < 0 ? true : false;
        //}

        lastPos = transform.position;
    }

    private void OnDrawGizmos()
    {
        Debug.DrawLine(transform.position, transform.position + dirToOther);
        Debug.DrawLine(transform.position, transform.position + moveDir);
    }
}
