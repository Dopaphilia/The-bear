using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class Door : MonoBehaviour
{
    private bool isOpen = false;
    private bool isMoving = false;
    public float openAngle = -90.0f;
    public float closeAngle = 0f;
    public float doorSpeed = 3.0f;
    private Quaternion targetRotation;
    private float doorRotateSpeed = 90.0f;
    void Start()
    {
        targetRotation = Quaternion.Euler(0, closeAngle, 0);
    }
    // 문 열기/닫기 함수
    public bool doorOpen()
    {
        if (isMoving)
        {
            return false;
        }
        // 상태 반전
        isMoving = true;
        isOpen = !isOpen;

        if (isOpen)
        {
            targetRotation = Quaternion.Euler(0, openAngle, 0);
        }
        else
        {
            targetRotation = Quaternion.Euler(0, closeAngle, 0);
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

}
