using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorPeephole : MonoBehaviour
{
[Header("Settings")]
    public GameObject peepholeCamera; // 문에 달려있는 렌즈 카메라 오브젝트

    // 렌즈 보기 활성화
    public void EnableView()
    {
        peepholeCamera.SetActive(true);
    }

    // 렌즈 보기 비활성화
    public void DisableView()
    {
        peepholeCamera.SetActive(false);
    }
}
