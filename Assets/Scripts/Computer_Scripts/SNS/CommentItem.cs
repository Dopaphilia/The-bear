using UnityEngine;
using TMPro;

public class CommentItem : MonoBehaviour
{
    public CommentData data;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI contentText;

    public void RefreshUI()
    {
        if (data != null)
        {
            if (nameText != null) nameText.text = data.userName;
            if (contentText != null) contentText.text = data.comment;
        }
    }
}