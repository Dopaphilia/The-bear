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
    public float fadeDuration = 0.3f;  // 창이 서서히 나타나는 시간
    
    private Queue<string> sentences;
    private bool isTyping = false;
    private bool isFadingOut = false;
    private string currentSentence;
    private CanvasGroup canvasGroup;
    private Coroutine fadeCoroutine;
    private Coroutine typingCoroutine;
    private float startDialogueTime = -1f;

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

        if (dialoguePanel != null)
        {
            canvasGroup = dialoguePanel.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = dialoguePanel.AddComponent<CanvasGroup>();
            }
        }
    }

    // 대화 진행 중 여부 확인 프로퍼티
    public bool IsDialogueActive
    {
        get
        {
            return dialoguePanel != null && dialoguePanel.activeSelf && !isFadingOut;
        }
    }

    // 대화 시작 (외부에서 호출)
    public void StartDialogue(string[] lines)
    {
        if (dialoguePanel == null || dialogueText == null) return;

        isFadingOut = false;
        dialoguePanel.SetActive(true);
        sentences.Clear();
        startDialogueTime = Time.time;

        // 대화 시작 즉시 플레이어 애니메이션을 IDLE 상태로 전환!
        PlayerController player = UnityEngine.Object.FindObjectOfType<PlayerController>();
        if (player != null)
        {
            player.ResetAnimationToIdle();
        }
        
        if (canvasGroup != null)
        {
            if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
            fadeCoroutine = StartCoroutine(FadeRoutine(0f, 1f));
        }

        // 대화 중 마우스 커서 표시 및 잠금 해제
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

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
        if (typingCoroutine != null) StopCoroutine(typingCoroutine);
        typingCoroutine = StartCoroutine(TypeSentence(sentence));
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

    private void CompleteSentence()
    {
        if (typingCoroutine != null) StopCoroutine(typingCoroutine);
        dialogueText.text = currentSentence;
        isTyping = false;
    }

    public void EndDialogue()
    {
        if (isFadingOut) return;

        // 대화 종료 시 마우스 커서 다시 숨김 및 잠금
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (canvasGroup != null && gameObject.activeInHierarchy)
        {
            if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
            fadeCoroutine = StartCoroutine(FadeOutAndHide());
        }
        else
        {
            dialoguePanel.SetActive(false);
            dialogueText.text = "";
        }
    }

    IEnumerator FadeRoutine(float startAlpha, float endAlpha)
    {
        float elapsed = 0f;
        canvasGroup.alpha = startAlpha;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, elapsed / fadeDuration);
            yield return null;
        }
        canvasGroup.alpha = endAlpha;
    }

    IEnumerator FadeOutAndHide()
    {
        isFadingOut = true;
        yield return StartCoroutine(FadeRoutine(canvasGroup.alpha, 0f));
        dialoguePanel.SetActive(false);
        dialogueText.text = "";
        isFadingOut = false;
    }

    void Update()
    {
        // 대화창이 켜진 직후 0.25초 이내에는 E키/클릭 입력을 방지하여, 상호작용할 때 누른 E키로 인해 대화창 페이드(선형보간)가 즉시 스킵되는 현상 해결!
        if (dialoguePanel.activeSelf && !isFadingOut && Time.time > startDialogueTime + 0.25f)
        {
            if (Input.GetKeyDown(KeyCode.E) || Input.GetMouseButtonDown(0))
            {
                DisplayNextSentence();
            }
        }
    }
}
