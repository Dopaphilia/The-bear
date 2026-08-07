using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PeepholeInteractable : MonoBehaviour
{
    [Header("1. 카메라 & UI 설정 (Camera & UI)")]
    [Tooltip("문에 달려있는 렌즈 카메라 오브젝트")]
    public GameObject peepholeCamera;
    [Tooltip("렌즈를 볼 때 화면에 띄울 2D UI (예: 어안렌즈 프레임 등)")]
    public GameObject peepholeUIPanel;

    [Header("2. 상호작용 텍스트 (Prompt)")]
    [Tooltip("화면에 표시될 안내 텍스트 (예: 들여다보기)")]
    public string interactPrompt = "들여다보기";

    [Header("3. 효과음 설정 (Sound Effects)")]
    [Tooltip("렌즈에 눈을 갖다 댈 때 재생할 효과음")]
    public AudioSource peepStartSound;
    [Tooltip("렌즈를 보고 있는 동안 재생할 이벤트 효과음 (예: 밖에서 나는 소리 등)")]
    public AudioSource peepEventSound;
    [Tooltip("렌즈에서 눈을 뗄 때 재생할 효과음")]
    public AudioSource peepExitSound;

    [Header("4. 대화 설정 (Dialogue)")]
    [Tooltip("렌즈를 들여다보기 시작할 때 출력될 대사")]
    [TextArea(1, 3)]
    public string[] dialogueOnPeep = new string[] { "아.. 옆집 할머니네.." };
    [Tooltip("렌즈에서 눈을 떼고 났을 때 출력될 대사")]
    [TextArea(1, 3)]
    public string[] dialogueAfterPeep;

    [Header("5. 렌즈 보는 즉시 작동할 오브젝트 (On Peep Start)")]
    [Tooltip("렌즈를 들여다볼 때 활성화할 오브젝트 (예: 복도의 몬스터/그림자 등)")]
    public GameObject[] objectsToActivateOnPeep;
    [Tooltip("렌즈를 들여다볼 때 비활성화할 오브젝트")]
    public GameObject[] objectsToDeactivateOnPeep;

    [Header("6. 렌즈 보기가 끝난 후 작동할 오브젝트 (On Peep Exit)")]
    [Tooltip("렌즈 보기를 마쳤을 때 활성화할 오브젝트")]
    public GameObject[] objectsToActivateOnExit;
    [Tooltip("렌즈 보기를 마쳤을 때 비활성화할 오브젝트")]
    public GameObject[] objectsToDeactivateOnExit;

    [Header("7. 위치 설정 (Optional)")]
    [Tooltip("렌즈 시점 종료 후 플레이어가 서 있을 위치(선택, 비어있으면 현재 위치 유지)")]
    public Transform exitStandPoint;

    // 렌즈 카메라 및 UI 켜기
    public void EnableView()
    {
        if (peepholeCamera != null) peepholeCamera.SetActive(true);
        if (peepholeUIPanel != null) peepholeUIPanel.SetActive(true);
    }

    // 렌즈 카메라 및 UI 끄기
    public void DisableView()
    {
        if (peepholeCamera != null) peepholeCamera.SetActive(false);
        if (peepholeUIPanel != null) peepholeUIPanel.SetActive(false);
    }

    public void PlayStartSound()
    {
        if (peepStartSound != null) peepStartSound.Play();
    }

    public void PlayEventSound()
    {
        if (peepEventSound != null) peepEventSound.Play();
    }

    public void PlayExitSound()
    {
        if (peepExitSound != null) peepExitSound.Play();
    }
}
