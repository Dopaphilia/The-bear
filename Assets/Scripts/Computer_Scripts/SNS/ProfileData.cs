using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewProfile", menuName = "SNS/ProfileData")]
public class ProfileData : ScriptableObject
{
    public string profileName;       // 프로필 주인 이름
    public Sprite profileImage;      // 프로필 사진
    public string statusMessage;  // 내 상태 메시지
    public List<PostData> myPosts;   // 이 사람이 작성한 게시물 리스트

}