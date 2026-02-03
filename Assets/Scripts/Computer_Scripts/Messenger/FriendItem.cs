using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FriendItem : MonoBehaviour
{
    public Image profileImage;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI statusText;

    // 친구 이름을 저장해둘 변수
    private string friendName;

    public void Setup(string name, string status, Color profileColor)
    {
        friendName = name;
        nameText.text = name;
        statusText.text = status;
        profileImage.color = profileColor;

        // 버튼 컴포넌트를 가져와서 클릭 리스너 연결
        GetComponent<Button>().onClick.AddListener(OnFriendClick);
    }

    void OnFriendClick()
    {
        // 클릭했을 때 실행될 로직 (매니저에게 알림)
        // 예: ChatManager.Instance.OpenChat(friendName);
        Debug.Log(friendName + "님과의 채팅창을 엽니다.");
    }
}