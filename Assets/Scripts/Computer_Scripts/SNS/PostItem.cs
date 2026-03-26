using UnityEngine;
using UnityEngine.UI;

public class PostItem : MonoBehaviour
{
    public PostData myData; // 인스펙터에서 게시물 데이터 파일 할당

    void Start()
    {
        Button btn = GetComponent<Button>();
        if (btn != null)
            btn.onClick.AddListener(OnClickPost);
    }

    public void OnClickPost()
    {
        // 매니저를 찾아서 내 데이터를 넘기며 상세창을 열라고 함
        SNSUIManager manager = FindFirstObjectByType<SNSUIManager>();
        if (manager != null) manager.OpenPostDetail(myData);
    }
}