using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Game Settings")]
    public int currentDay = 1;

    [Header("Dialogue Settings")]
    [TextArea(2, 5)]
    public string[] introDialogueLines = new string[] { "으으 똥마려워.." };

    [Header("Rule States")]
    public bool hasLitCandle = false;    // 규칙 1: 향초 피우기
    public bool hasTakenSupplements = false; // 규칙 2: 영양제 먹기
    public bool hasWashedHands = false;    // 규칙 3: 손 씻기

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // 게임 시작 시 첫 번째 날 연출 실행
        StartCoroutine(FirstDayStartRoutine());
    }

    IEnumerator FirstDayStartRoutine()
    {
        // 플레이어 컨트롤러를 찾을 때까지 잠시 대기 (씬 로딩 고려)
        PlayerController player = null;
        while (player == null)
        {
            player = FindObjectOfType<PlayerController>();
            yield return null;
        }

        // 1. 시작 시 화면을 즉시 검게 만들고 Day 1 텍스트 설정
        if (player.blackFadeImage != null)
        {
            player.blackFadeImage.color = Color.black;
            player.blackFadeImage.gameObject.SetActive(true);
        }

        if (player.dayTextUI != null)
        {
            player.dayTextUI.text = "DAY " + currentDay;
            player.dayTextUI.gameObject.SetActive(true);
            Color textCol = player.dayTextUI.color;
            textCol.a = 1f;
            player.dayTextUI.color = textCol;
        }

        player.isHandlingRoutine = true; // 연출 중 이동 금지

        // 2. 잠시 대기 (Day 1을 보여줌)
        yield return new WaitForSeconds(2.0f);

        // 3. 서서히 밝아지기 (Fade In)
        float elapsed = 0;
        float fadeDuration = 2.0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeDuration;

            if (player.blackFadeImage != null)
            {
                Color c = player.blackFadeImage.color;
                c.a = 1f - t;
                player.blackFadeImage.color = c;
            }

            if (player.dayTextUI != null)
            {
                Color tc = player.dayTextUI.color;
                tc.a = 1f - t;
                player.dayTextUI.color = tc;
            }
            yield return null;
        }

        if (player.dayTextUI != null) player.dayTextUI.gameObject.SetActive(false);
        player.isHandlingRoutine = false; // 이동 해제

        // [추가] 시작 대화 출력
        if (DialogueManager.Instance != null && introDialogueLines != null && introDialogueLines.Length > 0)
        {
            DialogueManager.Instance.StartDialogue(introDialogueLines);
        }
    }

    public string GetNextDayText()
    {
        currentDay++;
        // 다음 날로 넘어갈 때 규칙 상태 초기화
        hasLitCandle = false;
        hasTakenSupplements = false;
        hasWashedHands = false;

        // 씬 내의 모든 향초를 찾아 불을 끔
        ExtinguishAllCandles();

        return "DAY " + currentDay;
    }

    private void ExtinguishAllCandles()
    {
        CandleController[] candles = FindObjectsOfType<CandleController>();
        foreach (CandleController candle in candles)
        {
            candle.ExtinguishCandle();
        }
    }

    // 모든 규칙을 지켰는지 확인하는 함수
    public bool IsAllRulesCleared()
    {
        return hasLitCandle && hasTakenSupplements && hasWashedHands;
    }
}