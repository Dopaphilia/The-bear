using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class Door : MonoBehaviour
{
    public enum DoorType
    {
        Rotating, Sliding
    }
    public DoorType doorType = DoorType.Rotating;
    private bool isOpen = false;
    private bool isMoving = false;
    public float doorSpeed = 3.0f;

    [Header("Rotating Door")]
    public float openAngle = -90.0f;
    public float closeAngle = 0f;
    private Quaternion targetRotation;
    private float doorRotateSpeed = 90.0f;
    [Header("Sliding Door")]
    public Vector3 slideDirection = new Vector3(0, 0, 1);
    public float slideDistance = -1.8f;
    private Vector3 initialPosition;
    private Vector3 targetPosition;

        void Start()
    {
        targetRotation = Quaternion.Euler(0, closeAngle, 0);
        initialPosition = transform.localPosition;
        targetPosition = initialPosition;
    }
    
    public bool IsPlayerInPath (Vector3 playerPosition)
    {
        if (doorType == DoorType.Sliding) return false;

        Vector3 localPos = transform.InverseTransformPoint(playerPosition);
        if (localPos.z < 0) return true;
        if (isOpen && openAngle < 0 && localPos.x < 0) return true;
        if (isOpen && openAngle > 0 && localPos.x > 0) return true;
        return false;
    }
    // 문 열기/닫기 함수
    public bool doorOpen()
    {
        if (isMoving)
        {
            return false;
        }

        isMoving = true;
        isOpen = !isOpen;

        if (doorType == DoorType.Rotating) {
            if (isOpen)
            {
                targetRotation = Quaternion.Euler(0, openAngle, 0);
            }
            else
            {
                targetRotation = Quaternion.Euler(0, closeAngle, 0);
            }
        }
        else if (doorType == DoorType.Sliding)
        {
            if (isOpen)
            {
                targetPosition = initialPosition + (slideDirection.normalized * slideDistance);
            }
            else
            {
                targetPosition = initialPosition;
            }
        }
        return true;
    }
    // 문이 움직이는 중인지 반환
    public bool isDoorMoving()
    {
        return isMoving;
    }

    void Update()
    {
        if (doorType == DoorType.Rotating)
        {
            if(transform.localRotation != targetRotation)
            {   // Quternion = 회전을 나타내는 수학적 구조 (3D)
                transform.localRotation = Quaternion.RotateTowards(transform.localRotation, targetRotation, doorRotateSpeed * Time.deltaTime);
                if (Quaternion.Angle(transform.localRotation, targetRotation) < 0.1f) // Quaternion.Angle(a,b) = a와 b가 몇도가 차이나는지 리턴
                {
                    transform.localRotation = targetRotation;
                    isMoving = false;
                }
            }
        }
        else if (doorType == DoorType.Sliding)
        {
            transform.localPosition = Vector3.MoveTowards (transform.localPosition, targetPosition, doorSpeed * Time.deltaTime);
            if (Vector3.Distance(transform.localPosition, targetPosition) < 0.01f)
                {
                    if (isMoving)
                    {
                        transform.localPosition = targetPosition;
                        isMoving = false;
                    }
                }
        }
    }
}
