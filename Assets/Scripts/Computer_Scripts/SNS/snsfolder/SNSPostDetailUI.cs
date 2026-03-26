using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SNSPostDetailUI : MonoBehaviour
{
    [Header("Panel")]
    public GameObject postDetailPanel;

    [Header("UI References")]
    public Image postImage;
    public TMP_Text dateText;
    public TMP_Text likeText;
    public TMP_Text commentText;
    public TMP_Text contentText;

    [Header("Buttons")]
    public Button closeButton;

    private void Start()
    {
        if (postDetailPanel != null)
        {
            postDetailPanel.SetActive(false);
        }

        if (closeButton != null)
        {
            closeButton.onClick.AddListener(ClosePostDetail);
        }
    }

    public void OpenPostDetail(Sprite imageSprite, string date, int likeCount, int commentCount, string content)
    {
        if (postDetailPanel == null) return;

        postDetailPanel.SetActive(true);
        postDetailPanel.transform.SetAsLastSibling();

        if (postImage != null) postImage.sprite = imageSprite;
        if (dateText != null) dateText.text = date;
        if (likeText != null) likeText.text = likeCount.ToString();
        if (commentText != null) commentText.text = commentCount.ToString();
        if (contentText != null) contentText.text = content;
    }

    public void ClosePostDetail()
    {
        if (postDetailPanel != null)
        {
            postDetailPanel.SetActive(false);
        }
    }
}