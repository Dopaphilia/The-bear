using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MedicineInteractable : MonoBehaviour
{
    [Header("위치 및 시선 설정")]
    [Tooltip("약 상호작용 시 캐릭터가 이동하여 설 위치 (냉장고 앞 빈 오브젝트)")]
    public Transform fridgeStandPoint;

    [Tooltip("약 상호작용 시 캐릭터가 바라볼 포스트잇 위치 (냉장고 포스트잇 빈 오브젝트)")]
    public Transform postItLookAtPoint;

    [Tooltip("이동 및 시선 포커싱에 걸리는 시간 (초)")]
    public float moveDuration = 1.5f;

    [Header("오브젝트 및 대화 설정")]
    [Tooltip("상호작용 후 비활성화할 약 오브젝트 (선택사항)")]
    public GameObject medicineObjectToHide;

    [Tooltip("약을 먹을 때 재생할 효과음 (선택사항)")]
    public AudioSource medicineSound;

    [TextArea(2, 5)]
    [Tooltip("약 복용 및 포스트잇 응시 후 출력할 대화 (선택사항)")]
    public string[] dialogueAfterMedicine = new string[] {
        "포스트잇이 붙어있네..."
    };

    public void InteractMedicine(PlayerController player = null)
    {
        if (medicineObjectToHide != null)
        {
            medicineObjectToHide.SetActive(false);
        }
        else
        {
            // 게임오브젝트를 바로 비활성화하면 AudioSource 효과음이 즉시 멈추므로 렌더러와 콜라이더만 비활성화합니다.
            foreach (Renderer r in GetComponentsInChildren<Renderer>())
            {
                r.enabled = false;
            }
            foreach (Collider c in GetComponentsInChildren<Collider>())
            {
                c.enabled = false;
            }
        }

        if (medicineSound != null)
        {
            medicineSound.Play();
        }
    }
}
