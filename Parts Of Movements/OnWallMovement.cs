using UnityEngine;

public class OnWallMovement : MonoBehaviour
{

    public GameObject cube;

    public Transform spawn_place;

    public PlayerMovement movement;

    public float headZDistance;            //distance from wall to hed when player is on wall
    float headZcurrentPosition;


    float antyClipping = 0;

    Vector3 vinalBodyRotation;
    Vector3 currentBodyRotation;


    public float bodyRotationSpeed = 0.125f;
    int wallMask;

    float CameraAngle
    {
        get
        {
            return movement.head.transform.localEulerAngles.y;
        }
    }


    enum WallSide
    {
        noOne,
        front,
        left,
        right
    }

    WallSide wallSide = WallSide.noOne;


    private void OnEnable()
    {
        PlayerEvents.OnJump += UnclipWall;
    }

    private void OnDisable()
    {
        PlayerEvents.OnJump -= UnclipWall;
    }

    private void Start()
    {
        wallMask = LayerMask.GetMask("Wall", "MapsElement", "HitElement"); //to do set this in inspector
    }

    void Update()
    {
        if (movement.isSepcialJump)
            return;

        BodyRotation();

        if (antyClipping >= 0)
        {
            antyClipping -= Time.deltaTime;
        }


        if (!movement.isWallClip)
        {
            if (antyClipping <= 0)
            {
                if (Input.GetKey(KeyCode.W))
                {
                    float _wallSide = WallDetectorAngle();

                    if (Input.GetKey(KeyCode.W) && _wallSide == 0)
                    {
                        Player.Instance.cameraMovement.Shake(new Vector3(3, 3, 3), new Vector3(12, 1, 1));
                        HandsObiect.Instance.handsMoving.JumpShake(new Vector3(20, 20, 20), Vector3.zero);
                        wallSide = WallSide.front;
                        movement.isWallClip = true;
                        ClipToWall(transform.forward, 3);

                        PlayerEvents.ClimbingUp(true);
                    }

                    if (Input.GetKey(KeyCode.A) && _wallSide == 270)
                    {
                        Player.Instance.cameraMovement.Shake(new Vector3(3, 3, 3), new Vector3(12, 1, 1));
                        HandsObiect.Instance.handsMoving.JumpShake(new Vector3(20, 20, 20), Vector3.zero);
                        wallSide = WallSide.left;
                        movement.isWallClip = true;
                        ClipToWall(-transform.right, 1);

                        PlayerEvents.ClimbingUp(false);
                    }

                    if (Input.GetKey(KeyCode.D) && _wallSide == 90)
                    {
                        Player.Instance.cameraMovement.Shake(new Vector3(3, 3, 3), new Vector3(12, 1, 1));
                        HandsObiect.Instance.handsMoving.JumpShake(new Vector3(20, 20, 20), Vector3.zero);
                        wallSide = WallSide.right;
                        movement.isWallClip = true;
                        ClipToWall(transform.right, 1);

                        PlayerEvents.ClimbingUp(false);
                    }
                }

            }
        }
        else
        {

            if (Input.GetKey(KeyCode.W))
            {
                ClipToWall(transform.forward, 3);

                if (wallSide == WallSide.front)
                    if (CameraAngle <= 60 || CameraAngle >= 300) // upp climbing
                    {

                        vinalBodyRotation.x = -35;
                        vinalBodyRotation.z = 0;

                        currentBodyRotation.y = 0;


                        headZcurrentPosition = headZDistance;

                        movement.velocity = transform.forward * 3;
                        movement.velocity.y = movement.moveSpeed * 0.8f; 
                        
                        return;
                    }
                    else
                    {
                        PlayerEvents.ClimbingUp(false);
                        UnclipWall();
                        return;
                    }


                if (wallSide == WallSide.left)
                    if (CameraAngle <= 220 && CameraAngle > 60)
                    {

                        vinalBodyRotation.z = -20f;
                        //finalBodyRotationX = -20f;
                        currentBodyRotation.y = 90;

                        movement.velocity = transform.right * movement.runSpeed + transform.forward * 3;
                        movement.velocity.y = 0;
                        headZcurrentPosition = headZDistance;
                        return;
                    }
                    else
                    {
                        if (CameraAngle <= 60)
                            wallSide = WallSide.front;

                        UnclipWall();
                        return;
                    }



                if (wallSide == WallSide.right)
                    if (CameraAngle >= 150 && CameraAngle < 300)
                    {
                        vinalBodyRotation.z = 20f;
                        //finalBodyRotationX = -20f;\
                        currentBodyRotation.y = -90;

                        headZcurrentPosition = headZDistance;

                        movement.velocity = -transform.right * movement.runSpeed + transform.forward * 3;
                        movement.velocity.y = 0;

                        return;
                    }
                    else
                    {
                        if (CameraAngle >= 300)
                            wallSide = WallSide.front;

                        UnclipWall();
                        return;
                    }

            }
            else
            {
                PlayerEvents.ClimbingUp(false);
                UnclipWall();
            }
        }
    }

