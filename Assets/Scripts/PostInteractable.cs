using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PostInteractable : MonoBehaviour
{
    [Header("택배 열기 오브젝트 설정")]
    [Tooltip("기존에 닫혀있는 택배 상자 오브젝트 (상호작용 시 비활성화됨)")]
    public GameObject closedPostObject;

    [Tooltip("열려있는 택배 상자 오브젝트 (상호작용 시 활성화됨)")]
    public GameObject openedPostObject;

    [Tooltip("택배를 열 때 재생할 효과음 (선택사항)")]
    public AudioSource openSound;

    public void OpenPost()
    {
        if (closedPostObject != null)
        {
            closedPostObject.SetActive(false);
        }
        else
        {
            // closedPostObject가 따로 지정되지 않았다면 자기 자신을 비활성화
            gameObject.SetActive(false);
        }

        if (openedPostObject != null)
        {
            openedPostObject.SetActive(true);
        }

        if (openSound != null)
        {
            openSound.Play();
        }
    }
}
