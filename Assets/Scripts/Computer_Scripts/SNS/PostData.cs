using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewPost", menuName = "SNS/PostData")]
public class PostData : ScriptableObject
{
    public Sprite postImage;         // 게시물 사진
    public int heartCount;           // 하트(좋아요) 수
    [TextArea(3, 5)]
    public string postContent;       // 오늘 한마디 (본문)
    public string postDate; // 👈 날짜 추가 (예: "2026년 3월 15일")

    public List<CommentData> comments = new List<CommentData>(); // 이 게시물에 달린 댓글 리스트
}