    void UnclipWall()
    {
        
        antyClipping = 0.25f;
        movement.isWallClip = false;
        headZcurrentPosition = 0;
        vinalBodyRotation.z = 0f;
        vinalBodyRotation.x = 0;
        //currentBodyRotation.y = 0;
        wallSide = WallSide.noOne;
    }



    float WallDetectorAngle()
    {

        float i;

        for (i = 0; i < 360; i += 90)
        {
            if (i == 180)
            {
                continue;
            }


            Vector3 curTrans = transform.forward;
            if (i == 90) curTrans = transform.right;
            else
            if (i == 270) curTrans = -transform.right;

            var hitRay = Physics.Raycast(transform.position, curTrans, out RaycastHit _hit, 1, wallMask);

            if (hitRay)
            {

                return i;

                /*
                if (!movement.wallClip && antyClipping < 0)
                {
                    //finalBodyRotationX = 0;
                    //movement.climbingUp = false;
                    //movement.bodyRotation.localEulerAngles = new Vector3(0, 0, 20);
                    finalBodyRotationZ = 20;
                    movement.velocity.x = 0;
                    movement.velocity.z = 0;
                    movement.wallClip = true;
                    //movement.runSide = true;
                    movement.flying_Jumps = movement.maxflyingJumps;
                }
                */


                //break;
            }
            else
            {
                if (i == 270)
                {
                    //movement.bodyRotation.localEulerAngles = new Vector3(0, 0, 0);
                    //finalBodyRotationZ = 0;
                    // movement.wallClip = false;
                    break;
                }
            }
        }

        return 360;

    }


    /// <summary>
    /// when you first time touch wall
    /// </summary>
    /// <param name="direction"></param>
    /// <param name="range"></param>
    void ClipToWall(Vector3 direction, float range)
    {
        var _hitRay = Physics.Raycast(transform.position, direction, out RaycastHit _hit, range, wallMask);
        if (_hitRay)
        {
            float _newYDegree = Vector3.SignedAngle(_hit.normal, transform.forward, Vector3.up) - 180;

            this.transform.Rotate(0, -_newYDegree, 0, Space.World);
            movement.head.transform.Rotate(0, _newYDegree, 0, Space.World);
        }
    }



    void BodyRotation()
    {
        if (currentBodyRotation.z != vinalBodyRotation.z)
        {
            currentBodyRotation.z = Mathf.Lerp(currentBodyRotation.z, vinalBodyRotation.z, bodyRotationSpeed);

        }
        else

        if (currentBodyRotation.x != vinalBodyRotation.x)
        {
            currentBodyRotation.x = Mathf.Lerp(currentBodyRotation.x, vinalBodyRotation.x, bodyRotationSpeed);

        }
        else
            return;

        movement.bodyRotation.localEulerAngles = new Vector3(currentBodyRotation.x, currentBodyRotation.y, currentBodyRotation.z);

        float zHeadPosition = Mathf.Lerp(movement.head.localPosition.z, headZcurrentPosition, bodyRotationSpeed);
        //movement.head.localPosition = Vector3.Lerp(movement.head.localPosition, new Vector3(0, movement.head.localPosition.y, headZcurrentPosition), bodyRotationSpeed);
        movement.head.localPosition = new Vector3(movement.head.localPosition.x, movement.head.localPosition.y, zHeadPosition);
    }



}
