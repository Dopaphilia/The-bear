using System.Collections;
using UnityEngine;

public class ElevatorController : MonoBehaviour
{
    [Header("Elevator Body")]
    [SerializeField] Transform elevatorCar;

    [Header("Inner Doors (Elevator Car)")]
    [SerializeField] Transform innerLeftDoor;  
    [SerializeField] Transform innerRightDoor; 

    [Header("Outer Doors (Start Floor / 9F)")]
    [SerializeField] Transform startFloorLeftDoor;  
    [SerializeField] Transform startFloorRightDoor; 

    [Header("Outer Doors (Target Floor / 1F)")]
    [SerializeField] Transform targetFloorLeftDoor;  
    [SerializeField] Transform targetFloorRightDoor; 

    [Header("Move Points")]
    public Transform startFloorPoint;  
    public Transform targetFloorPoint; 

    [Header("Settings")]
    [SerializeField] Vector3 doorOpenOffset = new Vector3(0, 0, 1.5f); 
    [SerializeField] float doorMoveDuration = 1.0f; 
    [SerializeField] float moveSpeed = 3f;
    [SerializeField] float autoCloseDelay = 3.0f;

    [Header("Player Control")]
    [SerializeField] Transform playerStandPoint;
    [SerializeField] Vector3 playerStandRotation = new Vector3(5f,0f,0f);

    private bool isDoorOpen = false;
    private bool isMoving = false;
    private bool isAtStartFloor = true;

    private Vector3 innerLeftClosedPos, innerRightClosedPos;
    private Vector3 startLeftClosedPos, startRightClosedPos;
    private Vector3 targetLeftClosedPos, targetRightClosedPos;

    private Coroutine autoCloseCoroutine;

    void Start()
    {
        if (innerLeftDoor) innerLeftClosedPos = innerLeftDoor.localPosition;
        if (innerRightDoor) innerRightClosedPos = innerRightDoor.localPosition;
        
        if (startFloorLeftDoor) startLeftClosedPos = startFloorLeftDoor.localPosition;
        if (startFloorRightDoor) startRightClosedPos = startFloorRightDoor.localPosition;
        
        if (targetFloorLeftDoor) targetLeftClosedPos = targetFloorLeftDoor.localPosition;
        if (targetFloorRightDoor) targetRightClosedPos = targetFloorRightDoor.localPosition;
    }

    public void Interact(GameObject hitObject, GameObject player)
    {
        if (isMoving) return;

        if (hitObject.name == "9F_Outer_Button" || hitObject.name == "1F_Outer_Button")
        {
            if (!isDoorOpen) StartCoroutine(OpenDoorRoutine());
        }
        else if (hitObject.name == "Inner_Button")
        {
            StartCoroutine(MoveElevatorRoutine(player));
        }
    }

    IEnumerator OpenDoorRoutine()
    {
        isDoorOpen = true;
        yield return StartCoroutine(MoveDoors(true));

        if (autoCloseCoroutine != null) StopCoroutine(autoCloseCoroutine);
        autoCloseCoroutine = StartCoroutine(AutoCloseTimer());
    }

    IEnumerator AutoCloseTimer()
    {
        yield return new WaitForSeconds(autoCloseDelay);
        if (isDoorOpen && !isMoving) 
        {
            yield return StartCoroutine(CloseDoorRoutine());
        }
    }

    IEnumerator CloseDoorRoutine()
    {
        isDoorOpen = false;
        yield return StartCoroutine(MoveDoors(false));
    }

