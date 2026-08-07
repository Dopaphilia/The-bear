using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FrontDoorEventInteractable : MonoBehaviour
{
    [Header("Position & Rotation Setup")]
    [Tooltip("현관문을 열 때 뒤로 물러날 위치 (빈 오브젝트)")]
    public Transform stepBackPoint;

    [Tooltip("현관문을 열고 바라볼 시선 고정 위치 (빈 오브젝트)")]
    public Transform lookAtPoint;

    [Tooltip("180도 돌아서 뒤를 바라볼 위치 (빈 오브젝트 / 비어있으면 정확히 180도 회전)")]
    public Transform lookBehindPoint;

    [Header("Animation Timings")]
    [Tooltip("지정된 위치로 물러나거나 시선을 회전할 때의 이동 시간 (초)")]
    public float moveDuration = 1.0f;

    [Tooltip("180도 몸과 시선을 돌릴 때의 회전 시간 (초)")]
    public float turnDuration = 0.8f;

    [Header("Step 1: 문 열었을 때 대화")]
    [Tooltip("현관문을 열고 시선이 고정된 후 출력할 대화 (예: '할머니 안녕하십니까?')")]
    [TextArea(2, 5)]
    public string[] dialogueBeforeTurn = new string[] {
        "어..? 할머니..?"
    };

    [Header("Step 2: 택배 던지기 이벤트")]
    [Tooltip("뒤에서 날아올 택배 박스 오브젝트 (비활성화 상태로 씬에 배치 추천)")]
    public GameObject deliveryBoxObject;

    [Tooltip("택배 박스가 날아오기 시작할 출발 위치 (빈 오브젝트 - 할머니 손/문 앞 위치 추천)")]
    public Transform boxSpawnPoint;

    [Tooltip("택배 박스가 포물선을 그리며 날아와 착지할 목표 위치 (빈 오브젝트 - 방 바닥/복도 바닥 추천)")]
    public Transform boxLandingPoint;

    [Tooltip("택배 박스가 공중에 떠서 날아오는 시간 (초)")]
    public float throwDuration = 0.65f;

    [Tooltip("택배 박스가 날아올 때 포물선의 최고 높이")]
    public float throwArcHeight = 1.5f;

    [Tooltip("택배 박스를 Rigidbody로 던질 경우 적용할 보조 물리 힘 (LandingPoint 미사용 시 적용)")]
    public Vector3 boxThrowVelocity = new Vector3(0f, 2f, 4f);

    [Tooltip("택배 박스 투척 시 재생할 효과음")]
    public AudioSource throwSound;

    [Tooltip("택배가 던져진 후 다시 문 쪽을 돌아볼 때까지의 대기 시간 (초)")]
    public float afterThrowDelay = 1.5f;

    [Header("Step 3: 문 닫은 후 반응 대화 (선택사항)")]
    [Tooltip("이벤트가 끝나고 문을 닫은 뒤 출력할 대화")]
    [TextArea(2, 5)]
    public string[] dialogueAfterEvent = new string[] {
        "뭐지.. 택배를 왜 던지시지..?"
    };
}
