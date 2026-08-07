using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToiletInteractable : MonoBehaviour
{
    [Header("Position Setup")]
    [Tooltip("변기에 앉을 위치 (빈 오브젝트)")]
    public Transform sitPoint;
    [Tooltip("앉아서 바라볼 시선 고정 위치 (빈 오브젝트)")]
    public Transform lookAtPoint;
    [Tooltip("대화 종료 후 다시 일어날 위치 (빈 오브젝트)")]
    public Transform standUpPoint;
    [Tooltip("일어난 후 바라볼 방향 (Euler Angles)")]
    public Vector3 standUpRotation = Vector3.zero;

    [Header("Settings")]
    [Tooltip("앉는 위치와 시선으로 이동하는 선형 보간 시간")]
    public float moveDuration = 1.0f;
    [Tooltip("화면이 어두워지고 밝아지는 페이드 속도")]
    public float fadeDuration = 0.3f;

    [Header("Dialogue Step 1 (사운드 재생 전 대화)")]
    [Tooltip("효과음이 울리기 전에 먼저 출력할 대화")]
    [TextArea(2, 5)]
    public string[] dialogueBeforeSound = new string[] { 
        "으으... 시원하다..." 
    };

    [Header("Event Sound Step 2 (이벤트 소리 & 정적)")]
    [Tooltip("첫 번째 대화가 끝난 후 재생할 효과음 (예: '쿵!' 소리)")]
    public AudioSource eventSound;
    [Tooltip("효과음 재생 후 다음 대화가 뜨기 전까지의 긴장감 있는 정적 시간 (초)")]
    public float soundDelay = 1.0f;

    [Header("Dialogue Step 3 (사운드 재생 후 대화)")]
    [Tooltip("효과음이 울린 뒤 이어서 출력할 대화")]
    [TextArea(2, 5)]
    public string[] dialogueAfterSound = new string[] { 
        "뭐지...?" 
    };


    [Header("Event Objects (Optional)")]
    [Tooltip("상호작용 종료 시 활성화할 오브젝트들 (예: 바닥에 떨어진 물건, 열쇠 등)")]
    public GameObject[] objectsToActivateOnFinish;
    [Tooltip("상호작용 종료 시 비활성화할 오브젝트들")]
    public GameObject[] objectsToDeactivateOnFinish;

    [Header("Sound (Optional)")]
    [Tooltip("상호작용 시작 시 재생할 소리 (예: 변기 커버 올리는 소리 혹은 앉는 소리)")]
    public AudioSource toiletSound;
    [Tooltip("모든 대화가 끝난 뒤 일어서기 직전 재생할 소리 (예: 변기 물 내리는 소리)")]
    public AudioSource flushSound;
    [Tooltip("물 내리는 소리가 난 후 일어서기 전까지의 짧은 대기 시간 (초)")]
    public float flushDelay = 0.5f;

    public void PlayToiletSound()
    {
        if (toiletSound != null)
        {
            toiletSound.Play();
        }
    }

    public void PlayFlushSound()
    {
        if (flushSound != null)
        {
            flushSound.Play();
        }
    }
}
