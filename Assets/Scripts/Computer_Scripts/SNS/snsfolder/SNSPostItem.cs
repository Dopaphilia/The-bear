using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class SNSPostItem : MonoBehaviour
{
    [Header("Post Data")]
    public Sprite postSprite;

    [TextArea(2, 5)]
    public string content;

    public string date;
    public int likeCount;
    public int commentCount;

    [Header("Detail UI")]
    public SNSPostDetailUI detailUI;

    private Button postButton;

    private void Awake()
    {
        postButton = GetComponent<Button>();
        postButton.onClick.AddListener(OpenDetail);
    }

    private void OpenDetail()
    {
        string spriteName = postSprite != null ? postSprite.name : "NULL";
        Debug.Log($"클릭된 오브젝트: {gameObject.name}, sprite: {spriteName}");

        if (detailUI == null) return;

        detailUI.OpenPostDetail(postSprite, date, likeCount, commentCount, content);
    }
}