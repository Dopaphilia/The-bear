using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public struct ChatLine
{
    public bool isMe;
    [TextArea(3, 5)]
    public string message;
}

public class ChatManager : MonoBehaviour
{
    [Header("UI 연결")]
    public GameObject myBubblePrefab;
    public GameObject npcBubblePrefab;
    public Transform content;
    public ScrollRect chatScroll;
    public InputField chatInputField;

    [Header("초기 설정")]
    // ★ 처음 시작할 때 미리 와 있을 메시지 개수를 적어주세요.
    public int initialMessageCount = 3;

    [Header("대본 설정")]
    public List<ChatLine> chatScript;
    private int currentStep = 0;
    private bool isProcessing = false;

    void OnEnable()
    {
        isProcessing = false;
        StopAllCoroutines();

        // 1. 저장된 기록이 있는지 확인합니다.
        if (!PlayerPrefs.HasKey("ChatStep"))
        {
            // ★ 2. 기록이 아예 없는 완전 처음이라면, 설정한 개수만큼 진행도를 올려버립니다.
            currentStep = Mathf.Min(initialMessageCount, chatScript.Count);
            SaveProgress();
        }
        else
        {
            // 3. 기존에 하던 대화가 있다면 그 진행도를 가져옵니다.
            currentStep = PlayerPrefs.GetInt("ChatStep", 0);
        }

        RefreshChatUI();
    }

    // (기존 RefreshChatUI, OnSendButtonClick, ChatFlowRoutine 등은 동일합니다)

    void RefreshChatUI()
    {
        foreach (Transform child in content) Destroy(child.gameObject);

        for (int i = 0; i < currentStep; i++)
        {
            CreateBubble(chatScript[i].isMe ? myBubblePrefab : npcBubblePrefab, chatScript[i].message);
        }

        if (currentStep < chatScript.Count && !chatScript[currentStep].isMe)
        {
            StartCoroutine(ChatFlowRoutine(true));
        }
        else
        {
            UpdateInputField();
        }
    }

    public void OnSendButtonClick()
    {
        if (!isProcessing && currentStep < chatScript.Count)
        {
            if (chatScript[currentStep].isMe)
            {
                StartCoroutine(ChatFlowRoutine(false));
            }
        }
    }

    IEnumerator ChatFlowRoutine(bool isResume)
    {
        isProcessing = true;
        if (!isResume)
        {
            CreateBubble(myBubblePrefab, chatScript[currentStep].message);
            currentStep++;
            SaveProgress();
            chatInputField.text = "";
        }

        while (currentStep < chatScript.Count && !chatScript[currentStep].isMe)
        {
            yield return new WaitForSeconds(1.0f);
            CreateBubble(npcBubblePrefab, chatScript[currentStep].message);
            currentStep++;
            SaveProgress();
        }

        UpdateInputField();
        isProcessing = false;
    }

    void SaveProgress() { PlayerPrefs.SetInt("ChatStep", currentStep); PlayerPrefs.Save(); }

    void UpdateInputField()
    {
        if (chatScript != null && currentStep < chatScript.Count)
            chatInputField.text = chatScript[currentStep].isMe ? chatScript[currentStep].message : "";
        else if (currentStep >= chatScript.Count)
            chatInputField.text = "대화 종료";
    }

    private void CreateBubble(GameObject prefab, string message)
    {
        GameObject newMessage = Instantiate(prefab, content);
        newMessage.GetComponentInChildren<TMP_Text>().text = message;
        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(content.GetComponent<RectTransform>());
        chatScroll.verticalNormalizedPosition = 0f;
    }

    [ContextMenu("Reset Progress")]
    public void ResetProgress()
    {
        PlayerPrefs.DeleteKey("ChatStep");
        // 리셋 후 OnEnable이 다시 실행되면서 initialMessageCount만큼 다시 채워집니다.
        OnEnable();
    }
}