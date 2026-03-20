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
    
    [Tooltip("문이 열린 후 자동으로 닫히기까지 기다리는 시간 (초)")]
    [SerializeField] float autoCloseDelay = 3.0f; // ★ 추가된 타이머 변수

    private bool isDoorOpen = false;
    private bool isMoving = false;
    private bool isAtStartFloor = true;

    private Vector3 innerLeftClosedPos, innerRightClosedPos;
    private Vector3 startLeftClosedPos, startRightClosedPos;
    private Vector3 targetLeftClosedPos, targetRightClosedPos;

    private Coroutine autoCloseCoroutine; // ★ 타이머 제어용 코루틴

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
            // ★ 수정됨: 문이 닫혀있어도 이동 로직이 실행되도록 조건문 제거
            StartCoroutine(MoveElevatorRoutine(player));
        }
    }

    IEnumerator OpenDoorRoutine()
    {
        isDoorOpen = true;
        yield return StartCoroutine(MoveDoors(true));

        // ★ 기존 타이머가 돌고 있다면 끄고, 새로 3초 타이머 시작
        if (autoCloseCoroutine != null) StopCoroutine(autoCloseCoroutine);
        autoCloseCoroutine = StartCoroutine(AutoCloseTimer());
    }

    // ★ 추가된 자동 닫힘 타이머 로직
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

        // ★ 추가 1: 이동 전 플레이어의 물리 연산(CharacterController) 끄기
        CharacterController cc = player.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;

        player.transform.SetParent(elevatorCar);

        Vector3 startPos = elevatorCar.position;
        Vector3 targetPos = isAtStartFloor ? targetFloorPoint.position : startFloorPoint.position;
        
        float distance = Vector3.Distance(startPos, targetPos);
        float duration = distance / moveSpeed; 
        float time = 0;

        while (time < 1f)
        {
            time += Time.deltaTime / duration;
            float smoothT = Mathf.SmoothStep(0f, 1f, time); 
            
            elevatorCar.position = Vector3.Lerp(startPos, targetPos, smoothT);
            yield return null;
        }
        
        elevatorCar.position = targetPos; 
        player.transform.SetParent(null);

        // ★ 추가 2: 도착 후 물리 연산 다시 켜기
        if (cc != null) cc.enabled = true;

        isAtStartFloor = !isAtStartFloor;

        yield return StartCoroutine(OpenDoorRoutine());

        isMoving = false;
    }
}