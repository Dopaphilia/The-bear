using UnityEngine;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;

public class SNSUIManager : MonoBehaviour
{
    [Header("Views")]
    public GameObject postDetailView;
    public GameObject commentOverlayView;
    public ProfileData myProfile; //  프로젝트 창의 Profile_1을 여기에 넣을 거예요.

    [Header("Profile View UI")] //  이 부분이 인스펙터에 나타나야 합니다!
    public Image profileImage;          // 메인 프로필 사진
    public TextMeshProUGUI profileNameText; // 프로필 이름 (Miffy)
    public TextMeshProUGUI profileStatusText; // 내 상태 메시지

    [Header("Detail View UI")]
    public Image detailImage;
    public TextMeshProUGUI detailContentText;
    public TextMeshProUGUI detailHeartText;
    public TextMeshProUGUI detailDateText;

    [Header("Comment Settings")]
    public List<CommentItem> commentRows;

    private PostData currentPost;

    // 게임 시작 시 내 프로필 정보를 세팅하는 함수
    public void SetProfileDisplay(ProfileData data)
    {
        if (data == null) return;
        if (profileImage != null) profileImage.sprite = data.profileImage;
        if (profileNameText != null) profileNameText.text = data.profileName;
        if (profileStatusText != null) profileStatusText.text = data.statusMessage;
    }

    public void OpenPostDetail(PostData data)
    {
        currentPost = data;
        postDetailView.SetActive(true);
        if (detailImage != null) detailImage.sprite = data.postImage;
        if (detailContentText != null) detailContentText.text = data.postContent;
        if (detailHeartText != null) detailHeartText.text = data.heartCount.ToString();
        if (detailDateText != null) detailDateText.text = data.postDate;
    }

    public void OpenComments()
    {
        if (currentPost == null) return;
        postDetailView.SetActive(false);
        commentOverlayView.SetActive(true);
        UpdateCommentUI(currentPost.comments);
    }

    public void CloseComments()
    {
        commentOverlayView.SetActive(false);
        postDetailView.SetActive(true);
    }

    private void UpdateCommentUI(List<CommentData> comments)
    {
        // 1. 기존 모든 댓글 줄을 비활성화
        foreach (var row in commentRows) row.gameObject.SetActive(false);

        // 2. 데이터 개수만큼 돌면서 이름 바꾸고 활성화
        for (int i = 0; i < comments.Count; i++)
        {
            if (i >= commentRows.Count) break;

            commentRows[i].gameObject.SetActive(true);

            // [이 부분 수정!] 하이어라키 이름을 "Comment 1", "Comment 2" 식으로 변경
            // i가 0부터 시작하니까 사람이 보기 편하게 +1을 해줍니다.
            commentRows[i].name = $"Comment {i + 1}";

            // 데이터 채우기
            commentRows[i].data = comments[i];
            commentRows[i].RefreshUI();
        }
    }
}