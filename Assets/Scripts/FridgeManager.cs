using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FridgeManager : MonoBehaviour
{
    [Header("Door Objects")]
    public Transform leftDoor;
    public Transform rightDoor;

    [Header("Settings")]
    [SerializeField] float openAngle = 150f; // 문이 열리는 각도
    [SerializeField] float speed = 2f;      // 문이 열리는 속도

    // 문 상태 관리 (true면 열림)
    public bool isLeftOpen = false;
    public bool isRightOpen = false;
    public bool isLeftMoving = false;
    public bool isRightMoving = false;
    // 코루틴 중복 실행 방지
    private Coroutine leftDoorRoutine;
    private Coroutine rightDoorRoutine;

    // PlayerController에서 호출할 함수
    public void Interact(GameObject hitObject)
    {
        // 플레이어가 바라보고 있는(Raycast에 맞은) 오브젝트가 무엇인지 판단
        if (hitObject == leftDoor.gameObject)
        {
            if (isLeftMoving) return;
            isLeftOpen = !isLeftOpen; // 상태 토글
            if (leftDoorRoutine != null) StopCoroutine(leftDoorRoutine);
            // 왼쪽 문 회전 시작 (Y축 기준 -90도 혹은 0도, 방향에 따라 부호 조절 필요)
            float targetY = isLeftOpen ? openAngle : 0f; 
            leftDoorRoutine = StartCoroutine(RotateDoor(leftDoor, targetY, true));
        }
        else if (hitObject == rightDoor.gameObject)
        {
            if (isRightMoving) return;
            isRightOpen = !isRightOpen;
            if (rightDoorRoutine != null) StopCoroutine(rightDoorRoutine);
            // 오른쪽 문 회전 시작
            float targetY = isRightOpen ? -openAngle : 0f;
            rightDoorRoutine = StartCoroutine(RotateDoor(rightDoor, targetY, false));
        }
    }

    // 부드럽게 회전시키는 코루틴
    IEnumerator RotateDoor(Transform door, float targetY, bool isLeft)
    {
        if (isLeft) isLeftMoving = true;
        else isRightMoving = true;

        // 현재 각도에서 목표 각도로 부드럽게 보간
        Quaternion startRotation = door.localRotation;
        Quaternion targetRotation = Quaternion.Euler(0, targetY, 0);
        
        float time = 0;
        while (time < 1)
        {
            time += Time.deltaTime * speed;
            door.localRotation = Quaternion.Slerp(startRotation, targetRotation, time);
            yield return null;
        }
        // 확실하게 목표 각도로 고정
        door.localRotation = targetRotation;

        if (isLeft) isLeftMoving = false;
        else isRightMoving = false;
    }
}