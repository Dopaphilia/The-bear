using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Assertions.Must;
using UnityEngine.Experimental.AI;

// RequireComponent(typeof(type)) : 해당type를 스크립트가 반드시 요구
[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] float playerSpeed = 5f;
    [SerializeField] float runSpeed = 8f;
    private bool isRunning = false;
    private Vector3 moveVelocity; // X, Y, Z 모든 움직임을 담을 변수
    public float currentCameraRotationX = 0f; // 카메라 회전 변수 (상하)
    
    [Header("Jumping")]
    [SerializeField] private float jumpForce = 6.5f; // 초기 속도
    [SerializeField] private float gravity = 20f; // 수동으로 적용할 중력 값

    [Header("Camera")]
    [SerializeField] private float lookSensitivity = 2f; // 회전 감도 (최적화됨)
    [SerializeField] private float cameraRotationLimitX = 65f; // 카메라 고정 각도 (상하)
    [SerializeField] public Camera playerCamera;

    [Header("Camera Position Sync (No Lag)")]
    public Transform headBone; // 머리뼈 (위치 동기화용)
    public Vector3 cameraOffset = Vector3.zero; // 눈 위치 미세 조정

    [Header("Pushing")]
    [SerializeField] private float pushForce = 2.0f;
    private CharacterController controller;
    
    [Header("Item")]
    [SerializeField] private GameObject lighterObject;
    private bool isHolding = false;
    private bool hasLighter = false;

    [Header("Peephole")]
    private bool isPeeping = false;
    private DoorPeephole currentPeephole;

    // 팔 각도조정 (임시 / 애니메이션 만들어진다면 제거)
    [Header("LeftArm")]
    public Transform leftShoulderBone; // 쇄골(어깨뼈) 추가
    public Transform leftArmBone;
    public Transform leftFrontArmBone;
    public Transform leftHandBone;
    public Vector3 shoulderBoneRot;
    public Vector3 armBoneRot;
    public Vector3 frontArmBoneRot;
    public Vector3 leftHandBoneRot;
    
    [Header("Arm Tracking Settings")]
    [Range(0f, 2f)] public float armFollowSensitivity = 0.5f; // 혹시 나중에 쓸 수 있으니 변수는 남겨둠

    [Header("Spine Tracking Settings")]
    public Transform spineBone; // 허리/척추 뼈
    public Vector3 spineTrackingAxis = new Vector3(1, 0, 0); // 척추가 꺾일 축
    [Range(0f, 1f)] public float spineFollowSensitivity = 0.5f; // 카메라 시선에 맞춰 척추가 꺾이는 정도

    private Animator anim;
    public LayerMask interactionLayer;

    [Header("Routine State")]
    public bool isHandlingRoutine = false;

    [Header("Story Progress")]
    public bool hasUsedToilet = false; // 화장실 상호작용 완료 여부
    public bool hasFinishedYearbook = false;
    public bool hasCheckedPeepholeAfterYearbook = false;
    public bool hasCompletedFrontDoorEvent = false; // 1일차 현관문 택배 투척 연출 완료 여부
    public bool hasOpenedPost = false; // 택배 열기 완료 여부
    public bool hasAcquiredLetter = false; // 편지 획득 완료 여부
    public bool hasPlacedLetterOnDesk = false; // 책상 편지 확인 완료 여부
    public bool hasCompletedPhoneCall = false; // 전화 통화 완료 여부
    public bool hasTakenMedicine = false; // 영양제(약) 복용 완료 여부
    public bool hasWashedHands = false; // 싱크대 손 씻기 완료 여부
    [Tooltip("캐릭터 손에 쥐어질 편지 오브젝트 (선택사항 - LetterInteractable에서 자동 제어 가능)")]
    public GameObject handLetterObject;

    [Header("UI")]
    public UnityEngine.UI.Image blackFadeImage;
    public TMPro.TextMeshProUGUI dayTextUI;
    public TMPro.TextMeshProUGUI interactionTextUI;
    void Start()
    {
        controller = GetComponent<CharacterController>(); 
        anim = GetComponentInChildren<Animator>();
        Cursor.lockState = CursorLockMode.Locked; // 커서 잠금 
        Cursor.visible = false; // 커서 안보이게
    }

    void Update()
    {
        // 대화창이 켜져있으면 플레이어 이동 및 시선 조작 잠금 (+ 이동 애니메이션 해제)
        if (DialogueManager.Instance != null && DialogueManager.Instance.dialoguePanel.activeSelf)
        {
            if (anim != null)
            {
                anim.SetBool("isWalking", false);
                anim.SetBool("isRunning", false);
            }
            return;
        }

        if (isPeeping)
        {
            CheckExitPeeping();
            return;
        }

        if (isHandlingRoutine)
        {
            return;
        }

        Move();
        Jump();
        // controller.Move()가 모든 것을 처리, Time.deltaTime 필수
        controller.Move(moveVelocity * Time.deltaTime);
        moveCamera();
        interaction();
    }

    void LateUpdate()
    {
        // 1. 시선에 맞춰 허리(척추) 굽히기 (상호작용 연출 중이나 엿보기 중이 아닐 때만 적용하여 싱크대/침대 클리핑 방지!)
        if (spineBone != null && !isHandlingRoutine && !isPeeping)
        {
            float spineAngle = currentCameraRotationX * spineFollowSensitivity;
            spineBone.localRotation *= Quaternion.Euler(spineAngle * spineTrackingAxis);
        }

        // 2. 카메라 위치 동기화 (허리가 굽혀진 이후의 머리뼈 위치를 복사해 와야 합니다!)
        if (headBone != null && playerCamera != null)
        {
            playerCamera.transform.position = headBone.position + headBone.TransformDirection(cameraOffset);
        }

        // 3. 라이터나 편지(아이템) 들었을 때의 팔 고정 (카메라 추적은 척추가 대신 하므로 뺌)
        if (isHolding || (lighterObject != null && lighterObject.activeSelf) || (handLetterObject != null && handLetterObject.activeSelf) || (hasAcquiredLetter && !hasPlacedLetterOnDesk))
        {
            // 쇄골(Shoulder/Clavicle) 각도 적용
            if (leftShoulderBone != null)
            {
                leftShoulderBone.localEulerAngles = shoulderBoneRot;
            }

            if (leftArmBone != null)
            {
                // 팔은 인스펙터에 적어둔 각도로 고정만 함 (위아래 움직임은 허리가 담당)
                leftArmBone.localEulerAngles = armBoneRot;
                leftFrontArmBone.localEulerAngles = frontArmBoneRot;
                leftHandBone.localEulerAngles = leftHandBoneRot;
            }
        }
    }

    void CheckExitPeeping()
    {
        // E키나 ESC키를 누르면 렌즈 보기 종료
        if (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Escape))
        {
            StopPeeping();
        }
    }
    void StartPeeping(DoorPeephole peephole)
    {
        isPeeping = true;
        currentPeephole = peephole;
        
        // 렌즈 볼 때 상호작용 UI/로고 즉시 숨기기!
        if (interactionTextUI != null)
        {
            interactionTextUI.gameObject.SetActive(false);
        }

        if (hasFinishedYearbook)
        {
            hasCheckedPeepholeAfterYearbook = true; // 졸업앨범 이후 렌즈를 확인했으므로 현관문 열기 가능!
        }

        // 1. 이벤트 오브젝트 활성화/비활성화
        if (peephole.objectsToActivateOnPeep != null)
        {
            foreach (GameObject obj in peephole.objectsToActivateOnPeep)
            {
                if (obj != null) obj.SetActive(true);
            }
        }
        if (peephole.objectsToDeactivateOnPeep != null)
        {
            foreach (GameObject obj in peephole.objectsToDeactivateOnPeep)
            {
                if (obj != null) obj.SetActive(false);
            }
        }

        // 2. 효과음 재생
        peephole.PlayStartSound();
        peephole.PlayEventSound();

        // 3. 렌즈 카메라 및 UI 화면 켜기
        peephole.EnableView();

        // 4. 대화 시작
        if (DialogueManager.Instance != null && peephole.dialogueOnPeep != null && peephole.dialogueOnPeep.Length > 0)
        {
            DialogueManager.Instance.StartDialogue(peephole.dialogueOnPeep);
            StartCoroutine(AutoStopPeepingWhenDialogueEndsCoroutine());
        }
    }

    IEnumerator AutoStopPeepingWhenDialogueEndsCoroutine()
    {
        // 1. 대화창이 활성화될 때까지 잠시 대기 (최대 1초)
        float waitTime = 0f;
        while (waitTime < 1f)
        {
            if (DialogueManager.Instance != null && DialogueManager.Instance.IsDialogueActive)
                break;
            waitTime += Time.deltaTime;
            yield return null;
        }

        if (DialogueManager.Instance == null || !DialogueManager.Instance.IsDialogueActive)
        {
            yield break;
        }

        // 2. 대화가 진행되는 동안 대기 (사용자가 E/ESC로 먼저 렌즈 보기를 끈 경우 즉시 코루틴 종료)
        while (DialogueManager.Instance != null && DialogueManager.Instance.IsDialogueActive)
        {
            if (!isPeeping) yield break;
            yield return null;
        }

        // 3. 대화가 끝났고 여전히 렌즈를 보고 있는 상태라면 자동으로 렌즈 보기 종료!
        if (isPeeping)
        {
            StopPeeping();
        }
    }

    // 렌즈 보기 종료
    void StopPeeping()
    {
        if (currentPeephole != null)
        {
            // 1. 종료 효과음 재생
            currentPeephole.PlayExitSound();

            // 2. 렌즈 카메라 및 UI 화면 끄기
            currentPeephole.DisableView();

            // 3. 종료 시 이벤트 오브젝트 활성화/비활성화
            if (currentPeephole.objectsToActivateOnExit != null)
            {
                foreach (GameObject obj in currentPeephole.objectsToActivateOnExit)
                {
                    if (obj != null) obj.SetActive(true);
                }
            }
            if (currentPeephole.objectsToDeactivateOnExit != null)
            {
                foreach (GameObject obj in currentPeephole.objectsToDeactivateOnExit)
                {
                    if (obj != null) obj.SetActive(false);
                }
            }

            // 4. 일어설 위치(exitStandPoint)가 지정되어 있으면 이동
            if (currentPeephole.exitStandPoint != null)
            {
                transform.position = currentPeephole.exitStandPoint.position;
            }

            // 5. 종료 후 대화 시작
            if (DialogueManager.Instance != null && currentPeephole.dialogueAfterPeep != null && currentPeephole.dialogueAfterPeep.Length > 0)
            {
                DialogueManager.Instance.StartDialogue(currentPeephole.dialogueAfterPeep);
            }

            currentPeephole = null;
        }
        isPeeping = false;
    }

    // 충돌시 밀어지는 기능 (알아서 실행)
    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        // 충돌한 물체의 콜라이더를 가지고 옴
        Rigidbody obj = hit.collider.attachedRigidbody;

        // 충돌 방어코드
        if (obj == null || obj.isKinematic)
        {
            return;
        }

        // 발밑 충돌 무시
        if (hit.moveDirection.y < -0.3f)
        {
            return;
        }

        // 충돌 물체 밀리는 방향
        Vector3 pushDir = new Vector3(hit.moveDirection.x, 0, hit.moveDirection.z);
        // 힘 부여
        obj.velocity = pushDir * pushForce;
    }

    // 움직임
    public void Move()
    {
        float moveDirX = Input.GetAxisRaw("Horizontal");
        float moveDirZ = Input.GetAxisRaw("Vertical");
        bool isWalking = (moveDirX != 0 || moveDirZ != 0); 
        isRunning = Input.GetKey(KeyCode.LeftShift);

        // 현재 바라보는 방향을 기준으로 계산
        Vector3 moveHorizontal = transform.right * moveDirX;
        Vector3 moveVertical = transform.forward * moveDirZ;
        Vector3 moveInput = (moveHorizontal + moveVertical).normalized;

        float currentSpeed = isRunning ? runSpeed : playerSpeed;

        moveVelocity.x = moveInput.x * currentSpeed;
        moveVelocity.z = moveInput.z * currentSpeed;
        if (anim != null)
        {
            anim.SetBool("isWalking", isWalking);
            anim.SetBool("isRunning", isRunning);
            anim.SetBool("isHolding", isHolding);
        }
    }

    // 대화나 이벤트 시작 시 애니메이션을 IDLE 상태로 즉시 변경
    public void ResetAnimationToIdle()
    {
        if (anim != null)
        {
            anim.SetBool("isWalking", false);
            anim.SetBool("isRunning", false);
            anim.Play("Standing Idle", 0, 0f);
        }
    }

    // 점프
    public void Jump()
    {
        // CharacterController의 .isGrounded는 기본제공 / .isGrounded의 값은 controller.move시에 update됨
        bool isGrounded = controller.isGrounded;

        if (isGrounded)
        {
            // 점프가 씹히지 않도록 미세하게 아래로 당기는 힘을 줌
            moveVelocity.y = -2.0f;

            if (Input.GetKeyDown(KeyCode.Space))
            {
                // AddForce가 아닌 Y 속도를 직접 설정
                // 달릴 때 점프력 증가
                float currentJumpForce = isRunning ? jumpForce * 1.1f : jumpForce; // 10% 더 높게
                moveVelocity.y = currentJumpForce;
            }
        }
        else
        {
            // 매 프레임 중력만큼 Y 속도를 감소
            moveVelocity.y -= gravity * Time.deltaTime;
        }
    }
    public void StartSinkRoutine(SinkInteractable sink)
    {
        StartCoroutine(SinkRoutineCoroutine(sink));
    }

    IEnumerator SinkRoutineCoroutine(SinkInteractable sink)
    {
        isHandlingRoutine = true;
        interactionTextUI.gameObject.SetActive(false);
        moveVelocity = Vector3.zero;

        // [라이터 처리] 루틴 시작 전 상태 기억 및 숨기기
        bool wasHoldingLighter = isHolding; 
        if (wasHoldingLighter && lighterObject != null)
        {
            lighterObject.SetActive(false);
            isHolding = false;
            if (anim != null) anim.SetBool("isHolding", false);
        }

        // 애니메이션 및 속도 초기화
        if (anim != null)
        {
            anim.SetBool("isWalking", false);
            anim.SetBool("isRunning", false);
            anim.CrossFade("Standing Idle", 0.1f); // 전환을 더 빠르게
        }
        moveVelocity = Vector3.zero;

        // --- 1단계: 거울 정면 위치로 빠르게 보정 ---
        float elapsed = 0;
        float setupDuration = 0.7f;
        Vector3 initialPos = transform.position;
        Quaternion initialRot = transform.rotation;
        float initialCamX = currentCameraRotationX;

        while (elapsed < setupDuration)
        {
            elapsed += Time.deltaTime * sink.transitionSpeed;
            float t = Mathf.Clamp01(elapsed / setupDuration);
            float smoothT = Mathf.SmoothStep(0f, 1f, t); // 부드러운 보간 적용

            transform.position = Vector3.Lerp(initialPos, sink.standPoint.position, t);

            // 시선 처리: 먼저 거울(Mirror) 정면 응시
            Vector3 mirrorDir = (sink.lookAtPoint_Mirror.position - playerCamera.transform.position).normalized;
            Quaternion mirrorFullRot = Quaternion.LookRotation(mirrorDir);
            transform.rotation = Quaternion.Slerp(initialRot, Quaternion.Euler(0, mirrorFullRot.eulerAngles.y, 0), smoothT);

            float mX = mirrorFullRot.eulerAngles.x;
            if (mX > 180) mX -= 360;
            currentCameraRotationX = Mathf.Lerp(initialCamX, mX, t);
            playerCamera.transform.localEulerAngles = new Vector3(currentCameraRotationX, 0, 0);

            yield return null;
        }

        // --- 2단계: 거울에서 싱크대로 고개 숙이며 빠르게 암전 ---
        sink.PlayWaterSound(); 
        
        elapsed = 0;
        float fadeOutDuration = 0.6f;
        float startSinkCamX = currentCameraRotationX;

        while (elapsed < fadeOutDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / fadeOutDuration);
            float smoothT = Mathf.SmoothStep(0f, 1f, t); // 부드러운 보간 적용

            // 시선 처리: 싱크대(Sink) 안쪽으로 고개 숙임
            Vector3 sinkDir = (sink.lookAtPoint_Sink.position - playerCamera.transform.position).normalized;
            float sX = Quaternion.LookRotation(sinkDir).eulerAngles.x;
            if (sX > 180) sX -= 360;
            
            currentCameraRotationX = Mathf.Lerp(startSinkCamX, sX, smoothT);
            playerCamera.transform.localEulerAngles = new Vector3(currentCameraRotationX, 0, 0);

            if (blackFadeImage != null)
            {
                Color c = blackFadeImage.color;
                c.a = t;
                blackFadeImage.color = c;
            }

            yield return null;
        }

        // --- 3단계: 짧은 암전 대기 및 시선 복구 ---
        // Inspector에서 routineDuration을 1.0~1.5 정도로 낮게 설정해 보세요.
        yield return new WaitForSeconds(sink.routineDuration); 
        float beforeFadeInCamX = currentCameraRotationX;
        
        elapsed = 0;
        float fadeInDuration = 0.8f; // 고개를 드는 연출을 위해 시간을 조금 늘림 (0.2f -> 0.8f)
        
        while (elapsed < fadeInDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / fadeInDuration);
            float smoothT = Mathf.SmoothStep(0f, 1f, t); // 부드러운 연출

            // 시선 처리: 암전 때 숙였던 고개(beforeFadeInCamX)에서 거울 높이(fMX)로 들기
            if (sink.lookAtPoint_Mirror != null)
            {
                Vector3 targetMirrorDir = (sink.lookAtPoint_Mirror.position - playerCamera.transform.position).normalized;
                float targetMX = Quaternion.LookRotation(targetMirrorDir).eulerAngles.x;
                if (targetMX > 180) targetMX -= 360;

                // t값에 따라 고개가 서서히 들어짐
                currentCameraRotationX = Mathf.Lerp(beforeFadeInCamX, targetMX, smoothT);
                playerCamera.transform.localEulerAngles = new Vector3(currentCameraRotationX, 0, 0);
            }

            // 화면 밝아지기
            if (blackFadeImage != null)
            {
                Color c = blackFadeImage.color;
                c.a = 1 - t;
                blackFadeImage.color = c;
            }
            yield return null;
        }

        // [복구] 라이터 다시 꺼내기
        if (wasHoldingLighter && lighterObject != null)
        {
            lighterObject.SetActive(true);
            isHolding = true;
            if (anim != null) anim.SetBool("isHolding", true);
        }
        hasWashedHands = true;
        if (GameManager.Instance != null) GameManager.Instance.hasWashedHands = true;
        isHandlingRoutine = false;
    }

    public void StartSleepRoutine(BedInteractable bed)
    {
        StartCoroutine(SleepRoutineCoroutine(bed));
    }

    IEnumerator SleepRoutineCoroutine(BedInteractable bed)
    {
        isHandlingRoutine = true;
        interactionTextUI.gameObject.SetActive(false);
        moveVelocity = Vector3.zero;

        // 물리 엔진 간섭 차단
        if (controller != null) controller.enabled = false;

        // 1. 침대로 이동 및 눕기 (밝은 상태에서 진행)
        float elapsed = 0;
        float moveDuration = 1.0f;
        Vector3 startPos = transform.position;
        Quaternion startRot = transform.rotation;

        // BedInteractable에서 설정한 각도대로 목표 회전값 계산
        Quaternion targetRot = Quaternion.Euler(bed.sleepRotation);

        while (elapsed < moveDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / moveDuration;
            float smoothT = Mathf.SmoothStep(0f, 1f, t); 

            transform.position = Vector3.Lerp(startPos, bed.sleepPoint.position, smoothT);
            transform.rotation = Quaternion.Slerp(startRot, targetRot, smoothT);
            yield return null;
        }

        if (GameManager.Instance != null && dayTextUI != null) 
        {
            dayTextUI.gameObject.SetActive(true); 
            dayTextUI.text = GameManager.Instance.GetNextDayText(); 
            
            Color startColor = dayTextUI.color;
            dayTextUI.color = startColor;
        }
        elapsed = 0;
        while (elapsed < bed.fadeDuration)
        {
            elapsed += Time.deltaTime;
            if (blackFadeImage != null)
            {
                Color c = blackFadeImage.color;
                c.a = Mathf.Clamp01(elapsed / bed.fadeDuration);
                blackFadeImage.color = c;
            }
            if (dayTextUI != null)
            {
                Color tc = dayTextUI.color;
                tc.a = Mathf.Clamp01(elapsed / bed.fadeDuration);
                dayTextUI.color = tc;
            }
            yield return null;
        }

        // 3. 암전 상태 유지 (이때 위치는 이미 침대 위)
        yield return new WaitForSeconds(bed.blackScreenHoldTime);


        // 4. 캐릭터 상태 원상복귀 (화면이 밝아지기 전 처리) ---
        transform.rotation = Quaternion.Euler(bed.wakeUpRotation); 
        transform.position = bed.wakeUpPoint.position; 
        currentCameraRotationX = 10.8f;
        playerCamera.transform.localEulerAngles = new Vector3(currentCameraRotationX, 0, 0);

        // 5. 화면 다시 밝아지기 (Fade In)
        elapsed = 0;
        while (elapsed < bed.fadeDuration)
        {
            elapsed += Time.deltaTime;
            if (blackFadeImage != null)
            {
                Color c = blackFadeImage.color;
                c.a = Mathf.Clamp01(1 - (elapsed / bed.fadeDuration));
                blackFadeImage.color = c;
            }
            if (dayTextUI != null)
            {
                Color tc = dayTextUI.color;
                tc.a = Mathf.Clamp01(1 - (elapsed / bed.fadeDuration));
                dayTextUI.color = tc;
            }
            yield return null;
        }

        // 모든 루틴이 끝난 후 다시 물리 활성화
        if (controller != null) controller.enabled = true;
        isHandlingRoutine = false;
    }

    public void StartToiletRoutine(ToiletInteractable toilet)
    {
        StartCoroutine(ToiletRoutineCoroutine(toilet));
    }

    IEnumerator ToiletRoutineCoroutine(ToiletInteractable toilet)
    {
        isHandlingRoutine = true;
        if (interactionTextUI != null) interactionTextUI.gameObject.SetActive(false);
        moveVelocity = Vector3.zero;

        bool wasHoldingLighter = isHolding;
        if (wasHoldingLighter && lighterObject != null)
        {
            lighterObject.SetActive(false);
            isHolding = false;
            if (anim != null) anim.SetBool("isHolding", false);
        }

        if (anim != null)
        {
            anim.SetBool("isWalking", false);
            anim.SetBool("isRunning", false);
            anim.CrossFade("Standing Idle", 0.1f);
        }

        if (controller != null) controller.enabled = false;

        // 변기 및 자식 콜라이더 잠시 끄기 (앉을 때 충돌/클리핑 방지)
        Collider[] toiletColliders = toilet.GetComponentsInChildren<Collider>();
        foreach (Collider col in toiletColliders)
        {
            if (col != null) col.enabled = false;
        }

        // 1. 변기 앉을 위치로 부드럽게 이동 및 시선 보간
        float elapsed = 0f;
        Vector3 startPos = transform.position;
        Quaternion startRot = transform.rotation;
        float startCamX = currentCameraRotationX;

        // 목표 위치(sitPoint)에서 lookAtPoint를 바라보는 회전 각도를 루프 전에 미리 딱 1번만 계산! (카메라 움직임 피드백 진동 원천 차단)
        Quaternion targetRot = startRot;
        float targetX = startCamX;
        if (toilet.sitPoint != null && toilet.lookAtPoint != null)
        {
            Vector3 targetDir = (toilet.lookAtPoint.position - toilet.sitPoint.position).normalized;
            if (targetDir != Vector3.zero)
            {
                Quaternion lookRot = Quaternion.LookRotation(targetDir);
                targetRot = Quaternion.Euler(0, lookRot.eulerAngles.y, 0);
                targetX = lookRot.eulerAngles.x;
                if (targetX > 180f) targetX -= 360f;
            }
        }

        // 1 & 2. 변기 앉을 위치로 이동하는 동시에 화면 암전(Fade Out) 진행! (커버 열기 모션 없이 자연스러운 연출)
        float totalDuration = Mathf.Max(toilet.moveDuration, toilet.fadeDuration);
        while (elapsed < totalDuration)
        {
            elapsed += Time.deltaTime;

            // 이동/시선 보간 (moveDuration 기준)
            float moveT = Mathf.Clamp01(elapsed / toilet.moveDuration);
            float smoothMoveT = Mathf.SmoothStep(0f, 1f, moveT);

            if (toilet.sitPoint != null)
            {
                transform.position = Vector3.Lerp(startPos, toilet.sitPoint.position, smoothMoveT);
            }

            if (toilet.lookAtPoint != null)
            {
                transform.rotation = Quaternion.Slerp(startRot, targetRot, smoothMoveT);
                currentCameraRotationX = Mathf.Lerp(startCamX, targetX, smoothMoveT);
                if (playerCamera != null)
                {
                    playerCamera.transform.localEulerAngles = new Vector3(currentCameraRotationX, 0f, 0f);
                }
            }

            // 동시에 화면 암전 진행 (fadeDuration 기준)
            if (blackFadeImage != null)
            {
                float fadeT = Mathf.Clamp01(elapsed / toilet.fadeDuration);
                Color c = blackFadeImage.color;
                c.a = fadeT;
                blackFadeImage.color = c;
            }

            yield return null;
        }

        if (blackFadeImage != null)
        {
            Color c = blackFadeImage.color;
            c.a = 1f;
            blackFadeImage.color = c;
        }

        // 3. 변기 효과음(선택) 및 단계별 대화/이벤트 소리 연출 진행
        toilet.PlayToiletSound();

        // [단계 1] 효과음 이전 대화 (예: "으으... 시원하다...")
        if (DialogueManager.Instance != null && toilet.dialogueBeforeSound != null && toilet.dialogueBeforeSound.Length > 0)
        {
            DialogueManager.Instance.StartDialogue(toilet.dialogueBeforeSound);
            while (DialogueManager.Instance.dialoguePanel != null && DialogueManager.Instance.dialoguePanel.activeSelf)
            {
                yield return null;
            }
        }

        // [단계 2] 중간 이벤트 효과음 ("쿵!" 소리 등) 및 긴장감 대기
        if (toilet.eventSound != null)
        {
            toilet.eventSound.Play();
            if (toilet.soundDelay > 0f)
            {
                yield return new WaitForSeconds(toilet.soundDelay);
            }
        }

        // [단계 3] 효과음 이후 대화 (예: "뭐지...?")
        if (DialogueManager.Instance != null && toilet.dialogueAfterSound != null && toilet.dialogueAfterSound.Length > 0)
        {
            DialogueManager.Instance.StartDialogue(toilet.dialogueAfterSound);
            while (DialogueManager.Instance.dialoguePanel != null && DialogueManager.Instance.dialoguePanel.activeSelf)
            {
                yield return null;
            }
        }

        // [단계 4] 모든 대화 종료 후, 일어서기 직전 변기 물 내리는 사운드 재생!
        if (toilet.flushSound != null)
        {
            toilet.flushSound.Play();
            if (toilet.flushDelay > 0f)
            {
                yield return new WaitForSeconds(toilet.flushDelay);
            }
        }

        // [단계 5] 이벤트 오브젝트 활성화/비활성화 (예: 바닥에 떨어진 물건 등장)
        if (toilet.objectsToActivateOnFinish != null)
        {
            foreach (GameObject obj in toilet.objectsToActivateOnFinish)
            {
                if (obj != null) obj.SetActive(true);
            }
        }
        if (toilet.objectsToDeactivateOnFinish != null)
        {
            foreach (GameObject obj in toilet.objectsToDeactivateOnFinish)
            {
                if (obj != null) obj.SetActive(false);
            }
        }

        // 4. 대화 종료 후 지정된 위치/회전값으로 일어서기
        if (toilet.standUpPoint != null)
        {
            transform.position = toilet.standUpPoint.position;
        }
        transform.rotation = Quaternion.Euler(toilet.standUpRotation);
        currentCameraRotationX = 0f;
        if (playerCamera != null)
        {
            playerCamera.transform.localEulerAngles = new Vector3(currentCameraRotationX, 0f, 0f);
        }

        // 5. 화면 다시 밝아지기 (Fade In)
        elapsed = 0f;
        while (elapsed < toilet.fadeDuration)
        {
            elapsed += Time.deltaTime;
            if (blackFadeImage != null)
            {
                Color c = blackFadeImage.color;
                c.a = Mathf.Clamp01(1f - (elapsed / toilet.fadeDuration));
                blackFadeImage.color = c;
            }
            yield return null;
        }
        if (blackFadeImage != null)
        {
            Color c = blackFadeImage.color;
            c.a = 0f;
            blackFadeImage.color = c;
        }

        if (wasHoldingLighter && lighterObject != null)
        {
            lighterObject.SetActive(true);
            isHolding = true;
            if (anim != null) anim.SetBool("isHolding", true);
        }

        // 제어권 복귀 시 변기 콜라이더 다시 켜기
        foreach (Collider col in toiletColliders)
        {
            if (col != null) col.enabled = true;
        }

        if (controller != null) controller.enabled = true;
        isHandlingRoutine = false;
        hasUsedToilet = true; // 화장실 상호작용 완료! Day 1 초기 제한 해제!
    }

    public void StartBookRoutine(BookInteractable book)
    {
        StartCoroutine(BookRoutineCoroutine(book));
    }

    IEnumerator BookRoutineCoroutine(BookInteractable book)
    {
        isHandlingRoutine = true;
        if (interactionTextUI != null) interactionTextUI.gameObject.SetActive(false);
        moveVelocity = Vector3.zero;

        if (anim != null)
        {
            anim.SetBool("isWalking", false);
            anim.SetBool("isRunning", false);
            anim.CrossFade("Standing Idle", 0.1f);
        }

        if (controller != null) controller.enabled = false;

        // 0-1. 책 상호작용 시작 즉시 이벤트 오브젝트 켜고 끄기 ("상호작용 즉시에 적용")
        if (book.objectsToActivateOnFinish != null)
        {
            foreach (GameObject obj in book.objectsToActivateOnFinish)
            {
                if (obj != null) obj.SetActive(true);
            }
        }
        if (book.objectsToDeactivateOnFinish != null)
        {
            foreach (GameObject obj in book.objectsToDeactivateOnFinish)
            {
                if (obj != null) obj.SetActive(false);
            }
        }

        // 0-2. 책 콜라이더 잠시 끄기 (이동 중 플레이어와의 충돌 방지)
        Collider[] bookColliders = book.GetComponentsInChildren<Collider>();
        foreach (Collider col in bookColliders)
        {
            if (col != null) col.enabled = false;
        }

        // 1. 지정된 위치(readPoint)로 부드럽게 이동 및 시선(lookAtPoint) 고정 보간
        if (book.readPoint != null || book.lookAtPoint != null)
        {
            float elapsed = 0f;
            Vector3 startPos = transform.position;
            Quaternion startRot = transform.rotation;
            float startCamX = currentCameraRotationX;

            while (elapsed < book.moveDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / book.moveDuration);
                float smoothT = Mathf.SmoothStep(0f, 1f, t);

                if (book.readPoint != null)
                {
                    transform.position = Vector3.Lerp(startPos, book.readPoint.position, smoothT);
                }

                if (book.lookAtPoint != null && playerCamera != null)
                {
                    // 싱크대(Sink) 연출과 동일하게 매 프레임 이동하는 현재 카메라 위치에서 LookAtPoint를 실시간 응시하도록 보간
                    Vector3 currentDir = (book.lookAtPoint.position - playerCamera.transform.position).normalized;
                    if (currentDir != Vector3.zero)
                    {
                        Quaternion currentLookRot = Quaternion.LookRotation(currentDir);

                        // 몸통 Y 회전
                        Quaternion targetYRot = Quaternion.Euler(0, currentLookRot.eulerAngles.y, 0);
                        transform.rotation = Quaternion.Slerp(startRot, targetYRot, smoothT);

                        // 카메라 X 고개 숙이기
                        float targetCamX = currentLookRot.eulerAngles.x;
                        if (targetCamX > 180f) targetCamX -= 360f;

                        currentCameraRotationX = Mathf.Lerp(startCamX, targetCamX, smoothT);
                        playerCamera.transform.localEulerAngles = new Vector3(currentCameraRotationX, 0f, 0f);
                    }
                }

                yield return null;
            }
        }

        // 2. 책 2D UI 패널 열기 (게임 화면보다 살짝 작은 지도 팝업 느낌)
        if (book.bookUIPanel != null)
        {
            book.bookUIPanel.SetActive(true);
        }

        // 3. [단계 1] 책을 열고 나오는 첫 대사 ("왜 얘 사진만 이상하지?")
        if (DialogueManager.Instance != null && book.dialogueBeforeSound != null && book.dialogueBeforeSound.Length > 0)
        {
            DialogueManager.Instance.StartDialogue(book.dialogueBeforeSound);
            while (DialogueManager.Instance.dialoguePanel != null && DialogueManager.Instance.dialoguePanel.activeSelf)
            {
                yield return null;
            }
        }

        // 4. 문 쾅쾅 사운드 직전에 책 UI를 닫는 옵션이 켜져 있다면 책 UI 닫기
        if (book.closeBookBeforeSound && book.bookUIPanel != null)
        {
            book.bookUIPanel.SetActive(false);
        }

        // 5. [단계 2] 현관문을 쾅쾅거리는 사운드 재생 & 긴장감 있는 대기
        if (book.doorBangSound != null)
        {
            book.doorBangSound.Play();
            if (book.soundDelay > 0f)
            {
                yield return new WaitForSeconds(book.soundDelay);
            }
        }

        // 6. [단계 3] 쾅쾅 소리 이후 반응 대사 ("현관으로 가보자...")
        if (DialogueManager.Instance != null && book.dialogueAfterSound != null && book.dialogueAfterSound.Length > 0)
        {
            DialogueManager.Instance.StartDialogue(book.dialogueAfterSound);
            while (DialogueManager.Instance.dialoguePanel != null && DialogueManager.Instance.dialoguePanel.activeSelf)
            {
                yield return null;
            }
        }

        // 7. 모든 대사 종료 후 책 UI 닫기 (아직 열려있을 경우)
        if (book.bookUIPanel != null)
        {
            book.bookUIPanel.SetActive(false);
        }

        hasFinishedYearbook = true; // 졸업앨범 대화 및 쾅쾅 사운드 이벤트 완료!
        
        // 현관문이 열려있었다면 자동으로 닫히도록 처리
        Door[] allDoors = FindObjectsOfType<Door>();
        foreach (Door d in allDoors)
        {
            if (d.isFrontDoor || d.gameObject.name.Contains("Front") || d.gameObject.name.Contains("현관") || d.gameObject.name.Contains("MainDoor"))
            {
                if (d.isOpen) d.doorOpen();
            }
        }

        // 제어권 복귀 시 책 콜라이더 다시 켜기
        foreach (Collider col in bookColliders)
        {
            if (col != null) col.enabled = true;
        }

        if (controller != null) controller.enabled = true;
        isHandlingRoutine = false;
    }

    // [1일차 현관문 택배 투척 연출 이벤트]
    IEnumerator FrontDoorEventRoutineCoroutine(FrontDoorEventInteractable eventData, Door door)
    {
        isHandlingRoutine = true;
        if (controller != null) controller.enabled = false;
        ResetAnimationToIdle();

        // 이벤트 시작 시 택배 박스는 완전히 숨겨두기
        if (eventData.deliveryBoxObject != null)
        {
            eventData.deliveryBoxObject.SetActive(false);
        }

        // 1. 현관문 열기!
        if (door != null && !door.isOpen)
        {
            door.doorOpen();
        }

        Vector3 startPos = transform.position;
        Quaternion startRot = transform.rotation;
        Quaternion startCamRot = playerCamera != null ? playerCamera.transform.rotation : Quaternion.identity;

        // 2. 지정된 위치까지 뒤로 물러나면서 시선은 빈 오브젝트(lookAtPoint)로 고정
        float elapsed = 0f;
        while (elapsed < eventData.moveDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / eventData.moveDuration);
            float smoothT = t * t * (3f - 2f * t); // SmoothStep

            if (eventData.stepBackPoint != null)
            {
                transform.position = Vector3.Lerp(startPos, eventData.stepBackPoint.position, smoothT);
            }

            if (eventData.lookAtPoint != null && playerCamera != null)
            {
                Vector3 dirToLook = (eventData.lookAtPoint.position - playerCamera.transform.position).normalized;
                if (dirToLook != Vector3.zero)
                {
                    Quaternion targetLookRot = Quaternion.LookRotation(dirToLook);
                    playerCamera.transform.rotation = Quaternion.Slerp(startCamRot, targetLookRot, smoothT);

                    Vector3 flatDir = dirToLook;
                    flatDir.y = 0;
                    if (flatDir != Vector3.zero)
                    {
                        transform.rotation = Quaternion.Slerp(startRot, Quaternion.LookRotation(flatDir), smoothT);
                    }
                }
            }
            yield return null;
        }

        // 3. 특정 대화 출력 (할머니와의 대화 등)
        if (DialogueManager.Instance != null && eventData.dialogueBeforeTurn != null && eventData.dialogueBeforeTurn.Length > 0)
        {
            DialogueManager.Instance.StartDialogue(eventData.dialogueBeforeTurn);
            while (DialogueManager.Instance.dialoguePanel != null && DialogueManager.Instance.dialoguePanel.activeSelf)
            {
                yield return null;
            }
        }

        // 4. 캐릭터의 몸과 시선을 180도 돌리기 (뒤를 보도록)
        startRot = transform.rotation;
        startCamRot = playerCamera != null ? playerCamera.transform.rotation : Quaternion.identity;
        Quaternion targetBodyRot = startRot * Quaternion.Euler(0, 180f, 0); // 기본 180도 회전
        if (eventData.lookBehindPoint != null)
        {
            Vector3 dir = (eventData.lookBehindPoint.position - transform.position);
            dir.y = 0;
            if (dir != Vector3.zero) targetBodyRot = Quaternion.LookRotation(dir);
        }

        elapsed = 0f;
        while (elapsed < eventData.turnDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / eventData.turnDuration);
            float smoothT = t * t * (3f - 2f * t);

            transform.rotation = Quaternion.Slerp(startRot, targetBodyRot, smoothT);
            if (playerCamera != null)
            {
                if (eventData.lookBehindPoint != null)
                {
                    Vector3 camDir = (eventData.lookBehindPoint.position - playerCamera.transform.position).normalized;
                    if (camDir != Vector3.zero)
                    {
                        playerCamera.transform.rotation = Quaternion.Slerp(startCamRot, Quaternion.LookRotation(camDir), smoothT);
                    }
                }
                else
                {
                    playerCamera.transform.localRotation = Quaternion.Slerp(playerCamera.transform.localRotation, Quaternion.identity, smoothT);
                }
            }
            yield return null;
        }
        transform.rotation = targetBodyRot;

        // 캐릭터가 완전히 180도 뒤를 돌아본 후 약간의 긴장감(0.2초)을 두고 택배 박스 생성 및 투척!
        yield return new WaitForSeconds(0.2f);

        // 5. 뒤에서 택배 박스 날아오기 (포물선 비행 연출 + 착지 후 물리 뒹굴기)
        if (eventData.deliveryBoxObject != null)
        {
            // 출발점 (기본: 플레이어 등 뒤 문 쪽)
            Vector3 spawnPos = transform.position - transform.forward * 1.5f + Vector3.up * 1.5f;
            if (eventData.boxSpawnPoint != null) spawnPos = eventData.boxSpawnPoint.position;

            // 착지점 (기본: 플레이어 시선 정면 바닥)
            Vector3 targetPos = transform.position + transform.forward * 2.5f;
            if (eventData.boxLandingPoint != null) targetPos = eventData.boxLandingPoint.position;

            eventData.deliveryBoxObject.transform.position = spawnPos;
            eventData.deliveryBoxObject.SetActive(true); // 돌아서서 바라본 이후에 박스 생성(활성화)!

            Rigidbody rb = eventData.deliveryBoxObject.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = true; // 착지 시에도 물리로 인해 틀어지거나 넘어지지 않도록 고정!
                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }

            Quaternion startBoxRot = eventData.deliveryBoxObject.transform.rotation;
            // 착지 시 원래 상태(또는 LandingPoint가 지정된 경우 해당 각도)로 깔끔하게 정렬!
            Quaternion targetBoxRot = eventData.boxLandingPoint != null ? eventData.boxLandingPoint.rotation : startBoxRot;

            float throwElapsed = 0f;
            float duration = eventData.throwDuration > 0f ? eventData.throwDuration : 0.65f;
            while (throwElapsed < duration)
            {
                throwElapsed += Time.deltaTime;
                float t = Mathf.Clamp01(throwElapsed / duration);
                Vector3 currentPos = Vector3.Lerp(spawnPos, targetPos, t);
                currentPos.y += Mathf.Sin(t * Mathf.PI) * eventData.throwArcHeight; // 포물선 곡선

                eventData.deliveryBoxObject.transform.position = currentPos;
                // 공중에서는 3축으로 무질서하게 뱅글뱅글 돌다가 착지 시점(t=1)에는 정확히 원래 자세(targetBoxRot)로 깔끔하게 안착!
                Vector3 wildSpinAngles = new Vector3(720f * (1f - t), 1080f * (1f - t), 360f * (1f - t));
                eventData.deliveryBoxObject.transform.rotation = targetBoxRot * Quaternion.Euler(wildSpinAngles);
                yield return null;
            }
            eventData.deliveryBoxObject.transform.position = targetPos;
            eventData.deliveryBoxObject.transform.rotation = targetBoxRot;

            // --- 1차 착지 및 첫 번째 반동 (높이 25cm 바운스) ---
            if (eventData.throwSound != null) eventData.throwSound.Play();

            Vector3 bounce1EndPos = targetPos + transform.forward * 0.15f;
            float bounce1Duration = 0.18f;
            float b1Elapsed = 0f;
            while (b1Elapsed < bounce1Duration)
            {
                b1Elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(b1Elapsed / bounce1Duration);
                Vector3 bPos = Vector3.Lerp(targetPos, bounce1EndPos, t);
                bPos.y += Mathf.Sin(t * Mathf.PI) * 0.25f; // 25cm 반동

                eventData.deliveryBoxObject.transform.position = bPos;
                eventData.deliveryBoxObject.transform.rotation = targetBoxRot;
                yield return null;
            }
            eventData.deliveryBoxObject.transform.position = bounce1EndPos;

            // --- 2차 작은 반동 (높이 8cm 바운스) ---
            if (eventData.throwSound != null) eventData.throwSound.Play();

            Vector3 bounce2EndPos = bounce1EndPos + transform.forward * 0.05f;
            float bounce2Duration = 0.12f;
            float b2Elapsed = 0f;
            while (b2Elapsed < bounce2Duration)
            {
                b2Elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(b2Elapsed / bounce2Duration);
                Vector3 bPos = Vector3.Lerp(bounce1EndPos, bounce2EndPos, t);
                bPos.y += Mathf.Sin(t * Mathf.PI) * 0.08f; // 8cm 작은 반동

                eventData.deliveryBoxObject.transform.position = bPos;
                eventData.deliveryBoxObject.transform.rotation = targetBoxRot;
                yield return null;
            }
            eventData.deliveryBoxObject.transform.position = bounce2EndPos;
            eventData.deliveryBoxObject.transform.rotation = targetBoxRot; // 딱 바르고 정갈하게 안착!
        }
        else if (eventData.throwSound != null)
        {
            eventData.throwSound.Play();
        }

        // 6. 택배가 던져지고 조금 있다가 다시 문 쪽을 바라보기
        yield return new WaitForSeconds(eventData.afterThrowDelay);

        startRot = transform.rotation;
        startCamRot = playerCamera != null ? playerCamera.transform.rotation : Quaternion.identity;
        Quaternion backToDoorRot = startRot * Quaternion.Euler(0, 180f, 0); // 기본 다시 180도 회전
        if (eventData.lookAtPoint != null)
        {
            Vector3 dir = (eventData.lookAtPoint.position - transform.position);
            dir.y = 0;
            if (dir != Vector3.zero) backToDoorRot = Quaternion.LookRotation(dir);
        }

        elapsed = 0f;
        while (elapsed < eventData.turnDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / eventData.turnDuration);
            float smoothT = t * t * (3f - 2f * t);

            transform.rotation = Quaternion.Slerp(startRot, backToDoorRot, smoothT);
            if (playerCamera != null && eventData.lookAtPoint != null)
            {
                Vector3 camDir = (eventData.lookAtPoint.position - playerCamera.transform.position).normalized;
                if (camDir != Vector3.zero)
                {
                    playerCamera.transform.rotation = Quaternion.Slerp(startCamRot, Quaternion.LookRotation(camDir), smoothT);
                }
            }
            yield return null;
        }
        transform.rotation = backToDoorRot;

        // 7. 문 닫기!
        if (door != null && door.isOpen)
        {
            door.doorOpen(); // 문 닫기
        }

        // 8. 선택적 대화 (이벤트 종료 후 대화)
        if (DialogueManager.Instance != null && eventData.dialogueAfterEvent != null && eventData.dialogueAfterEvent.Length > 0)
        {
            DialogueManager.Instance.StartDialogue(eventData.dialogueAfterEvent);
            while (DialogueManager.Instance.dialoguePanel != null && DialogueManager.Instance.dialoguePanel.activeSelf)
            {
                yield return null;
            }
        }

        // 연출 종료 및 제어권 복구
        hasCompletedFrontDoorEvent = true;
        if (controller != null) controller.enabled = true;
        isHandlingRoutine = false;
    }

    // [1일차 책상 편지 상호작용 이벤트]
    IEnumerator DeskLetterRoutineCoroutine(DeskLetterInteractable eventData)
    {
        // 책상에 상호작용하는 즉시 편지 획득 상태 끄기, 손 편지 비활성화, 책상 편지 활성화 (캐릭터 이동 및 시선 고정 제거)
        if (eventData != null)
        {
            eventData.PlaceLetter(this);
        }
        hasPlacedLetterOnDesk = true;
        isHolding = false;
        if (anim != null) anim.SetBool("isHolding", false);

        // 편지를 놓은 후 전화벨 및 통화 이벤트 코루틴 시작!
        StartCoroutine(PhoneCallAfterDeskCoroutine(eventData));

        yield break;
    }

    IEnumerator PhoneCallAfterDeskCoroutine(DeskLetterInteractable eventData)
    {
        // 1. 전화벨 소리 (eventData.phoneRingSound가 있으면 재생)
        if (eventData != null && eventData.phoneRingSound != null)
        {
            eventData.phoneRingSound.Play();
        }

        // 2. 잠시 대기 (전화벨 울리는 시간, 기본 2초)
        float delay = (eventData != null && eventData.phoneRingDelay > 0f) ? eventData.phoneRingDelay : 2.0f;
        yield return new WaitForSeconds(delay);

        // 3. 전화 통화 대화 출력
        string[] phoneDialogue = new string[] {
            "언제까지 방에만 있을거니, 엄마가 보내준 영양제 계속 먹고 있지? 떨어지면 알려줘",
            "응.."
        };
        if (eventData != null && eventData.dialogueAfterPhone != null && eventData.dialogueAfterPhone.Length > 0)
        {
            phoneDialogue = eventData.dialogueAfterPhone;
        }

        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.StartDialogue(phoneDialogue);
        }

        hasCompletedPhoneCall = true;
    }

    // [1일차 약 먹기 및 냉장고 포스트잇 응시 이벤트]
    public void StartMedicineRoutine(MedicineInteractable medicine)
    {
        StartCoroutine(MedicineRoutineCoroutine(medicine));
    }

    IEnumerator MedicineRoutineCoroutine(MedicineInteractable medicine)
    {
        isHandlingRoutine = true;
        interactionTextUI.gameObject.SetActive(false);
        moveVelocity = Vector3.zero;

        if (controller != null) controller.enabled = false;

        if (medicine != null)
        {
            medicine.InteractMedicine(this);
        }
        hasTakenMedicine = true;
        if (GameManager.Instance != null) GameManager.Instance.hasTakenSupplements = true;

        ResetAnimationToIdle();

        Vector3 startPos = transform.position;
        Quaternion startRot = transform.rotation;
        Quaternion startCamRot = playerCamera != null ? playerCamera.transform.rotation : Quaternion.identity;

        float elapsed = 0f;
        float duration = (medicine != null && medicine.moveDuration > 0f) ? medicine.moveDuration : 1.5f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float smoothT = t * t * (3f - 2f * t);

            if (medicine != null && medicine.fridgeStandPoint != null)
            {
                transform.position = Vector3.Lerp(startPos, medicine.fridgeStandPoint.position, smoothT);
            }

            if (medicine != null && medicine.postItLookAtPoint != null && playerCamera != null)
            {
                Vector3 dirToLook = (medicine.postItLookAtPoint.position - playerCamera.transform.position).normalized;
                if (dirToLook != Vector3.zero)
                {
                    Quaternion targetLookRot = Quaternion.LookRotation(dirToLook);
                    playerCamera.transform.rotation = Quaternion.Slerp(startCamRot, targetLookRot, smoothT);

                    Vector3 flatDir = dirToLook;
                    flatDir.y = 0;
                    if (flatDir != Vector3.zero)
                    {
                        transform.rotation = Quaternion.Slerp(startRot, Quaternion.LookRotation(flatDir), smoothT);
                    }
                }
            }
            yield return null;
        }

        if (medicine != null && medicine.fridgeStandPoint != null) transform.position = medicine.fridgeStandPoint.position;

        if (controller != null) controller.enabled = true;
        isHandlingRoutine = false;

        if (DialogueManager.Instance != null && medicine != null && medicine.dialogueAfterMedicine != null && medicine.dialogueAfterMedicine.Length > 0)
        {
            DialogueManager.Instance.StartDialogue(medicine.dialogueAfterMedicine);
        }
    }

    // ----- 상호작용 -----
    [SerializeField] public float interactionDistance = 3f;

    // 캐싱 변수들 (매 프레임 GetComponent 호출 방지용)
    private Collider _lastHitCollider;
    private Door _cachedDoor;
    private Item _cachedItem;
    private CandleController _cachedCandle;
    private ItemSpot _cachedItemSpot;
    private DoorPeephole _cachedPeephole;
    private FridgeManager _cachedFridge;
    private SinkInteractable _cachedSink;
    private BedInteractable _cachedBed;
    private ToiletInteractable _cachedToilet;
    private BookInteractable _cachedBook;
    private ElevatorController _cachedElevator;
    private DialogueTrigger _cachedDialogueTrigger;
    private PostInteractable _cachedPost;
    private LetterInteractable _cachedLetter;
    private DeskLetterInteractable _cachedDeskLetter;
    private MedicineInteractable _cachedMedicine;

    private bool IsInteractableObject(Collider col)
    {
        if (col == null) return false;
        if (col.CompareTag("Door") || col.CompareTag("Candle") || col.CompareTag("ItemSpot") || col.CompareTag("Desk") ||
            col.CompareTag("Toilet") || col.CompareTag("Book") || col.CompareTag("Yearbook") || col.CompareTag("Peephole") ||
            col.CompareTag("Item") || col.CompareTag("Lighter") || col.CompareTag("Fridge") || col.CompareTag("Sink") ||
            col.CompareTag("Bed") || col.CompareTag("Elevator") || col.CompareTag("Post") || col.CompareTag("Letter") ||
            col.CompareTag("Medicine"))
        {
            return true;
        }
        if (col.GetComponentInParent<CandleController>() != null) return true;
        if (col.GetComponentInParent<DoorPeephole>() != null) return true;
        if (col.GetComponentInParent<PostInteractable>() != null) return true;
        if (col.GetComponentInParent<LetterInteractable>() != null) return true;
        if (col.GetComponentInParent<DeskLetterInteractable>() != null) return true;
        if (col.GetComponentInParent<MedicineInteractable>() != null) return true;
        return false;
    }

    public void interaction()
    {
        // 시작 위치 : 카메라 현재 위치
        // 발사 방향 : 카메라가 바라보는 정면
        Vector3 rayOrigin = playerCamera.transform.position;
        Vector3 rayDirection = playerCamera.transform.forward;
        RaycastHit hitInfo; //충돌 정보를 가지고있음
        // 물러나는 거리
        float safeInteractionDistance = 1.5f;
        float ItemGetDistance = 1.2f;
        float hitDistance;

        bool canInteract = false;
        string interactText = "";

        // raycast시각 확인용 (미충돌 빨강, 충돌 녹색)   
        Debug.DrawRay(rayOrigin, rayDirection * interactionDistance, Color.red);

        if (Physics.Raycast(rayOrigin, rayDirection, out hitInfo, interactionDistance, interactionLayer))
        {
            Collider currentHit = hitInfo.collider;
            if (_lastHitCollider != currentHit)
            {
                _lastHitCollider = currentHit;
                _cachedDoor = currentHit.GetComponentInParent<Door>();
                _cachedItem = currentHit.GetComponent<Item>();
                _cachedCandle = currentHit.GetComponentInParent<CandleController>();
                _cachedItemSpot = currentHit.GetComponent<ItemSpot>();
                _cachedPeephole = currentHit.GetComponent<DoorPeephole>();
                _cachedFridge = currentHit.GetComponentInParent<FridgeManager>();
                _cachedSink = currentHit.GetComponentInParent<SinkInteractable>();
                _cachedBed = currentHit.GetComponentInParent<BedInteractable>();
                _cachedToilet = currentHit.GetComponentInParent<ToiletInteractable>();
                _cachedBook = currentHit.GetComponentInParent<BookInteractable>();
                _cachedElevator = currentHit.GetComponentInParent<ElevatorController>();
                _cachedDialogueTrigger = currentHit.GetComponent<DialogueTrigger>();
                _cachedPost = currentHit.GetComponentInParent<PostInteractable>();
                _cachedLetter = currentHit.GetComponentInParent<LetterInteractable>();
                _cachedDeskLetter = currentHit.GetComponentInParent<DeskLetterInteractable>();
                _cachedMedicine = currentHit.GetComponentInParent<MedicineInteractable>();
            }

            // [핵심 로직] 상호작용 가능한 물체가 아닌 곳(벽, 바닥, 일반 배경 물체 등)에 E키를 누른 경우 제한 대사조차 띄우지 않고 건너뜀!
            if (!IsInteractableObject(hitInfo.collider) && Input.GetKeyDown(KeyCode.E))
            {
                return;
            }

            // [공통 규칙] 손에 물건이 있는 상태에서 문 열기 및 당연한 상호작용(라이터로 향초 피우기, 물건 놓기, 편지로 책상 상호작용) 외 시도 시 대사 출력!
            bool isHoldingAnything = isHolding || (hasAcquiredLetter && !hasPlacedLetterOnDesk);
            if (isHoldingAnything && Input.GetKeyDown(KeyCode.E))
            {
                bool isAllowedWhenHolding = false;

                if (hitInfo.collider.CompareTag("Door"))
                {
                    // 1. 문 열기/닫기는 언제든 허용
                    isAllowedWhenHolding = true;
                }
                else if (hasLighter && (hitInfo.collider.CompareTag("Candle") || hitInfo.collider.GetComponentInParent<CandleController>() != null))
                {
                    // 2. 라이터를 든 상태에서 향초를 켜거나 끄는 것은 당연히 허용
                    isAllowedWhenHolding = true;
                }
                else if (hitInfo.collider.CompareTag("ItemSpot"))
                {
                    // 3. 들고 있는 아이템을 원래 자리(ItemSpot)에 내려놓는 것은 허용
                    isAllowedWhenHolding = true;
                }
                else if ((hasAcquiredLetter && !hasPlacedLetterOnDesk) && hitInfo.collider.CompareTag("Desk"))
                {
                    // 4. 편지를 든 상태에서 책상과 상호작용하는 것은 허용
                    isAllowedWhenHolding = true;
                }

                if (!isAllowedWhenHolding)
                {
                    DialogueManager.Instance.StartDialogue(new string[] { "손에 물건이 있어.." });
                    return;
                }
            }

            // [Day 1 초기 제한] 시작 대사 이후 일반 문(화장실/방문) 열기와 변기 상호작용을 제외한 모든 상호작용(현관문 포함) 시도 시 대사 출력!
            bool isDay1NeedToilet = (GameManager.Instance != null && GameManager.Instance.currentDay == 1) && !hasUsedToilet;
            if (isDay1NeedToilet && Input.GetKeyDown(KeyCode.E))
            {
                bool isAllowedDoor = false;
                if (hitInfo.collider.CompareTag("Door"))
                {
                    Door d = _cachedDoor;
                    if (d != null)
                    {
                        bool isFront = d.isFrontDoor || d.gameObject.name.Contains("Front") || d.gameObject.name.Contains("현관") || d.gameObject.name.Contains("MainDoor");
                        if (!isFront) isAllowedDoor = true; // 현관문이 아닌 일반 문(화장실, 방문 등)만 허용!
                    }
                }

                if (!isAllowedDoor && !hitInfo.collider.CompareTag("Toilet"))
                {
                    DialogueManager.Instance.StartDialogue(new string[] { "배아파.. 화장실부터가자." });
                    return;
                }
            }

            // [Day 1 두 번째 제한] 화장실 사용 후 ~ 졸업앨범 확인 전: 일반 문 열기와 졸업앨범 상호작용을 제외한 모든 상호작용(현관문 포함) 시도 시 대사 출력!
            bool isDay1NeedBook = (GameManager.Instance != null && GameManager.Instance.currentDay == 1) && hasUsedToilet && !hasFinishedYearbook;
            if (isDay1NeedBook && Input.GetKeyDown(KeyCode.E))
            {
                bool isAllowedDoor = false;
                if (hitInfo.collider.CompareTag("Door"))
                {
                    Door d = _cachedDoor;
                    if (d != null)
                    {
                        bool isFront = d.isFrontDoor || d.gameObject.name.Contains("Front") || d.gameObject.name.Contains("현관") || d.gameObject.name.Contains("MainDoor");
                        if (!isFront) isAllowedDoor = true; // 현관문이 아닌 일반 문만 허용!
                    }
                }

                if (!isAllowedDoor && !hitInfo.collider.CompareTag("Book"))
                {
                    DialogueManager.Instance.StartDialogue(new string[] { "방에서 소리가 났던것같아.. 방으로 가보자." });
                    return;
                }
            }

            // [Day 1 세 번째 제한] 졸업앨범 확인 후 ~ 렌즈 상호작용 전: 일반 문 열기와 렌즈(Peephole) 상호작용을 제외한 모든 상호작용(현관문, 가구 등) 시도 시 대사 출력!
            bool isDay1NeedPeephole = (GameManager.Instance != null && GameManager.Instance.currentDay == 1) && hasFinishedYearbook && !hasCheckedPeepholeAfterYearbook;
            if (isDay1NeedPeephole && Input.GetKeyDown(KeyCode.E))
            {
                bool isAllowedDoor = false;
                if (hitInfo.collider.CompareTag("Door"))
                {
                    Door d = _cachedDoor;
                    if (d != null)
                    {
                        bool isFront = d.isFrontDoor || d.gameObject.name.Contains("Front") || d.gameObject.name.Contains("현관") || d.gameObject.name.Contains("MainDoor");
                        if (!isFront) isAllowedDoor = true; // 현관문이 아닌 일반 문만 허용!
                    }
                }

                if (!isAllowedDoor && !hitInfo.collider.CompareTag("Peephole"))
                {
                    DialogueManager.Instance.StartDialogue(new string[] { "누구지..? 렌즈로 확인하자." });
                    return;
                }
            }

            // [Day 1 네 번째 제한] 렌즈 확인 후 ~ 현관문 열기 전: 문 열기(일반 문 및 현관문)를 제외한 모든 상호작용(가구, 아이템 등) 시도 시 대사 출력!
            bool isDay1NeedOpenFrontDoor = (GameManager.Instance != null && GameManager.Instance.currentDay == 1) && hasCheckedPeepholeAfterYearbook && !hasCompletedFrontDoorEvent;
            if (isDay1NeedOpenFrontDoor && Input.GetKeyDown(KeyCode.E))
            {
                if (!hitInfo.collider.CompareTag("Door"))
                {
                    DialogueManager.Instance.StartDialogue(new string[] { "할머니가 왜 오셨지..? 문 열어드리자." });
                    return;
                }
            }

            // [Day 1 다섯 번째 제한] 현관문 택배 연출 후 ~ 향초 피우기 전: 일반 문 열기, 라이터 줍기, 향초 피우기를 제외한 모든 상호작용 시 대사 출력!
            bool isDay1NeedCandle = (GameManager.Instance != null && GameManager.Instance.currentDay == 1) && hasCompletedFrontDoorEvent && !GameManager.Instance.hasLitCandle;
            if (isDay1NeedCandle && Input.GetKeyDown(KeyCode.E))
            {
                bool isAllowedDoor = false;
                if (hitInfo.collider.CompareTag("Door"))
                {
                    Door d = _cachedDoor;
                    if (d != null)
                    {
                        bool isFront = d.isFrontDoor || d.gameObject.name.Contains("Front") || d.gameObject.name.Contains("현관") || d.gameObject.name.Contains("MainDoor");
                        if (!isFront) isAllowedDoor = true; // 현관문이 아닌 일반 문(방문/화장실 등)은 이동 허용!
                    }
                }

                bool isAllowedItemOrCandle = false;
                if (hitInfo.collider.CompareTag("Item") || hitInfo.collider.CompareTag("Lighter"))
                {
                    // 라이터 등 아이템 줍기는 허용!
                    isAllowedItemOrCandle = true;
                }
                else if (hitInfo.collider.CompareTag("Candle") || hitInfo.collider.GetComponentInParent<CandleController>() != null)
                {
                    // 향초 피우기도 허용!
                    isAllowedItemOrCandle = true;
                }

                if (!isAllowedDoor && !isAllowedItemOrCandle)
                {
                    DialogueManager.Instance.StartDialogue(new string[] { "향초 피워야지..." });
                    return;
                }
            }

            // [Day 1 여섯 번째 제한] 향초 피우기 완료 후 ~ 택배 열기 전: 일반 문 열기 및 택배 상호작용 외 시도 시 대사 출력!
            bool isDay1NeedPost = (GameManager.Instance != null && GameManager.Instance.currentDay == 1) && GameManager.Instance.hasLitCandle && !hasOpenedPost;
            if (isDay1NeedPost && Input.GetKeyDown(KeyCode.E))
            {
                bool isAllowedDoor = false;
                if (hitInfo.collider.CompareTag("Door"))
                {
                    Door d = _cachedDoor;
                    if (d != null)
                    {
                        bool isFront = d.isFrontDoor || d.gameObject.name.Contains("Front") || d.gameObject.name.Contains("현관") || d.gameObject.name.Contains("MainDoor");
                        if (!isFront) isAllowedDoor = true;
                    }
                }

                bool isAllowedSpot = hitInfo.collider.CompareTag("ItemSpot") && hasLighter;

                if (!isAllowedDoor && !isAllowedSpot && !hitInfo.collider.CompareTag("Post") && _cachedPost == null)
                {
                    DialogueManager.Instance.StartDialogue(new string[] { "택배는 뭐지..?" });
                    return;
                }
            }

            // [Day 1 일곱 번째 제한] 택배 열기 완료 후 ~ 편지 획득 전: 일반 문 열기 및 편지 상호작용 외 시도 시 대사 출력!
            bool isDay1NeedLetter = (GameManager.Instance != null && GameManager.Instance.currentDay == 1) && hasOpenedPost && !hasAcquiredLetter;
            if (isDay1NeedLetter && Input.GetKeyDown(KeyCode.E))
            {
                bool isAllowedDoor = false;
                if (hitInfo.collider.CompareTag("Door"))
                {
                    Door d = _cachedDoor;
                    if (d != null)
                    {
                        bool isFront = d.isFrontDoor || d.gameObject.name.Contains("Front") || d.gameObject.name.Contains("현관") || d.gameObject.name.Contains("MainDoor");
                        if (!isFront) isAllowedDoor = true;
                    }
                }

                if (!isAllowedDoor && !hitInfo.collider.CompareTag("Letter") && _cachedLetter == null)
                {
                    DialogueManager.Instance.StartDialogue(new string[] { "택배는 뭐지..?" });
                    return;
                }
            }

            // [Day 1 여덟 번째 제한] 편지 획득 완료 후 ~ 책상 상호작용 전: 일반 문 열기 및 책상 상호작용 외 시도 시 대사 출력!
            bool isDay1NeedDesk = (GameManager.Instance != null && GameManager.Instance.currentDay == 1) && hasAcquiredLetter && !hasPlacedLetterOnDesk;
            if (isDay1NeedDesk && Input.GetKeyDown(KeyCode.E))
            {
                bool isAllowedDoor = false;
                if (hitInfo.collider.CompareTag("Door"))
                {
                    Door d = _cachedDoor;
                    if (d != null)
                    {
                        bool isFront = d.isFrontDoor || d.gameObject.name.Contains("Front") || d.gameObject.name.Contains("현관") || d.gameObject.name.Contains("MainDoor");
                        if (!isFront) isAllowedDoor = true;
                    }
                }

                if (!isAllowedDoor && !hitInfo.collider.CompareTag("Desk") && _cachedDeskLetter == null)
                {
                    DialogueManager.Instance.StartDialogue(new string[] { "손에 물건이 있어.." });
                    return;
                }
            }

            // [Day 1 아홉 번째 제한] 책상 편지 확인 후 ~ 약 먹기 전: 일반 문 열기 및 약 상호작용 외 시도 시 대사 출력!
            bool isDay1NeedMedicine = (GameManager.Instance != null && GameManager.Instance.currentDay == 1) && hasPlacedLetterOnDesk && !hasTakenMedicine;
            if (isDay1NeedMedicine && Input.GetKeyDown(KeyCode.E))
            {
                bool isAllowedDoor = false;
                if (hitInfo.collider.CompareTag("Door"))
                {
                    Door d = _cachedDoor;
                    if (d != null)
                    {
                        bool isFront = d.isFrontDoor || d.gameObject.name.Contains("Front") || d.gameObject.name.Contains("현관") || d.gameObject.name.Contains("MainDoor");
                        if (!isFront) isAllowedDoor = true;
                    }
                }

                if (!isAllowedDoor && !hitInfo.collider.CompareTag("Medicine") && _cachedMedicine == null)
                {
                    DialogueManager.Instance.StartDialogue(new string[] { "약 먹어야지.." });
                    return;
                }
            }

            // [Day 1 열 번째 제한] 약 먹기 완료 후 ~ 손 씻기 전: 일반 문 열기 및 싱크대(Sink) 상호작용 외 시도 시 대사 출력!
            bool isDay1NeedWashHands = (GameManager.Instance != null && GameManager.Instance.currentDay == 1) && hasTakenMedicine && !hasWashedHands;
            if (isDay1NeedWashHands && Input.GetKeyDown(KeyCode.E))
            {
                bool isAllowedDoor = false;
                if (hitInfo.collider.CompareTag("Door"))
                {
                    Door d = _cachedDoor;
                    if (d != null)
                    {
                        bool isFront = d.isFrontDoor || d.gameObject.name.Contains("Front") || d.gameObject.name.Contains("현관") || d.gameObject.name.Contains("MainDoor");
                        if (!isFront) isAllowedDoor = true;
                    }
                }

                if (!isAllowedDoor && !hitInfo.collider.CompareTag("Sink") && _cachedSink == null)
                {
                    DialogueManager.Instance.StartDialogue(new string[] { "졸리다.. 손 씻고 자야지.." });
                    return;
                }
            }

            // [Day 1 열한 번째 제한] 손 씻기 완료 후 ~ 침대 상호작용 전: 일반 문 열기 및 침대(Bed) 상호작용 외 시도 시 대사 출력!
            bool isDay1NeedBed = (GameManager.Instance != null && GameManager.Instance.currentDay == 1) && hasWashedHands;
            if (isDay1NeedBed && Input.GetKeyDown(KeyCode.E))
            {
                bool isAllowedDoor = false;
                if (hitInfo.collider.CompareTag("Door"))
                {
                    Door d = _cachedDoor;
                    if (d != null)
                    {
                        bool isFront = d.isFrontDoor || d.gameObject.name.Contains("Front") || d.gameObject.name.Contains("현관") || d.gameObject.name.Contains("MainDoor");
                        if (!isFront) isAllowedDoor = true;
                    }
                }

                if (!isAllowedDoor && !hitInfo.collider.CompareTag("Bed") && _cachedBed == null)
                {
                    DialogueManager.Instance.StartDialogue(new string[] { "졸리다.. 자야지.." });
                    return;
                }
            }

            if (hitInfo.collider.CompareTag("Door"))
            {
                hitDistance = hitInfo.distance;
                if (hitDistance <= safeInteractionDistance)
                {
                    Door doorScript = _cachedDoor;
                    Debug.DrawRay(rayOrigin, rayDirection * interactionDistance, Color.green);
                    if (doorScript.isOpen && doorScript.isMoving == false)
                    {
                        interactText = "E : 문 닫기";
                    }
                    else if (doorScript.isOpen == false && doorScript.isMoving == false)
                    {
                        interactText = "E : 문 열기";
                    }
                    canInteract = true;

                    if (Input.GetKeyDown(KeyCode.E))
                    {
                        bool isThisFrontDoor = doorScript.isFrontDoor || 
                                               doorScript.gameObject.name.Contains("Front") || 
                                               doorScript.gameObject.name.Contains("현관") ||
                                               doorScript.gameObject.name.Contains("MainDoor");

                        if (isThisFrontDoor && hasFinishedYearbook && !hasCheckedPeepholeAfterYearbook)
                        {
                            // 졸업앨범 완료 후 렌즈를 아직 보지 않았다면 문을 열지 않고 대사 출력!
                            DialogueManager.Instance.StartDialogue(new string[] { "누구지..? 렌즈로 확인하자." });
                        }
                        else if (isThisFrontDoor && hasCheckedPeepholeAfterYearbook && !hasCompletedFrontDoorEvent)
                        {
                            // [1일차 현관문 택배 투척 이벤트 실행!]
                            FrontDoorEventInteractable frontEvent = doorScript.GetComponentInParent<FrontDoorEventInteractable>();
                            if (frontEvent == null) frontEvent = UnityEngine.Object.FindObjectOfType<FrontDoorEventInteractable>();

                            if (frontEvent != null)
                            {
                                StartCoroutine(FrontDoorEventRoutineCoroutine(frontEvent, doorScript));
                            }
                            else
                            {
                                // 연출 스크립트가 씬에 없다면 안전하게 일반 개폐 처리
                                doorScript.doorOpen();
                                hasCompletedFrontDoorEvent = true;
                            }
                        }
                        else
                        {
                            if (doorScript.doorType == Door.DoorType.Rotating)
                            {
                                if (doorScript != null)
                                {
                                    if (!doorScript.isDoorMoving())
                                    {
                                        doorScript.doorOpen();
                                    }
                                }
                            }
                            else if (doorScript.doorType == Door.DoorType.Sliding)
                            {
                                if (doorScript != null)
                                {
                                    if (!doorScript.isDoorMoving())
                                    {
                                        doorScript.doorOpen();
                                    }
                                }
                            }
                        }
                    }
                }

            }
            else if (hitInfo.collider.CompareTag("Item") || hitInfo.collider.CompareTag("Lighter") || hitInfo.collider.CompareTag("Candle"))
            {
                hitDistance = hitInfo.distance;
                if (hitDistance <= ItemGetDistance)
                {
                    Debug.DrawRay(rayOrigin, rayDirection * interactionDistance, Color.green);
                    Item itemScript = _cachedItem;
                    if (itemScript == null) itemScript = hitInfo.collider.GetComponent<Item>();
                    if (itemScript == null) itemScript = hitInfo.collider.GetComponentInParent<Item>();

                    bool isLighter = hitInfo.collider.CompareTag("Lighter") || (itemScript != null && itemScript.itemName == "Lighter");
                    bool isCandle = hitInfo.collider.CompareTag("Candle") || (itemScript != null && itemScript.itemName == "Candle") || _cachedCandle != null;

                    if (isLighter)
                    {
                        interactText = "E : 라이터 획득";
                        canInteract = true;

                        if (Input.GetKeyDown(KeyCode.E))
                        {
                            if (hitDistance <= ItemGetDistance && hasLighter == false)
                            {
                                Debug.Log("라이터 획득");
                                hitInfo.collider.gameObject.SetActive(false);
                                if (itemScript != null && itemScript.linkedSpot != null)
                                {
                                    BoxCollider spotCollider = itemScript.linkedSpot.GetComponent<BoxCollider>();
                                    if (spotCollider != null)
                                    {
                                        spotCollider.enabled = true; 
                                    }
                                }
                                if (lighterObject != null)
                                {
                                    lighterObject.SetActive(true);
                                    isHolding = true;
                                    hasLighter = true;
                                }
                            }
                        }
                    }
                    else if (isCandle)
                    {
                        CandleController candle = _cachedCandle;
                        if (candle == null) candle = hitInfo.collider.GetComponentInParent<CandleController>();

                        if (candle != null && hitDistance <= ItemGetDistance) {
                            if (hasLighter && !candle.isFire)
                            {
                                interactText = "E : 향 피우기";
                                canInteract = true;

                                if (Input.GetKeyDown(KeyCode.E))
                                {
                                    if (hitDistance <= ItemGetDistance)
                                    {
                                        candle.IgniteCandle();
                                        // 규칙 체크: 향 피우기 완료
                                        if (GameManager.Instance != null)
                                        {
                                            GameManager.Instance.hasLitCandle = true;
                                            if (GameManager.Instance.currentDay == 1 && DialogueManager.Instance != null)
                                            {
                                                DialogueManager.Instance.StartDialogue(new string[] { "택배는 뭐지..?" });
                                            }
                                        }
                                    }
                                }
                            }
                            else if (candle.isFire)
                            {
                                interactText = "E : 향 끄기";
                                canInteract = true;

                                if (Input.GetKeyDown(KeyCode.E))
                                {
                                    if (hitDistance <= ItemGetDistance)
                                    {
                                        candle.ExtinguishCandle();
                                    }
                                }
                            }
                        }
                    }
                }
            }
            else if (hitInfo.collider.CompareTag("ItemSpot"))
            {
                if (hasLighter) {
                    interactText = "E : 라이터 놓기";
                    canInteract = true;

                    if (Input.GetKeyDown(KeyCode.E))
                    {
                        ItemSpot spotScript = _cachedItemSpot;
                        if (spotScript != null)
                        {
                            lighterObject.SetActive(false);
                            isHolding = false;
                            hasLighter = false;
                            spotScript.originalItem.SetActive(true);
                            if (hitInfo.collider != null)
                            {
                                hitInfo.collider.enabled = false;
                            }
                        }
                    }
                }
            }
            else if (hitInfo.collider.CompareTag("Peephole"))
            {
                hitDistance = hitInfo.distance;
                if (hitDistance <= 1.0f) 
                {
                    Debug.DrawRay(rayOrigin, rayDirection * interactionDistance, Color.green);
                    string prompt = (_cachedPeephole != null && !string.IsNullOrEmpty(_cachedPeephole.interactPrompt)) 
                                    ? _cachedPeephole.interactPrompt : "들여다보기";
                    interactText = "E : " + prompt;
                    canInteract = true;

                    if (Input.GetKeyDown(KeyCode.E))
                    {
                        DoorPeephole peepholeScript = _cachedPeephole;
                        if (peepholeScript != null)
                        {
                            StartPeeping(peepholeScript);
                        }
                    }
                }
            }
            else if (hitInfo.collider.CompareTag("Fridge"))
            {
                hitDistance = hitInfo.distance;
                if (hitDistance <= 2.0f) 
                {
                    FridgeManager fridge = _cachedFridge;
                    Debug.DrawRay(rayOrigin, rayDirection * interactionDistance, Color.green);
                    if (hitInfo.collider.gameObject == fridge.leftDoor.gameObject) 
                    {
                        if (fridge.isLeftOpen == false && fridge.isLeftMoving == false)
                        {
                            interactText = "E : 냉장고 열기";
                        }
                        else if (fridge.isLeftOpen == true && fridge.isLeftMoving == false)
                        {
                            interactText = "E : 냉장고 닫기";
                        }
                    }
                    if (hitInfo.collider.gameObject == fridge.rightDoor.gameObject)
                    {
                        if (fridge.isRightOpen == false && fridge.isRightMoving == false)
                        {
                            interactText = "E : 냉장고 열기";
                        }
                        else if (fridge.isRightOpen == true && fridge.isRightMoving == false)
                        {
                            interactText = "E : 냉장고 닫기";
                        }
                    }

                    canInteract = true;

                    if (Input.GetKeyDown(KeyCode.E))
                    {   
                        if (fridge != null)
                        {
                            fridge.Interact(hitInfo.collider.gameObject);
                        }
                    }
                }
            }
            else if (hitInfo.collider.CompareTag("Sink"))
            {
                hitDistance = hitInfo.distance;
                if (hitDistance <= 2.0f) 
                {
                    interactText = "E : 싱크대 사용";
                    canInteract = true;

                    if (Input.GetKeyDown(KeyCode.E))
                    {
                        SinkInteractable sinkData = _cachedSink;
                        if (sinkData != null)
                        {
                            StartSinkRoutine(sinkData);
                            // 규칙 체크: 손 씻기 완료
                            if (GameManager.Instance != null) GameManager.Instance.hasWashedHands = true;
                        }
                    }
                }
            }
            else if (hitInfo.collider.CompareTag("Bed"))
            {
                hitDistance = hitInfo.distance;
                if (hitDistance <= 2.0f) 
                {
                    bool canSleep = GameManager.Instance != null && GameManager.Instance.IsAllRulesCleared();
                    interactText = "E : 잠자기";
                    canInteract = true;

                    if (Input.GetKeyDown(KeyCode.E))
                    {
                        if (canSleep)
                        {
                            BedInteractable bedData = _cachedBed;
                            if (bedData != null)
                            {
                                StartSleepRoutine(bedData);
                            }
                        }
                        else if (GameManager.Instance != null && DialogueManager.Instance != null)
                        {
                            if (!GameManager.Instance.hasLitCandle)
                            {
                                DialogueManager.Instance.StartDialogue(new string[] { "아.. 향초 피워야지..." });
                            }
                            else if (!GameManager.Instance.hasWashedHands)
                            {
                                DialogueManager.Instance.StartDialogue(new string[] { "아.. 손 씻어야지..." });
                            }
                            else if (!GameManager.Instance.hasTakenSupplements)
                            {
                                DialogueManager.Instance.StartDialogue(new string[] { "아.. 영양제 먹어야지..." });
                            }
                        }
                    }
                }
            }
            else if (hitInfo.collider.CompareTag("Toilet"))
            {
                hitDistance = hitInfo.distance;
                if (hitDistance <= 2.0f) 
                {
                    interactText = "E : 변기 사용하기";
                    canInteract = true;

                    if (Input.GetKeyDown(KeyCode.E))
                    {
                        ToiletInteractable toiletData = _cachedToilet;
                        if (toiletData != null)
                        {
                            StartToiletRoutine(toiletData);
                        }
                    }
                }
            }
            else if (hitInfo.collider.CompareTag("Book"))
            {
                hitDistance = hitInfo.distance;
                if (hitDistance <= 2.5f)
                {
                    interactText = "E : 책 읽기";
                    canInteract = true;

                    if (Input.GetKeyDown(KeyCode.E))
                    {
                        BookInteractable bookData = _cachedBook;
                        if (bookData != null)
                        {
                            StartBookRoutine(bookData);
                        }
                    }
                }
            }
            else if (hitInfo.collider.CompareTag("Elevator"))
            {
                hitDistance = hitInfo.distance;
                if (hitDistance <= 2.0f)
                {
                    Debug.DrawRay(rayOrigin, rayDirection * interactionDistance, Color.green);
                    interactText = "E : 엘리베이터 사용";
                    canInteract = true;

                    if (Input.GetKeyDown(KeyCode.E))
                    {
                        ElevatorController elevator = _cachedElevator;
                        
                        if (elevator != null)
                        {
                            elevator.Interact(hitInfo.collider.gameObject, this.gameObject);
                        }
                    }
                }
            }
            else if (_cachedDialogueTrigger != null)
            {
                hitDistance = hitInfo.distance;
                if (hitDistance <= 2.0f)
                {
                    Debug.DrawRay(rayOrigin, rayDirection * interactionDistance, Color.green);
                    interactText = "E : " + _cachedDialogueTrigger.interactPrompt;
                    canInteract = true;

                    if (Input.GetKeyDown(KeyCode.E))
                    {
                        _cachedDialogueTrigger.TriggerDialogue();
                    }
                }
            }
            else if (hitInfo.collider.CompareTag("Post"))
            {
                hitDistance = hitInfo.distance;
                if (hitDistance <= 2.5f)
                {
                    interactText = "E : 택배열기";
                    canInteract = true;

                    if (Input.GetKeyDown(KeyCode.E))
                    {
                        PostInteractable postData = _cachedPost;
                        if (postData == null) postData = hitInfo.collider.GetComponentInParent<PostInteractable>();

                        if (postData != null)
                        {
                            postData.OpenPost();
                        }
                        else
                        {
                            hitInfo.collider.gameObject.SetActive(false);
                        }
                        hasOpenedPost = true;
                    }
                }
            }
            else if (hitInfo.collider.CompareTag("Letter"))
            {
                hitDistance = hitInfo.distance;
                if (hitDistance <= 2.5f)
                {
                    interactText = "E : 획득하기";
                    canInteract = true;

                    if (Input.GetKeyDown(KeyCode.E))
                    {
                        LetterInteractable letterData = _cachedLetter;
                        if (letterData == null) letterData = hitInfo.collider.GetComponentInParent<LetterInteractable>();

                        isHolding = true;
                        if (anim != null) anim.SetBool("isHolding", true);
                        hasAcquiredLetter = true;

                        if (letterData != null)
                        {
                            letterData.AcquireLetter(this);
                            if (DialogueManager.Instance != null && letterData.dialogueOnAcquire != null && letterData.dialogueOnAcquire.Length > 0)
                            {
                                DialogueManager.Instance.StartDialogue(letterData.dialogueOnAcquire);
                            }
                        }
                        else
                        {
                            hitInfo.collider.gameObject.SetActive(false);
                            if (handLetterObject != null) handLetterObject.SetActive(true);
                            if (DialogueManager.Instance != null)
                            {
                                DialogueManager.Instance.StartDialogue(new string[] { "내가 이런 편지를 받았었나..?" });
                            }
                        }
                    }
                }
            }
            else if (hitInfo.collider.CompareTag("Desk") && (hasAcquiredLetter && !hasPlacedLetterOnDesk))
            {
                hitDistance = hitInfo.distance;
                if (hitDistance <= 2.5f)
                {
                    interactText = "E : 편지 놓기";
                    canInteract = true;

                    if (Input.GetKeyDown(KeyCode.E))
                    {
                        DeskLetterInteractable deskData = _cachedDeskLetter;
                        if (deskData == null) deskData = hitInfo.collider.GetComponentInParent<DeskLetterInteractable>();

                        if (deskData != null)
                        {
                            StartCoroutine(DeskLetterRoutineCoroutine(deskData));
                        }
                        else
                        {
                            if (handLetterObject != null) handLetterObject.SetActive(false);
                            hasPlacedLetterOnDesk = true;
                        }
                    }
                }
            }
            else if (hitInfo.collider.CompareTag("Medicine") && (hasPlacedLetterOnDesk && !hasTakenMedicine))
            {
                hitDistance = hitInfo.distance;
                if (hitDistance <= 2.5f)
                {
                    interactText = "E : 약 먹기";
                    canInteract = true;

                    if (Input.GetKeyDown(KeyCode.E))
                    {
                        MedicineInteractable medData = _cachedMedicine;
                        if (medData == null) medData = hitInfo.collider.GetComponentInParent<MedicineInteractable>();

                        if (medData != null)
                        {
                            StartMedicineRoutine(medData);
                        }
                        else
                        {
                            hitInfo.collider.gameObject.SetActive(false);
                            hasTakenMedicine = true;
                            if (GameManager.Instance != null) GameManager.Instance.hasTakenSupplements = true;
                        }
                    }
                }
            }
        }
        else
        {
            _lastHitCollider = null;
        }

        if (interactionTextUI != null)
        {
            if (isHandlingRoutine || isPeeping) 
            {
                if (interactionTextUI.gameObject.activeSelf) 
                    interactionTextUI.gameObject.SetActive(false);
                return; 
            }

            if (canInteract)
            {
                if (interactionTextUI.text != interactText) 
                {
                    interactionTextUI.text = interactText; 
                }
                
                if (!interactionTextUI.gameObject.activeSelf) 
                {
                    interactionTextUI.gameObject.SetActive(true);
                }
            }
            else
            {
                if (interactionTextUI.gameObject.activeSelf) 
                {
                    interactionTextUI.gameObject.SetActive(false);
                }
            }
        }
    }


    // ----- 플레이어 시선 -----
    public void moveCamera()
    {
        // 좌우 (몸 전체 회전)
        float cameraY = Input.GetAxisRaw("Mouse X") * lookSensitivity;
        transform.Rotate(Vector3.up * cameraY); 
        
        // 상하 (카메라 부모 회전)
        float cameraX = Input.GetAxisRaw("Mouse Y") * lookSensitivity;
        currentCameraRotationX -= cameraX;
        currentCameraRotationX = Mathf.Clamp(currentCameraRotationX, -cameraRotationLimitX, cameraRotationLimitX); // 최대값 제한
        
        // 개발자님의 날카로운 지적: CameraRot은 불필요! 위치를 복사하니까 카메라 자체를 돌리면 됩니다.
        if (playerCamera != null)
        {
            playerCamera.transform.localEulerAngles = new Vector3(currentCameraRotationX, 0f, 0f);
        }
    }
}