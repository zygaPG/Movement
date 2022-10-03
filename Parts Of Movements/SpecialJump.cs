using System.Collections;
using UnityEngine;
using DG.Tweening;


// jump above small wall/ window 
// find closest edge and jump to moment when your y possition == y possition of point on this edge
public class SpecialJump : MonoBehaviour
{
    [SerializeField] LayerMask layerMask;
    [SerializeField] Transform upRaycastStart;
    [SerializeField] float raycastLength;


    Vector3 pointOnEdge;

    [SerializeField] GameObject marker;


    [SerializeField] PlayerMovement playerMovement;

    [SerializeField] Transform head;
    [SerializeField] Vector3 headFlippRotation = new Vector3();

    bool isJumpingNow = false;
    float endYpossition = 0;

    [SerializeField] float climbingSpeed = 2;


    private void FixedUpdate()
    {
        if (Physics.Raycast(upRaycastStart.position, -transform.up, out RaycastHit _hit, raycastLength, layerMask)) //  | down
        {
            Vector3 _startPosVertical = transform.position;
            _startPosVertical.y = _hit.point.y;

            if (Physics.Raycast(_startPosVertical, transform.forward, out RaycastHit _hitTwo, raycastLength, layerMask)) // > forward
            {
                Vector3 _startPosHorizontal = _hitTwo.point;
                _startPosHorizontal.y = upRaycastStart.position.y;

                if (Physics.Raycast(_startPosHorizontal, -transform.up, out RaycastHit _hitTree, raycastLength, layerMask)) // | down
                {
                    pointOnEdge = _hitTree.point;
                    marker.transform.position = pointOnEdge;

                    //Vector3 incomingVec = _hitTwo.point - _startPosVertical;

                    //// Use the point's normal to calculate the reflection vector.
                    //Vector3 reflectVec = Vector3.Reflect(incomingVec, _hitTwo.normal);

                    //znacznik.transform.eulerAngles = _hitTwo.normal;
                    return;
                }
            }
        }
        pointOnEdge = Vector3.zero;
        marker.transform.position = Vector3.zero;
    }

    

    private void Update()
    {
        if (isJumpingNow)
        {
            if (playerMovement.transform.position.y < endYpossition)
            {
                playerMovement.velocity = playerMovement.transform.forward * 2;
                playerMovement.velocity.y = climbingSpeed;
            }
            else
            {
                playerMovement.isSepcialJump = false;
                StartCoroutine(CrounchingOffDelay());
            }
        }
        else
        {
            if (pointOnEdge != Vector3.zero && Input.GetKeyDown(KeyCode.Space))     // toDo: new input system
            {
                PlayerEvents.SmallLeftHandJump();

                isJumpingNow = true;
                playerMovement.IsCounching = true;
                playerMovement.isSepcialJump = true;

                endYpossition = pointOnEdge.y + (playerMovement.controller.height / 2) - playerMovement.controller.center.y;

                CameraFlip(true);
            }
        }
    }


    IEnumerator CrounchingOffDelay()
    {
        yield return new WaitForSeconds(0.3f);
        playerMovement.IsCounching = false;
        isJumpingNow = false;
        CameraFlip(false);
    }


    void CameraFlip(bool isFlipped)// toDo: move tis to event and make special class for this
    {
        head.DOKill();
        if (isFlipped)
            head.DOLocalRotate(headFlippRotation, 0.26f);
        else
            head.DOLocalRotate(Vector3.zero, 0.26f);
    }
}
