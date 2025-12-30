using UnityEngine;
using UnityEngine.SceneManagement; // 이 줄을 스크립트 맨 위에 추가해야 합니다.

public class WindowManager : MonoBehaviour
{
    // Inspector 창에서 연결할 이메일 작성 창 오브젝트
    public GameObject emailComposeWindow;

    // 창을 활성화하여 보이게 하는 함수
    public void OpenWindow()
    {
        if (emailComposeWindow != null)
        {
            Debug.Log("이메일 창 열기 실행됨.");
            emailComposeWindow.SetActive(true);
        }
    }

    // 창을 비활성화하여 숨기는 함수
    public void CloseWindow()
    {
        if (emailComposeWindow != null)
        {
            Debug.Log("이메일 창 닫기 실행됨.");
            emailComposeWindow.SetActive(false);
        }
    }

    // 씬 전환을 위한 새로운 함수 추가 
    public void GoToRoomScene()
    {
        // 씬 파일 이름과 정확히 일치해야 합니다. (Room.unity)
        Debug.Log("Room 씬으로 이동합니다.");
        SceneManager.LoadScene("Room");
    }
    // 씬 전환을 위한 새로운 함수 추가 
}