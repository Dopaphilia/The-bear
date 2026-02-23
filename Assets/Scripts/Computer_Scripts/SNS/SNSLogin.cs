using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class SNSLogin : MonoBehaviour
{
    [Header("UI 연결")]
    public TMP_InputField idInputField;   // 아이디 입력창 드래그
    public TMP_InputField pwInputField;   // 비밀번호 입력창 드래그
    public GameObject loginPanel;        // 현재 로그인 화면 (끄기용)
    public GameObject snsFeedPanel;      // 로그인 후 나타날 피드 화면 (켜기용)

    [Header("로그인 설정")]
    public string correctID = "miffy";    // 정답 아이디
    public string correctPW = "0505";     // 정답 비밀번호

    // 로그인 버튼에 연결할 함수
    public void CheckLogin()
    {
        // 1. 아이디와 비밀번호가 일치하는지 확인
        if (idInputField.text == correctID && pwInputField.text == correctPW)
        {
            Debug.Log("로그인 성공!");

            // 2. 로그인 창은 끄고, SNS 피드 화면을 켭니다.
            if (loginPanel != null) loginPanel.SetActive(false);
            if (snsFeedPanel != null) snsFeedPanel.SetActive(true);
        }
        else
        {
            // 3. 틀렸을 경우 알림 (선택사항: 입력창 초기화)
            Debug.Log("아이디 또는 비밀번호가 틀렸습니다.");
            pwInputField.text = ""; // 비밀번호만 지워줌
        }
    }
}