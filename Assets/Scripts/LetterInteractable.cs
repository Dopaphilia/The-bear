using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LetterInteractable : MonoBehaviour
{
    [Header("편지 획득 오브젝트 설정")]
    [Tooltip("택배 상자 안에 있는 편지 오브젝트 (상호작용 시 비활성화됨)")]
    public GameObject letterInBoxObject;

    [Tooltip("캐릭터 손에 준비되어 있는 편지 오브젝트 (상호작용 시 활성화됨)")]
    public GameObject letterInHandObject;

    [Header("대화 설정")]
    [TextArea(2, 5)]
    public string[] dialogueOnAcquire = new string[] {
        "내가 이런 편지를 받았었나..?"
    };

    [Tooltip("편지를 획득할 때 재생할 효과음 (선택사항)")]
    public AudioSource acquireSound;

    public void AcquireLetter(PlayerController player = null)
    {
        if (letterInBoxObject != null)
        {
            letterInBoxObject.SetActive(false);
        }
        else
        {
            gameObject.SetActive(false);
        }

        if (letterInHandObject != null)
        {
            letterInHandObject.SetActive(true);
        }
        else if (player != null && player.handLetterObject != null)
        {
            player.handLetterObject.SetActive(true);
        }

        if (acquireSound != null)
        {
            acquireSound.Play();
        }
    }
}
