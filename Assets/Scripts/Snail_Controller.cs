using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Snail_Controller : MonoBehaviour
{
    // Snail Movement
    [HideInInspector]
    public float moveSpeed, rotateSpeed;
    public float baseRotateSpeed, baseMoveSpeed;
    public Transform respawnPoint;

    // Cam View
    [HideInInspector]
    public Transform firstPersonView, thirdPersonView, camTrans;
    public float camSwitchSpeed;
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
    public KeyCode camSwitchKey, strafeKey1, strafeKey2;
    bool Strafe => Input.GetKey(strafeKey2) || Input.GetKey(strafeKey1);



    // Start is called before the first frame update
    void Start()
    {
        ResetPos();
        camTrans = Camera.main.transform;
        SetCamPos(true);

        col = GetComponent<Collider>();
        rotateSpeed = baseRotateSpeed;
        moveSpeed = baseMoveSpeed;
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

    private void OnCollisionEnter(Collision collision)
    {
        Transform t = collision.transform;
        if (t.tag == "Bubble")
        {
            t.GetComponent<BubbleFlight>().snailRider = this;
            transform.parent = collision.transform;
            Debug.Log("parented to bubble");
        }
        //Debug.Log(collision.transform.name);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "LandingPad")
        {
            transform.parent = null;
            other.GetComponent<BubbleFlight>().DropSnail();
            Debug.Log("unparented from bubble");
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        Transform t = collision.transform;
        if (t.tag == "Bubble")
        {
            transform.parent = null;
            t.GetComponent<BubbleFlight>().DropSnail();
            Debug.Log("unparented disconnect");
        }
    }

    // Update is called once per frame
    void Update()
    {
        Move();

        if (camSwitchLerpActive)
            return;
        //change cam pos
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
        if (!bubbleRiding)
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
    }
}
