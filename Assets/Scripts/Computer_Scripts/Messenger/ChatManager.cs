using UnityEngine;
using System.Collections; // IEnumerator와 Coroutine 사용을 위해 필수!
using TMPro;              // TextMeshPro 기능을 사용하기 위해 필수!

public class ChatManager : MonoBehaviour
{
    public GameObject listPanel;        // Messenger_List_Panel 연결용
    public GameObject chatPanel;        // Chat_Panel 연결용
    public TMP_InputField messageInput; // 입력창 연결

    // 친구 클릭 시 실행 (리스트 끔, 채팅창 켬)
    public void OpenChat()
    {
        listPanel.SetActive(false);
        chatPanel.SetActive(true);
    }

    // 뒤로가기 버튼(<) 클릭 시 실행 (채팅창 끔, 리스트 켬)
    public void BackToList()
    {
        chatPanel.SetActive(false);
        listPanel.SetActive(true);
    }

    // 전송 버튼에 이 함수를 연결하세요
    public void OnClickSend()
    {
        // 입력창에 적힌 텍스트 가져오기
        string userText = messageInput.text;

        // 텍스트가 비어있지 않을 때만 실행
        if (!string.IsNullOrEmpty(userText))
        {
            // 1. 내 메시지를 콘솔에 출력 (추후 말풍선 생성 코드가 들어갈 자리)
            Debug.Log("나: " + userText);

            // 2. 입력창 비우기
            messageInput.text = "";

            // 3. 1초 뒤 답장 오게 하기 (코루틴 시작)
            StartCoroutine(ReplyAfterDelay());
        }
    }

    // 1초 대기 후 답장을 로그로 찍는 기능
    IEnumerator ReplyAfterDelay()
    {
        yield return new WaitForSeconds(1f); // 1초 대기

        // 4. 상대방 답장 로직 (추후 상대방 말풍선 생성 코드가 들어갈 자리)
        Debug.Log("상대방: 그래 반가워!");
    }
}