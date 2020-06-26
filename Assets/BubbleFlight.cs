using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

public class BubbleFlight : MonoBehaviour
{
    public Transform landingZone, landingZoneTransformPoint;
    public float flyHeight, speed, heightRange, riseSpeed, adjustBubbleSpeed, adjustLandingPointSpeed;
    public Snail_Controller snailRider = null;
    public Transform a, b, c;
    public bool inCoral = true;
    public GameObject popFX;
    public Vector3 extents;

    private void Start()
    {
        GameObject _pop = Instantiate(popFX, transform.position + Vector3.up, Quaternion.identity);
        Debug.Log("spawnPop");
        _pop.name = "Birth";
    }

    public void OnSpawn(BubbleSpawn _spawner)
    {
        // create name
        this.name = "Bubble_" + _spawner.bubbleIndex;
        landingZone = _spawner.landingZone;
        landingZoneTransformPoint = _spawner.lZoneTransformPoint;
        // set bubble speed
        speed = _spawner.bubbleSpeed;
        riseSpeed = _spawner.riseSpeed;

        // create transform for start of lerp trajectory
        a = new GameObject().transform;
        a.position = _spawner.a.position;
        a.name = "Point_A (" + this.name + ")";
        a.parent = _spawner.bubbleParent;

        // transform that give top point for arc
        b = _spawner.b;

        // create end transform for lerp trajectory destination
        c = new GameObject().transform;
        c.position = _spawner.c.position;
        c.rotation = landingZone.rotation;
        c.name = "Point_C (" + this.name + ")";
        c.parent = _spawner.bubbleParent;

        // get landingZone extents
        Vector3 colExtents = landingZone.GetComponent<MeshFilter>().mesh.bounds.extents;
        extents = new Vector3(colExtents.x * landingZone.localScale.x, 0, colExtents.z * landingZone.localScale.z);

        inCoral = true;
    }
    public float posTrade;
    IEnumerator FlyOver()
    {
        // set first waypoint to current position
        a.position = transform.position;
        float timer = 0f;
        while (timer < 1f)
        {
            Vector3 AbLerp = Vector3.Lerp(a.position, b.position, timer);
            Vector3 BcLerp = Vector3.Lerp(b.position, c.position, timer);
            Vector3 AbBcLerp = Vector3.Lerp(AbLerp, BcLerp, timer);

            transform.position = Vector3.Lerp(AbBcLerp, transform.position, posTrade);

            timer += Time.deltaTime * speed * 0.25f;

            yield return null;
        }
        // Destroy Bubble On Arrival
        DestroyBubble();
    }

    Vector4 CanMoveDestinationPoint(Vector2 movement)
    {
        // bubble destination movement
        float x = 0;
        float y = 0;
        // bubble movement
        float z = 0;
        float w = 0;
    
        // X axis - moving right
        if (movement.x > 0)
        {
            if (c.InverseTransformPoint(landingZoneTransformPoint.position).x < (-extents.x))
            {
                x = 0;
                z = movement.x;
            }
            else
            {
                x = movement.x;
                z = 0;
            }
        }
        // X axis moving left
        else if(movement.x < 0)
        {
            if (c.InverseTransformPoint(landingZoneTransformPoint.position).x > (extents.x))
            {
                x = 0;
                z = movement.x;
            }
            else
            {
                x = movement.x;
                z = 0;
            }
        }
        // no x axis movement
        else
        {
            x = z = 0;
        }

        // Z axis - moving forward
        if (movement.y > 0)
        {
            if (c.InverseTransformPoint(landingZoneTransformPoint.position).z < (-extents.z))
            {
                y = 0;
                w = movement.y;
            }
            else
            {
                y = movement.y;
                w = 0;
            }
        }
        // Z axis moving back
        else if (movement.y < 0)
        {
            if (c.InverseTransformPoint(landingZoneTransformPoint.position).z > (extents.z))
            {
                y = 0;
                w = movement.y;
            }
            else
            {
                y = movement.y;
                w = 0;
            }
        }
        // no x maxis movement
        else
        {
            y = w = 0;
        }


        return new Vector4(x,y,z,w);
    }

    public void PickUpSnail(Snail_Controller snail)
    {
        snailRider = snail;
        Debug.Log("picked up snail");
    }

    public void DropSnail()
    {
        if (snailRider == null)
        {
            Debug.Log("tried to drop snail. No snail to drop");
            return;
        }
        snailRider = null;
    }

    // Update is called once per frame
    void Update()
    {
        if (inCoral)
        {
            Rise();
        }
        else if (snailRider)
        {
            ShiftBubble();
        }
    }

    void DestroyBubble()
    {
        // destroy lerp transforms
        Destroy(a.gameObject);
        Destroy(c.gameObject);
        // bubble pop FX
        GameObject _pop = Instantiate(popFX, transform.position, Quaternion.identity);
        _pop.name = "Death_POP";
        // remove snail
        if (snailRider)
        {
            snailRider.transform.parent = null;
            snailRider = null;
        }
        // destroy bubble
        Destroy(gameObject);
    }

    void ShiftBubble()
    {
        // get movement input
        Vector2 bubbleMovement = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));

        if (bubbleMovement.magnitude != 0)
        {
            // get clamped translations
            Vector4 translations = CanMoveDestinationPoint(bubbleMovement);
            // move landing pad
            c.Translate(new Vector3(translations.x, 0, translations.y) * Time.deltaTime * adjustLandingPointSpeed);
            // move bubble
            transform.Translate(new Vector3(translations.z, 0, translations.w) * Time.deltaTime * adjustBubbleSpeed);
        }
    }

    void Rise()
    {
        transform.Translate(Vector3.up * riseSpeed * Time.deltaTime);
    }

    void StartFlight()
    {
        inCoral = false;
        StartCoroutine(FlyOver());
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.transform.tag == "Coral")
        {
            StartFlight();
        }
    }
}
