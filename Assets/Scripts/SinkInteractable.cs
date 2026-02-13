using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SinkInteractable : MonoBehaviour
{
    [Header("Position Setup")]
    public Transform standPoint;  // 설 위치
    public Transform lookAtPoint; // 바라볼 위치 (오브젝트)
    
    [Header("Settings")]
    public float routineDuration = 4.0f; // 손 씻는 시간
    public float transitionSpeed = 2.0f; // 이동 속도

    [Header("Sound")]
    public AudioSource waterSound; // 여기에 오디오 소스를 연결하세요

    // 소리 켜기
    public void PlayWaterSound()
    {
        if (waterSound != null) 
        {
            waterSound.loop = false; // 루프 켜기
            waterSound.Play();
        }
    }

    // 소리 끄기
    public void StopWaterSound()
    {
        if (waterSound != null) 
        {
            waterSound.Stop();
        }
    }
}