    IEnumerator MoveDoors(bool isOpen)
    {
        Transform currentOuterLeft = isAtStartFloor ? startFloorLeftDoor : targetFloorLeftDoor;
        Transform currentOuterRight = isAtStartFloor ? startFloorRightDoor : targetFloorRightDoor;
        
        Vector3 outerLeftClosedPos = isAtStartFloor ? startLeftClosedPos : targetLeftClosedPos;
        Vector3 outerRightClosedPos = isAtStartFloor ? startRightClosedPos : targetRightClosedPos;

        Vector3 targetInnerLeft = isOpen ? innerLeftClosedPos - doorOpenOffset : innerLeftClosedPos;
        Vector3 targetInnerRight = isOpen ? innerRightClosedPos + doorOpenOffset : innerRightClosedPos;
        
        Vector3 targetOuterLeft = isOpen ? outerLeftClosedPos - doorOpenOffset : outerLeftClosedPos;
        Vector3 targetOuterRight = isOpen ? outerRightClosedPos + doorOpenOffset : outerRightClosedPos;

        Vector3 startInnerLeft = innerLeftDoor.localPosition;
        Vector3 startInnerRight = innerRightDoor.localPosition;
        Vector3 startOuterLeft = currentOuterLeft ? currentOuterLeft.localPosition : Vector3.zero;
        Vector3 startOuterRight = currentOuterRight ? currentOuterRight.localPosition : Vector3.zero;

        float time = 0;
        while (time < 1f)
        {
            time += Time.deltaTime / doorMoveDuration;
            float smoothT = Mathf.SmoothStep(0f, 1f, time);

            if (innerLeftDoor) innerLeftDoor.localPosition = Vector3.Lerp(startInnerLeft, targetInnerLeft, smoothT);
            if (innerRightDoor) innerRightDoor.localPosition = Vector3.Lerp(startInnerRight, targetInnerRight, smoothT);
            
            if (currentOuterLeft) currentOuterLeft.localPosition = Vector3.Lerp(startOuterLeft, targetOuterLeft, smoothT);
            if (currentOuterRight) currentOuterRight.localPosition = Vector3.Lerp(startOuterRight, targetOuterRight, smoothT);
            
            yield return null;
        }
    }

    IEnumerator MoveElevatorRoutine(GameObject player)
    {
        isMoving = true;

        if (autoCloseCoroutine != null) StopCoroutine(autoCloseCoroutine);

        if (isDoorOpen) 
        {
            yield return StartCoroutine(CloseDoorRoutine());
        }

        CharacterController cc = player.GetComponent<CharacterController>();
        PlayerController pc = player.GetComponent<PlayerController>();

        if (cc != null)
        {
            cc.enabled = false;
        }
        if (pc != null) 
        {
            pc.isHandlingRoutine = true;
        }

        if (playerStandPoint != null)
        {
            float elapsed = 0f;
            float duration = 0.8f;

            // 시작 지점 저장
            Vector3 startPos = player.transform.position;
            Quaternion startRot = player.transform.rotation;
            float startCamX = pc.currentCameraRotationX;

            // 목표 지점 설정
            Vector3 targetPos = playerStandPoint.position;
            Quaternion targetRot = Quaternion.Euler(0, playerStandRotation.y, 0);
            float targetCamX = playerStandRotation.x;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                float smoothT = Mathf.SmoothStep(0f, 1f, t); // S자 곡선 보간

                // 위치 이동
                player.transform.position = Vector3.Lerp(startPos, targetPos, smoothT);

                // 몸통 회전 (좌우)
                player.transform.rotation = Quaternion.Slerp(startRot, targetRot, smoothT);

                // 카메라 회전 (상하)
                if (pc != null && pc.playerCamera != null)
                {
                    pc.currentCameraRotationX = Mathf.Lerp(startCamX, targetCamX, smoothT);
                    pc.playerCamera.transform.localEulerAngles = new Vector3(pc.currentCameraRotationX, 0, 0);
                }

                yield return null;
            }
            player.transform.position = targetPos;
            player.transform.rotation = targetRot;
        }
        

        player.transform.SetParent(elevatorCar);

        Vector3 elevatorStartPos = elevatorCar.position;
        Vector3 elevatorTargetPos = isAtStartFloor ? targetFloorPoint.position : startFloorPoint.position;
        
        float distance = Vector3.Distance(elevatorStartPos, elevatorTargetPos);
        float elevatorDuration = distance / moveSpeed; 
        float time = 0;

        while (time < 1f)
        {
            time += Time.deltaTime / elevatorDuration;
            float smoothT = Mathf.SmoothStep(0f, 1f, time); 
            
            elevatorCar.position = Vector3.Lerp(elevatorStartPos, elevatorTargetPos, smoothT);
            yield return null;
        }
        
        elevatorCar.position = elevatorTargetPos; 

        player.transform.SetParent(null);

        if (cc != null)
        {
            cc.enabled = true;
        }

        if (pc != null) 
        {
            pc.isHandlingRoutine = false;
        }

        isAtStartFloor = !isAtStartFloor;
        yield return StartCoroutine(OpenDoorRoutine());
        isMoving = false;
    }
}