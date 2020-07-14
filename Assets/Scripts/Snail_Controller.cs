//using DecalSystem;
using NaughtyAttributes;
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
    public bool placePlantMode;
    public Decal_Client decals;
    // Snail Movement
    public MoveState moveState;
    //[HideInInspector]
    public float moveSpeed, rotateSpeed;
    public float baseRotateSpeed, baseMoveSpeed;
    public Transform respawnPoint;
    Rigidbody rigid;
    public bool onBubble => transform.parent != null;

    // fly down
    public float floatJourneySpeed, adjustBubbleSpeed, adjustLandingPointSpeed;
    public Transform a, b, c;
    public Transform landingZone;//, landingZoneTransformPoint;
    public Vector3 extents;
    public TrailRenderer lRend1, lRend2;
    // Cam View
    //[HideInInspector]
    public Transform firstPersonView, thirdPersonViewPlay, thirdPersonViewEdit, camTrans, bodyBase;
    Snail_Anim_Controller anim_Contoller;
    public float camSwitchSpeed, jumpForce;
    // state tracker (1st or third person)
    public bool thirdPersonState;
    // lerp or snap when changing state?
    public bool camSwitchLerp;
    // is lerp active?
    public bool camSwitchLerpActive;


    // Debugging
    Collider col;
    public List<Vector3> colliderCorners => GetColliderCorners();

    public float posTradeOff;

    public float noiseScale, noiseDeltaRate;

    // Controls
    public KeyCode camSwitchKey, strafeKey1, strafeKey2, jump;
    bool Strafe => Input.GetKey(strafeKey2) || Input.GetKey(strafeKey1);

    public bool useStrafe;

    private void Awake()
    {
        // get components
        col = GetComponent<Collider>();
        anim_Contoller = GetComponentInChildren<Snail_Anim_Controller>();
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

        if (moving != movingLastFrame)
        {
            anim_Contoller.IdleOrMoving(moving);
        }

        movingLastFrame = moving;
    }

    bool movingLastFrame;

    IEnumerator FloatDown()
    {
        SetMoveState(MoveState.FloatDown);

        // set first waypoint to current position
        a.position = transform.position;
        float timer = 0f;

        float noiseSeed = Random.Range(0f, 10f);
        float bX = 0;
        float bStartX = Mathf.PerlinNoise(noiseSeed + timer, 0);
        b.position = new Vector3(bX, b.position.y, b.position.z);

        while (timer < 1f)
        {
            // add noise to bPos
            bX = Mathf.PerlinNoise(noiseSeed + (timer * noiseDeltaRate), 0) * noiseScale;
            b.position = new Vector3((bStartX - (noiseScale / 2f)) + bX, b.position.y, b.position.z);


            Vector3 AbLerp = Vector3.Lerp(a.position, b.position, timer);
            Vector3 BcLerp = Vector3.Lerp(b.position, c.position, timer);
            Vector3 AbBcLerp = Vector3.Lerp(AbLerp, BcLerp, timer);

            transform.position = Vector3.Lerp(AbBcLerp, transform.position, timer);

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
            if (c.InverseTransformPoint(landingZone.position).x < (-extents.x))
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
            if (c.InverseTransformPoint(landingZone.position).x > (extents.x))
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
            if (c.InverseTransformPoint(landingZone.position).z < (-extents.z))
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
            if (c.InverseTransformPoint(landingZone.position).z > (extents.z))
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

        Vector3 jumpDir = (Vector3.forward + Vector3.up * 1.5f) * jumpForce;
        SetMoveState(MoveState.GravityJump);
        rigid.AddForce(jumpDir, ForceMode.Impulse);
        StartCoroutine(TemporaryGravity(false));

    }


    IEnumerator TemporaryGravity(bool toGround)
    {
        SetMoveState(MoveState.GravityJump);
        yield return null;

        if (!toGround)
        {
            //Debug.LogError("pre while: " + rigid.velocity.y);
            while (rigid.velocity.y > 0)
            {
                //Debug.LogError("while: " + rigid.velocity.y);
                yield return null;
            }

            // begin float down;
            StartCoroutine(FloatDown());
        }
    }

    void SetMoveState(MoveState newState)
    {
        moveState = newState;
        switch (newState)
        {
            case MoveState.Standard:
                EnableTrails(false);
                rigid.isKinematic = true;
                anim_Contoller.IdleOrMoving(moving);
                break;

            case MoveState.BubbleRise:
                EnableTrails(false);
                rigid.isKinematic = true;
                anim_Contoller.PrepareNextAnimState(Snail_Anim_Controller.AnimState.Idle);
                break;

            case MoveState.GravityJump:
                EnableTrails(true);
                rigid.isKinematic = false;
                anim_Contoller.PrepareNextAnimState(Snail_Anim_Controller.AnimState.Fly);
                break;

            case MoveState.FloatDown:
                EnableTrails(true);
                rigid.isKinematic = true;
                anim_Contoller.PrepareNextAnimState(Snail_Anim_Controller.AnimState.Fly);
                break;
        }
    }

    void EnableTrails(bool enabled)
    {
        lRend1.enabled = enabled;
        lRend2.enabled = enabled;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.transform.tag == "Bubble" && moveState != MoveState.FloatDown)
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
        if (collision.transform.tag == "Rock" && moveState == MoveState.GravityJump)
        {

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
            transform.position = transform.parent.GetComponent<BubbleFlight>().snailSitSpot.position;
        }
        else
        {
            Debug.Log("Snail Not on Bubble. Can't snap to bubble position");
        }

    }
    public float collisionDist;
    public Transform collisionRaycastOrigin;
    bool CanMove(Vector3 translation)
    {
        if (Physics.Raycast(collisionRaycastOrigin.position, translation, out RaycastHit hit, collisionDist, 1 << 9))
        {
            //if (hit.transform == "Obstacle")
            //{
            Debug.DrawLine(transform.position, hit.point);
            // Debug.Log(hit.transform.name);
            return false;
            //}
        }
        return true;
    }

    void UnparentSnail()
    {
        if (transform.parent != null)
        {
            transform.parent = null;
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
            if (placePlantMode)
            {
                camTrans.position = thirdPersonViewEdit.position;
                camTrans.rotation = thirdPersonViewEdit.rotation;
            }
            else
            {
                camTrans.position = thirdPersonViewPlay.position;
                camTrans.rotation = thirdPersonViewPlay.rotation;
            }
        }
        else
        {
            camTrans.position = firstPersonView.position;
            camTrans.rotation = firstPersonView.rotation;
        }
        thirdPersonState = _thirdPerson;
    }
    public float heightError;
    public Vector3 lastTranslation;
    public bool moving;
    //public bool turning;
    void Move()
    {
        float forward = Input.GetAxis("Vertical");
        float hor = Input.GetAxis("Horizontal");

        Vector3 translation = new Vector3(); ;
        Vector3 rotation = new Vector3();

        if (useStrafe)
        {
            translation = Strafe ? new Vector3(hor, 0, forward) : new Vector3(0, 0, forward);
            rotation = Strafe ? new Vector3() : new Vector3(0, hor, 0);
        }
        else
        {
            translation = new Vector3(0, 0, forward);
            rotation = new Vector3(0, hor, 0);
        }


        bool movement = translation.sqrMagnitude != 0;

        if (moveState == MoveState.Standard)
        {
            if (movement && CanMove(transform.TransformDirection(translation)))
            {
                lastTranslation = translation * Time.deltaTime * moveSpeed;
                transform.Translate(lastTranslation);
                decals.CreateTrail(bodyBase.position, transform.forward);
            }
            else
            {
                lastTranslation = translation;
            }

            AdjustHeight();

        }

        bool turning = rotation.sqrMagnitude != 0;

        if (turning)
            transform.Rotate(rotation * Time.deltaTime * rotateSpeed);

        moving = movement;
    }

    void AdjustHeight()
    {
        Ray ray = new Ray(bodyBase.position + new Vector3(0, 2, 0), Vector3.down * 5);

        if (Physics.Raycast(ray, out RaycastHit hit, 5f, 1 << 8))
        {
            Vector3 distVector = hit.point - bodyBase.position;
            float distSqr = distVector.sqrMagnitude;
            if (distSqr > (heightError * heightError))
            {
                transform.Translate(distVector * Time.deltaTime);
                // print(distVector);
            }
        }
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
        Vector3 startPos = toThirdP ? firstPersonView.position : thirdPersonViewPlay.position;
        Vector3 finishPos = toThirdP ? thirdPersonViewPlay.position : firstPersonView.position;

        Quaternion startRot = toThirdP ? firstPersonView.rotation : thirdPersonViewPlay.rotation;
        Quaternion finishRot = toThirdP ? thirdPersonViewPlay.rotation : firstPersonView.rotation;

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
        StopAllCoroutines();
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
