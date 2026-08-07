using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BookInteractable : MonoBehaviour
{
    [Header("Position Setup (Optional)")]
    [Tooltip("책을 읽기 위해 이동할 위치 (빈 오브젝트)")]
    public Transform readPoint;
    [Tooltip("책을 읽을 때 시선을 고정할 위치 (빈 오브젝트)")]
    public Transform lookAtPoint;
    [Tooltip("지정 위치로 이동 및 시선 보간에 걸리는 시간 (초)")]
    public float moveDuration = 0.8f;

    [Header("2D Book UI Panel")]
    [Tooltip("상호작용 시 화면 중앙에 팝업할 책 2D UI 패널 (Canvas 하위 Panel 등)")]
    public GameObject bookUIPanel;
    [Tooltip("체크 시 문 쾅쾅 사운드가 나기 직전에 책 UI를 닫습니다. 해제 시 모든 대사가 끝나고 닫힙니다.")]
    public bool closeBookBeforeSound = false;

    [Header("Dialogue Step 1 (책 열었을 때 대사)")]
    [Tooltip("책 UI가 열리면서 첫 번째로 출력할 대사")]
    [TextArea(2, 5)]
    public string[] dialogueBeforeSound = new string[] { 
        "왜 얘 사진만 이상하지?" 
    };

    [Header("Event Sound Step 2 (문을 쾅쾅거리는 사운드 & 정적)")]
    [Tooltip("첫 대사가 끝난 직후 재생할 사운드 (예: 현관문 쾅쾅 소리)")]
    public AudioSource doorBangSound;
    [Tooltip("사운드 재생 후 다음 대사가 나오기 전까지 긴장감 있는 정적 시간 (초)")]
    public float soundDelay = 1.0f;

    [Header("Dialogue Step 3 (소리 들은 후 반응 대사)")]
    [Tooltip("쾅쾅 소리 이후 출력할 반응 대사")]
    [TextArea(2, 5)]
    public string[] dialogueAfterSound = new string[] { 
        "현관으로 가보자..." 
    };

    [Header("Event Objects (Optional)")]
    [Tooltip("상호작용 종료 시 활성화할 오브젝트들 (예: 현관 이동 이벤트 트리거 등)")]
    public GameObject[] objectsToActivateOnFinish;
    [Tooltip("상호작용 종료 시 비활성화할 오브젝트들")]
    public GameObject[] objectsToDeactivateOnFinish;
}
