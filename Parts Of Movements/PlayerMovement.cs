using System.Collections;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField]  Player player;

    [SerializeField] LayerMask wallLayerMask;


    [SerializeField] public Transform head;


    //[SerializeField]
    //public CharacterController characterControler;

    [SerializeField] public CharacterController controller;

    public bool IsGrounded
    {
        get
        {
            return controller.isGrounded;
        }
    }

   


    public Animator animator;

    [SerializeField] CameraSlidingMove cameraSliding;


    [Space(10)]

    bool isSliging;

    public bool IsSliding
    {
        get { return isSliging; }
        set
        {
            isSliging = value;
            HandsObiect.Instance.walkingAnimation.GoToSliding(value);
        }
    }

    #region crounching
    bool isCrounching = false;

    [SerializeField] float standardHeight;
    [SerializeField] float crounchHeight;

    Coroutine CheckingUp;

    IEnumerator CheckCrounchingUp()
    {
        while(1 != 2)
        {
            if (!Physics.SphereCast(this.transform.position, controller.radius, transform.up, out RaycastHit hit, standardHeight/2, layerMask: wallLayerMask))
            {
                controller.height = standardHeight;
                controller.center = new Vector3(0, 0, 0);
                isCrounching = false;
                PlayerEvents.Crounch(false);
                break;
            }

            yield return new WaitForEndOfFrame();
        }
    }

    public bool IsCounching
    {
        get
        {
            return isCrounching;
        }

        set
        {
            if (value)
            {
                float _center = -(standardHeight / 2) + crounchHeight/2;
                controller.height = crounchHeight;
                controller.center = new Vector3(0, _center, 0);
                PlayerEvents.Crounch(value);
                isCrounching = value;

                if(CheckingUp != null)
                StopCoroutine(CheckingUp);
                CheckingUp = null;
            }
            else
            {
                CheckingUp = StartCoroutine(CheckCrounchingUp());
            }

        }
    }
    #endregion

    public bool isSepcialJump;

    public bool isWallClip = false;
    public bool climbingUp = false;
    public bool isFirstMomentUp = true;

    [Space(4)]
    [Header("movement stats")]
    public float moveSpeed;
    public float runSpeed;
    public float jumpStrenht;

    public int flyingJumps;
    public int maxFlyingJumps;
    public float jumpDashStrenght;

    public float dragEnd = 2;                       //if moveSpeed < dragEnd ---> moveSpeed = 0

    [SerializeField] float gravityDrag = 22;

    #region camera movement
    [Space(4)]
    [Header("camera movement")]
    public float xCamera;
    public float yCamera;
    float xMouseMove;
    float yMouseMove;

    float lastCameraX;
    float lastCameraY;
    public float cameraResetSens;

    public float cameraSpeed = 2.5f;

    #endregion

    bool yCameraUppDown;
    bool xCameraUppDown;


    [SerializeField] float timeBetwenJump = 0.3f;


    [HideInInspector] public float currentTimeBetwenJump;



    [SerializeField] public Vector3 velocity = new Vector3();


    [SerializeField] public Transform bodyRotation;

    public AudioManager audioManager;


    Vector3 keyboardVector = new Vector3();


    public float timeToStopMoving = 0.2f;

    /// <summary>
    /// time to stop sliding from run speed
    /// </summary>
    public float timeToStopSliding = 2f;


    private void Start()
    {
        player = GetComponent<Player>();
    }


    void Update()
    {
        if (!player.isAlive || !player.isLocalPlayer || !HandsObiect.Instance)
            return;



        if (Input.GetKey(KeyCode.LeftControl))
        {
            UpdateCrouching(true);
        }

        if (Input.GetKeyUp(KeyCode.LeftControl))
        {
            UpdateCrouching(false);
        }


        Drag();

        if (currentTimeBetwenJump > 0)
        {
            currentTimeBetwenJump -= Time.deltaTime;
        }
        else
        {
            if (Input.GetKeyDown("space"))
            {
                Jump();
            }
        }


        MouseMovement();
        SetKeyboardVelocity();
        UpdateCharacterControlerVelocity();
    }



    void MouseMovement()
    {
        xCamera += cameraSpeed * Input.GetAxis("Mouse X");
        yCamera -= cameraSpeed * Input.GetAxis("Mouse Y");

        xMouseMove += cameraSpeed * Input.GetAxis("Mouse X");
        yMouseMove -= cameraSpeed * Input.GetAxis("Mouse Y");


        if (!isWallClip && !IsSliding)
        {
            if (IsGrounded || (!IsGrounded && !Input.GetKeyDown(KeyCode.LeftControl))) // obracaie w locie
            {
                transform.eulerAngles = new Vector3(0, xCamera, 0);
                head.localEulerAngles = new Vector3(yCamera, 0, bodyRotation.localEulerAngles.z);
            }
        }
        else
        {
            head.eulerAngles = new Vector3(yCamera, xCamera, bodyRotation.localEulerAngles.z);
        }



        // camera breake aiming
        if (yCameraUppDown && Input.GetAxis("Mouse Y") < 0)
        {
            yCameraUppDown = false;
            lastCameraY = yMouseMove;
        }
        else
        {
            if (!yCameraUppDown && Input.GetAxis("Mouse Y") > 0)
            {
                yCameraUppDown = true;
                lastCameraY = yMouseMove;
            }
        }

        if (xCameraUppDown && Input.GetAxis("Mouse X") < 0)
        {
            xCameraUppDown = false;
            lastCameraX = xMouseMove;
        }
        else
        {
            if (!xCameraUppDown && Input.GetAxis("Mouse X") > 0)
            {
                xCameraUppDown = true;
                lastCameraX = xMouseMove;
            }
        }


        if (Mathf.Abs(yMouseMove - lastCameraY) > cameraResetSens || Mathf.Abs(xMouseMove - lastCameraX) > cameraResetSens)
        {
            lastCameraY = yMouseMove;
            lastCameraX = xMouseMove;
            player.cameraMovement.Break_Falling();
        }
    }


    void Jump()
    {
        if (IsGrounded)
        {
            currentTimeBetwenJump = timeBetwenJump;
            velocity.y = jumpStrenht;

            flyingJumps = maxFlyingJumps;

            audioManager.JumpStart();
            audioManager.PlayRunning(false);

            
        }
        else
        {
            if (isWallClip)
            {
                audioManager.JumpStart();

                float cameraAngle = head.transform.localEulerAngles.y;
                this.transform.eulerAngles = new Vector3(0, xCamera, 0);
                head.transform.localEulerAngles = new Vector3(yCamera, 0, 0);


                if (cameraAngle <= 45 || cameraAngle >= 315) // upp climbing
                {
                    // velocity.z = -jumpDashStrenght * 2;
                }
                else
                {
                    if (cameraAngle > 45 && cameraAngle <= 180)
                    {
                        velocity.x = jumpDashStrenght;
                    }
                    else
                    {
                        velocity.x = -jumpDashStrenght;
                    }
                }

                velocity.y = jumpStrenht;
                velocity.z = runSpeed + jumpDashStrenght;


                //Debug.Log("jump on wal");

                isWallClip = false;
            }
            else //jump in air
            {
                audioManager.JumpStart();
                if (flyingJumps > 0)
                {
                    this.transform.eulerAngles = new Vector3(0, xCamera, 0);
                    head.transform.localEulerAngles = new Vector3(yCamera, 0, 0);

                    flyingJumps--;
                    velocity.y = jumpStrenht;
                    velocity.z += 4;
                }
            }
        }


        PlayerEvents.JumpUp();
    }


    /// <summary>
    /// set keyboard dirrection and moving speed
    /// </summary>
    private void SetKeyboardVelocity()
    {
        if (isSepcialJump)
            return;

        keyboardVector = Vector3.zero;
        if (climbingUp || IsSliding || isWallClip)
        {
            return;
        }

        if (Input.GetKey(KeyCode.W))
            keyboardVector.z = 1;
        if (Input.GetKey(KeyCode.S))
            keyboardVector.z = -1;

        if (Input.GetKey(KeyCode.D))
            keyboardVector.x = 1;
        if (Input.GetKey(KeyCode.A))
            keyboardVector.x = -1;

        keyboardVector = keyboardVector.normalized;

        float _speed = moveSpeed;

        if (IsCounching)
            _speed = moveSpeed * 0.7f;

        if (player.isAiming)
            _speed = _speed * 0.7f;

        if (Input.GetKey(KeyCode.LeftShift))
            _speed = runSpeed;


        if (IsGrounded) // to do movement w powietrzu
        {

        }
        else
        {

        }

        if (IsGrounded) // to do nie wiem gdzie to wsadziæ jeszcze
        {
            if (!isFirstMomentUp)  // touch ground
            {
                isFirstMomentUp = true;
                if (IsCounching)
                    velocity.z += 2;
            }
        }
        else
        {
            if (isFirstMomentUp) // go to air
                isFirstMomentUp = false;
        }



        if(keyboardVector != Vector3.zero)
        keyboardVector  = transform.rotation * keyboardVector * _speed;
    }


    /// <summary>
    /// camouflaging moving vorce
    /// </summary>
    public void Drag()
    {
        if (isSepcialJump)
            return;

        if (IsGrounded)
        {

            if(keyboardVector.magnitude > 0)
            {
                velocity = keyboardVector;
                velocity.y = -3;
            }
            else
            {
                if (IsSliding)
                {
                    if (velocity.magnitude < dragEnd)
                    {
                        velocity.x = 0;
                        velocity.z = 0;
                        velocity.y = -3;
                        IsSliding = false;
                        return;
                    }

                    velocity -= (velocity.normalized * runSpeed) * (1 / timeToStopSliding) * Time.deltaTime;
                    velocity.y = -3;
                    return;
                }


                velocity -= velocity * (1/timeToStopMoving) * Time.deltaTime;

                velocity.y = -3;
            }
        }
        else
        {


            if (isWallClip)
            {
                if (velocity.y > 0)
                    velocity.y -= gravityDrag * Time.deltaTime;
                else
                    velocity.y = 0;
            }
            else
            {
                if (velocity.y > -18)
                    velocity.y -= gravityDrag * Time.deltaTime;
            }
        }
    }


    void UpdateCrouching(bool _startCrouching) // toDo: change this to IsCrounching
    {
        if (_startCrouching)
            if (IsGrounded)
            {
                if (IsCounching == false)
                {
                    IsCounching = true;

                    if (isWallClip)
                    {
                        isWallClip = false;
                    }
                }

                if (isSliging == false)
                {
                    if (velocity.z > moveSpeed)
                    {

                        IsSliding = true;
                    }
                }
            }

        if (_startCrouching == false)
        {

            IsCounching = false;
            IsSliding = false;
        }
    }


    void UpdateCharacterControlerVelocity()
    {
        controller.Move(velocity * Time.deltaTime * 1.5f);
    }

    /// <summary>
    /// force wen you shoot gun
    /// </summary>
    /// <param name="strength"></param>
    public void GunKickForceBack(float strength)
    {
        if (!IsGrounded)
        {
            velocity -= strength * transform.forward;
        }
        else
        {
            velocity -= (strength / 3) * transform.forward;
        }
    }

}


