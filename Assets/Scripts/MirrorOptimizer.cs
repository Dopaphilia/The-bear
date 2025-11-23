using UnityEngine;

public class MirrorOptimizer : MonoBehaviour
{
    [Header("Settings")]
    public float renderDistance = 5f; // 거울이 보이는 거리
    
    [Header("References")]
    public Camera mirrorCamera; // 거울에 달린 자식 카메라

    private Transform playerTransform; // 플레이어(메인카메라)의 위치

    void Start()
    {
        // 씬이 달라도 태그나 Camera.main을 통해 플레이어를 찾을 수 있습니다.
        // 보통 1인칭 게임이면 MainCamera가 플레이어 눈 위치입니다.
        if (Camera.main != null)
        {
            playerTransform = Camera.main.transform;
            Debug.Log("✅ 플레이어(MainCamera)를 찾았습니다!");
        }
    }

    void Update()
    {
        // 플레이어를 못 찾았으면 다시 찾기 시도 (씬 로딩 타이밍 차이 대비)
        if (playerTransform == null)
        {
            if (Camera.main != null) playerTransform = Camera.main.transform;
            return;
        }

        // 거리 계산 (성능을 위해 제곱 거리 비교 사용 가능하지만, 간단하게 Distance 사용)
        float distance = Vector3.Distance(transform.position, playerTransform.position);

        // 거리가 설정값보다 가까우면 카메라 켜고, 멀면 끄기
        if (distance <= renderDistance)
        {
            if (!mirrorCamera.enabled) mirrorCamera.enabled = true;
        }
        else
        {
            if (mirrorCamera.enabled) mirrorCamera.enabled = false;
        }
    }
}