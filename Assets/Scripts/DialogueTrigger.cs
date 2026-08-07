using UnityEngine;

public class DialogueTrigger : MonoBehaviour
{
    [Header("대화 설정")]
    [TextArea(2, 5)]
    public string[] lines; // 인스펙터에서 편하게 적을 수 있도록 여러 줄 텍스트 속성 부여

    [Header("상호작용 설정")]
    public string interactPrompt = "조사하기"; // 화면에 띄울 상호작용 텍스트 (기본값)

    // 대화를 강제로 실행하고 싶을 때 호출하는 함수
    public void TriggerDialogue()
    {
        if (lines != null && lines.Length > 0 && DialogueManager.Instance != null)
        {
            DialogueManager.Instance.StartDialogue(lines);
        }
    }
}
