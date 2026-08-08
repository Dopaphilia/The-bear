using UnityEngine;

public class MirrorOptimizer : MonoBehaviour
{
    [Header("Settings")]
    public float renderDistance = 5f; // 거울이 보이는 거리 (현재 사용 안 함)
    
    [Header("References")]
    public Camera mirrorCamera; // 거울에 달린 인식 카메라
    private Transform playerTransform; // 플레이어 위치 (현재 사용 안 함)

    void Start()
    {
        // 스크립트에 의한 최적화 끄기 기능(버그 원인)을 삭제했습니다.
        // 게임 시작 시 무조건 카메라를 켭니다.
        if (mirrorCamera != null)
        {
            mirrorCamera.enabled = true;
        }
    }

    void Update()
    {
        // 항상 켜져있도록 유지합니다. 거리에 따른 꺼짐 기능을 완전히 삭제했습니다.
        if (mirrorCamera != null && !mirrorCamera.enabled)
        {
            mirrorCamera.enabled = true;
        }
    }
}