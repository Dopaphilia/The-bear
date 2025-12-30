using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ComputerLogin : MonoBehaviour
{
    public InputField passwordInput;
    public GameObject Login_Panel;
    public GameObject Desktop_Panel;
    public GameObject JumpScare_Panel;
    public string correctPassword = "1234";

    private Text placeholderText;
    private int failCount = 0;
    private bool isLocked = false;
    private bool hasJumpScared = false; // 이미 한 번 떴는지 체크

    void Start()
    {
        if (Login_Panel != null) Login_Panel.SetActive(true);
        if (Desktop_Panel != null) Desktop_Panel.SetActive(false);
        if (JumpScare_Panel != null) JumpScare_Panel.SetActive(false);

        if (passwordInput != null && passwordInput.placeholder != null)
        {
            placeholderText = passwordInput.placeholder.GetComponent<Text>();
        }

        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas != null && canvas.renderMode == RenderMode.ScreenSpaceCamera)
        {
            if (canvas.worldCamera == null)
            {
                canvas.worldCamera = GameObject.Find("Camera").GetComponent<Camera>();
            }
        }
    }

    public void CheckPassword()
    {
        if (isLocked) return;

        if (passwordInput.text == correctPassword)
        {
            failCount = 0;
            Login_Panel.SetActive(false);
            Desktop_Panel.SetActive(true);
        }
        else
        {
            failCount++;
            passwordInput.text = "";

            // [수정] 3번 틀렸고, 아직 사진이 뜬 적이 없을 때만 실행
            if (failCount >= 3 && !hasJumpScared)
            {
                UpdateUI("SYSTEM CRITICAL ERROR!", Color.red);
                StartCoroutine(ShowJumpScareRoutine());
            }
            else
            {
                // [수정] 모든 틀린 상황(1회, 2회, 혹은 이미 사진을 본 후의 3회 이상)에서 동일한 메시지 출력
                UpdateUI("다시 시도하세요.", Color.red);
            }
        }
    }

    IEnumerator ShowJumpScareRoutine()
    {
        isLocked = true;
        hasJumpScared = true; // 실행 완료 기록

        if (JumpScare_Panel != null)
        {
            JumpScare_Panel.SetActive(true);
            JumpScare_Panel.transform.SetAsLastSibling();
        }

        yield return new WaitForSeconds(2f); // 2초 대기

        if (JumpScare_Panel != null)
        {
            JumpScare_Panel.SetActive(false);
        }

        UpdateUI("비밀번호 입력...", Color.gray);
        isLocked = false;
        failCount = 0; // 카운트 초기화
        passwordInput.ActivateInputField();
    }

    void UpdateUI(string msg, Color color)
    {
        if (placeholderText != null)
        {
            placeholderText.text = msg;
            placeholderText.color = color;
        }
    }
}