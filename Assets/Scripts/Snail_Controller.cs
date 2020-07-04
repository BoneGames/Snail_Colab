using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Snail_Controller : MonoBehaviour
{
    public enum MoveState
    {
        Standard,
        BubbleRise,
        GravityJump,
        FloatDown
    }
    // Snail Movement
    public MoveState moveState;
    [HideInInspector]
    public float moveSpeed, rotateSpeed;
    public float baseRotateSpeed, baseMoveSpeed;
    public Transform respawnPoint;
    Rigidbody rigid;
    public bool onBubble => transform.parent != null;

    // fly down
    public float floatJourneySpeed, adjustBubbleSpeed, adjustLandingPointSpeed;
    public Transform a, b, c;
    public Transform landingZone, landingZoneTransformPoint;
    public Vector3 extents;

    // Cam View
    [HideInInspector]
    public Transform firstPersonView, thirdPersonView, camTrans;
    public float camSwitchSpeed, jumpForce;
    // state tracker (1st or third person)
    public bool thirdPersonState;
    // lerp or snap when changing state?
    public bool camSwitchLerp;
    // is lerp active?
    public bool camSwitchLerpActive;

    // Status
    bool bubbleRiding => transform.parent != null;

    // Debugging
    Collider col;
    public List<Vector3> colliderCorners => GetColliderCorners();


    // Controls
    public KeyCode camSwitchKey, strafeKey1, strafeKey2, jump;
    bool Strafe => Input.GetKey(strafeKey2) || Input.GetKey(strafeKey1);



    private void Awake()
    {
        // get components
        col = GetComponent<Collider>();
        rigid = GetComponent<Rigidbody>();
        camTrans = Camera.main.transform;
    }

    void Start()
    {
        // init positions
        ResetPos();
        SetCamPos(true);

        // set move specs
        rotateSpeed = baseRotateSpeed;
        moveSpeed = baseMoveSpeed;

        // get landingZone extents
        Vector3 colExtents = landingZone.GetComponent<MeshFilter>().mesh.bounds.extents;
        extents = new Vector3(colExtents.x * landingZone.localScale.x, 0, colExtents.z * landingZone.localScale.z);
    }

    void Update()
    {
        if (moveState == MoveState.FloatDown)
            ShiftFloatDown();


        Move();

        SwitchCamView();
    }

    public float posTradeOff;
    IEnumerator FloatDown()
    {
        SetMoveState(MoveState.FloatDown);

        // set first waypoint to current position
        a.position = transform.position;
        float timer = 0f;
        while (timer < 1f)
        {
            Vector3 AbLerp = Vector3.Lerp(a.position, b.position, timer);
            Vector3 BcLerp = Vector3.Lerp(b.position, c.position, timer);
            Vector3 AbBcLerp = Vector3.Lerp(AbLerp, BcLerp, timer);

            transform.position = Vector3.Lerp(AbBcLerp, transform.position, posTradeOff);

            timer += Time.deltaTime * floatJourneySpeed * 0.25f;

            yield return null;
        }
        // Destroy Bubble On Arrival
        // DestroyBubble();
        StartCoroutine(TemporaryGravity(true));
    }

    void ShiftFloatDown()
    {
        // get movement input
        Vector2 floatMovement = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));

        if (floatMovement.magnitude != 0)
        {
            // get clamped translations
            Vector4 translations = CanMoveDestinationPoint(floatMovement);
            // move landing pad
            c.Translate(new Vector3(translations.x, 0, translations.y) * Time.deltaTime * adjustLandingPointSpeed);
            // move bubble
            transform.Translate(new Vector3(translations.z, 0, translations.w) * Time.deltaTime * adjustBubbleSpeed);
        }
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
        else if (movement.x < 0)
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


        return new Vector4(x, y, z, w);
    }

    public void JumpOffBubble()
    {
        UnparentSnail();

        StartCoroutine(TemporaryGravity(false));
        rigid.AddForce((Vector3.forward * 2 + Vector3.up) * jumpForce, ForceMode.Acceleration);

    }

    IEnumerator TemporaryGravity(bool toGround)
    {
        SetMoveState(MoveState.GravityJump);
        yield return null;

        if (!toGround)
        {
            while (rigid.velocity.y > 0)
            {
                yield return null;
            }

            // begin float down;
            StartCoroutine(FloatDown());
        }
    }

    void SetMoveState(MoveState newState)
    {
        Debug.Log(newState);
        switch (newState)
        {
            case MoveState.Standard:
                rigid.isKinematic = true;
                moveState = newState;
                break;

            case MoveState.BubbleRise:
                rigid.isKinematic = true;
                moveState = newState;
                break;

            case MoveState.GravityJump:
                rigid.isKinematic = false;
                moveState = newState;
                break;

            case MoveState.FloatDown:
                rigid.isKinematic = true;
                moveState = newState;
                break;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.transform.tag == "Bubble")
        {

            Transform sT = collision.transform;
            transform.parent = sT;
            SetMoveState(MoveState.BubbleRise);
            SnapToBubble();
            // Debug.Log("parented to bubble");
        }
        if (collision.transform.tag == "Ground")
        {
            rigid.isKinematic = true;
            SetMoveState(MoveState.Standard);
        }
        // Debug.Log(collision.transform.name);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "LandingPad")
        {
            UnparentSnail();
        }
    }

    public void SnapToBubble()
    {
        if (onBubble)
        {
            transform.localPosition = transform.parent.GetComponent<BubbleFlight>().snailSitSpot.position - transform.parent.position;
        }
        else
        {
            Debug.Log("Snail Not on Bubble. Can't snap to bubble position");
        }

    }

    void UnparentSnail()
    {
        if (transform.parent != null)
        {
            transform.parent = null;
            Debug.Log("unparented from bubble");
        }
        else
        {
            Debug.Log("tried to unparent from bubble while not parented to bubble");
        }
    }

    void SwitchCamView()
    {
        if (camSwitchLerpActive)
            return;

        if (Input.GetKeyDown(camSwitchKey))
        {
            Debug.Log("cam switch key pressed");

            if (camSwitchLerp)
            {
                StartCoroutine(ChangeCamView(!thirdPersonState));
            }
            else
            {
                SetCamPos(!thirdPersonState);
            }
        }
    }

    void SetCamPos(bool _thirdPerson)
    {
        if (_thirdPerson)
        {
            camTrans.position = thirdPersonView.position;
            camTrans.rotation = thirdPersonView.rotation;
        }
        else
        {
            camTrans.position = firstPersonView.position;
            camTrans.rotation = firstPersonView.rotation;
        }
        thirdPersonState = _thirdPerson;
    }

    void Move()
    {
        float vert = Input.GetAxis("Vertical");
        float hor = Input.GetAxis("Horizontal");

        Vector3 translation = Strafe ? new Vector3(hor, 0, vert) : new Vector3(0, 0, vert);
        Vector3 rotation = Strafe ? new Vector3() : new Vector3(0, hor, 0);

        if (moveState == MoveState.Standard)
            transform.Translate(translation * Time.deltaTime * moveSpeed);

        transform.Rotate(rotation * Time.deltaTime * rotateSpeed);
    }

    public void SetMoveSpeed(float _moveSpeed)
    {
        moveSpeed += _moveSpeed;
    }

    public void SetRotationSpeed(float _rotateSpeed)
    {
        rotateSpeed += _rotateSpeed;
    }

    IEnumerator ChangeCamView(bool toThirdP)
    {
        camSwitchLerpActive = true;

        float timer = 0;
        Vector3 startPos = toThirdP ? firstPersonView.position : thirdPersonView.position;
        Vector3 finishPos = toThirdP ? thirdPersonView.position : firstPersonView.position;

        Quaternion startRot = toThirdP ? firstPersonView.rotation : thirdPersonView.rotation;
        Quaternion finishRot = toThirdP ? thirdPersonView.rotation : firstPersonView.rotation;

        while (timer <= 1f)
        {
            Vector3 camPos = Vector3.Lerp(startPos, finishPos, timer);
            Quaternion camRot = Quaternion.Lerp(startRot, finishRot, timer);

            camTrans.position = camPos;
            camTrans.rotation = camRot;

            timer += Time.deltaTime * camSwitchSpeed;

            yield return null;
        }

        SetCamPos(toThirdP);
        camSwitchLerpActive = false;
    }

    public void ResetPos()
    {
        rotateSpeed = baseRotateSpeed;
        moveSpeed = baseMoveSpeed;
        transform.position = respawnPoint.position;
        transform.rotation = respawnPoint.rotation;
        SetMoveState(MoveState.GravityJump);
    }

    List<Vector3> GetColliderCorners()
    {
        List<Vector3> cC = new List<Vector3>();
        Vector3 boundPoint1 = col.bounds.min;
        Vector3 boundPoint2 = col.bounds.max;
        cC.Add(boundPoint1);
        cC.Add(boundPoint2);
        cC.Add(new Vector3(boundPoint1.x, boundPoint1.y, boundPoint2.z));
        cC.Add(new Vector3(boundPoint1.x, boundPoint2.y, boundPoint1.z));
        cC.Add(new Vector3(boundPoint2.x, boundPoint1.y, boundPoint1.z));
        cC.Add(new Vector3(boundPoint1.x, boundPoint2.y, boundPoint2.z));
        cC.Add(new Vector3(boundPoint2.x, boundPoint1.y, boundPoint2.z));
        cC.Add(new Vector3(boundPoint2.x, boundPoint2.y, boundPoint1.z));
        return cC;
    }
}
