using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance;

    [Header("UI References")]
    public GameObject dialoguePanel;   // 배경 패널
    public TextMeshProUGUI dialogueText; // 실제 글자가 출력될 텍스트
    
    [Header("Settings")]
    public float typingSpeed = 0.05f;  // 글자 출력 속도
    
    private Queue<string> sentences;
    private bool isTyping = false;
    private string currentSentence;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            sentences = new Queue<string>();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // 대화 시작 (외부에서 호출)
    public void StartDialogue(string[] lines)
    {
        if (dialoguePanel == null || dialogueText == null) return;

        dialoguePanel.SetActive(true);
        sentences.Clear();

        foreach (string line in lines)
        {
            sentences.Enqueue(line);
        }

        DisplayNextSentence();
    }

    public void DisplayNextSentence()
    {
        if (sentences.Count == 0)
        {
            if (!isTyping) EndDialogue();
            else CompleteSentence(); // 타이핑 중이면 즉시 완성
            return;
        }

        if (isTyping)
        {
            CompleteSentence(); // 타이핑 중이면 즉시 완성
            return;
        }

        string sentence = sentences.Dequeue();
        currentSentence = sentence;
        StartCoroutine(TypeSentence(sentence));
    }

    IEnumerator TypeSentence(string sentence)
    {
        dialogueText.text = "";
        isTyping = true;

        foreach (char letter in sentence.ToCharArray())
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }

        isTyping = false;
    }

    // 타이핑 스킵 기능
    private void CompleteSentence()
    {
        StopAllCoroutines();
        dialogueText.text = currentSentence;
        isTyping = false;
    }

    public void EndDialogue()
    {
        dialoguePanel.SetActive(false);
        dialogueText.text = "";
    }

    void Update()
    {
        // 대화창이 켜져 있을 때만 입력 감지
        if (dialoguePanel.activeSelf)
        {
            if (Input.GetKeyDown(KeyCode.E) || Input.GetMouseButtonDown(0))
            {
                DisplayNextSentence();
            }
        }
    }
}
