using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BedInteractable : MonoBehaviour
{
    [Header("Position Setup")]
    // 침대에 누웠을 때 플레이어가 이동할 위치 (빈 오브젝트로 위치 지정)
    public Transform sleepPoint;
    public Vector3 sleepRotation = new Vector3(-84f, -13f, 14f);

    [Header("Wake Up Setup")]
    public Transform wakeUpPoint;
    public Vector3 wakeUpRotation = new Vector3(0f, -58.17f, 0f);


    [Header("Settings")]
    // 화면이 완전히 어두워지는 데 걸리는 시간 (페이드 아웃 속도)
    public float fadeDuration = 1.5f; 
    
    // 검은 화면 상태로 유지되는 시간 (이 시간 동안 날짜 텍스트가 뜸)
    public float blackScreenHoldTime = 2.0f; 
}