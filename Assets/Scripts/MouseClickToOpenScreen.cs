using UnityEngine;
using UnityEngine.SceneManagement; 

public class MouseClickToOpenScreen : MonoBehaviour
{
    // 인스펙터 창에서 전환할 씬의 이름을 입력합니다.
    public string computerSceneName = "ComputerDesktopScene";

    void OnMouseDown()
    {
        // 이전에 확인했던 Debug.Log는 그대로 두셔도 됩니다.
        Debug.Log("컴퓨터 화면으로 이동 시작!");

        // 지정된 이름의 씬으로 로드합니다.
        SceneManager.LoadScene(computerSceneName);
    }
}