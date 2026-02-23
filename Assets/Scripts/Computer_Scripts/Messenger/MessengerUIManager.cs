using UnityEngine;

public class MessengerManager : MonoBehaviour
{
    [Header("Panel References")]
    public GameObject chatList;      // Chat_List 오브젝트
    public GameObject chatRoom;      // Chat_Room 패널 (전체)

    [Header("Chat Rooms")]
    public GameObject chatF1;        // Chat_F1 (친구 1의 대화창)

    // 친구 1 버튼을 눌렀을 때 (이미 만드신 것)
    public void OpenChatF1()
    {
        chatList.SetActive(false);
        chatRoom.SetActive(true);
        chatF1.SetActive(true);
    }

    // 뒤로가기 버튼을 눌렀을 때 실행할 함수
    public void BackToList()
    {
        // 1. 현재 켜져 있는 채팅방 화면들을 모두 끈다
        chatRoom.SetActive(false);
        chatF1.SetActive(false); // 나중에 친구가 늘어나면 이 부분도 일괄 관리하게 됩니다.

        // 2. 친구 목록을 다시 켠다
        chatList.SetActive(true);
    }
}