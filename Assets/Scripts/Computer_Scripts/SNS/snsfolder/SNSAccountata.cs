using UnityEngine;

[System.Serializable]
public class SNSAccountData
{
    public string userId;
    public string password;
    public string displayName;
    public Sprite profileImage;
    [TextArea(2, 5)]
    public string bio;
}