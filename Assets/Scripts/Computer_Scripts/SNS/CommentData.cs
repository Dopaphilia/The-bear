using UnityEngine;

[CreateAssetMenu(fileName = "NewComment", menuName = "SNS/CommentData")]
public class CommentData : ScriptableObject
{
    public string userName;          // 댓글 쓴 사람 이름
    [TextArea(3, 10)]
    public string comment;           // 댓글 내용
}