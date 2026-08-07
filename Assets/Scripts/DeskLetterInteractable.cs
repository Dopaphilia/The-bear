using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeskLetterInteractable : MonoBehaviour
{
    [Header("위치 및 시선 설정")]
    [Tooltip("책상 상호작용 시 캐릭터가 설 위치 (빈 오브젝트)")]
    public Transform deskStandPoint;

    [Tooltip("책상 상호작용 시 캐릭터와 카메라가 바라볼 시선 위치 (빈 오브젝트)")]
    public Transform deskLookAtPoint;

    [Tooltip("지정된 위치 및 시선으로 이동하는 시간 (초)")]
    public float moveDuration = 1.0f;

    [Header("오브젝트 설정")]
    [Tooltip("캐릭터 손에 있던 편지 오브젝트 (상호작용 시 비활성화됨)")]
    public GameObject letterInHandObject;

    [Tooltip("책상에 있던 비활성화된 편지 오브젝트 (상호작용 시 다시 활성화됨)")]
    public GameObject letterOnDeskObject;

    [Tooltip("편지를 내려놓을 때 재생할 효과음 (선택사항)")]
    public AudioSource placeSound;

    [Header("전화 및 대화 설정")]
    [Tooltip("편지를 내려놓은 후 재생할 전화벨 효과음 (선택사항)")]
    public AudioSource phoneRingSound;

    [Tooltip("전화벨 재생 후 대화가 나오기까지 대기 시간 (초)")]
    public float phoneRingDelay = 2.0f;

    [TextArea(2, 5)]
    [Tooltip("전화벨 재생 후 출력할 통화 대화")]
    public string[] dialogueAfterPhone = new string[] {
        "언제까지 방에만 있을거니, 엄마가 보내준 영양제 계속 먹고 있지? 떨어지면 알려줘",
        "응.."
    };

    public void PlaceLetter(PlayerController player = null)
    {
        if (letterInHandObject != null)
        {
            letterInHandObject.SetActive(false);
        }
        else if (player != null && player.handLetterObject != null)
        {
            player.handLetterObject.SetActive(false);
        }

        if (letterOnDeskObject != null)
        {
            letterOnDeskObject.SetActive(true);
        }

        if (placeSound != null)
        {
            placeSound.Play();
        }
    }
}
