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
    private float currentCameraRotationX = 0f; // 카메라 회전 변수 (상하)
    
    [Header("Jumping")]
    [SerializeField] private float jumpForce = 6.5f; // 초기 속도
    [SerializeField] private float gravity = 20f; // 수동으로 적용할 중력 값

    [Header("Camera")]
    [SerializeField] private float lookSensitivity = 200f; // 회전 감도
    [SerializeField] private float cameraRotationLimitX = 65f; // 카메라 고정 각도 (상하)
    [SerializeField] private Camera playerCamera;

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
    public Transform leftArmBone;
    public Transform leftFrontArmBone;
    public Transform leftHandBone;
    public Vector3 armBoneRot;
    public Vector3 frontArmBoneRot;
    public Vector3 leftHandBoneRot;
    
    [Header("Arm Tracking Settings")]
    [Range(0f, 2f)] public float armFollowSensitivity = 0.5f;
    public Vector3 armTrackingAxis = new Vector3(1,0,0);

    private Animator anim;
    public LayerMask interactionLayer;

    [Header("Routine State")]
    private bool isHandlingRoutine = false;

    [Header("Sleep UI")]
    public CanvasGroup sleepCanvasGroup; // 위에서 만든 Canvas의 CanvasGroup 연결
    public TMPro.TextMeshProUGUI dayTextUI; // DayText 연결

    void Start()
    {
        controller = GetComponent<CharacterController>(); 
        anim = GetComponentInChildren<Animator>();
        Cursor.lockState = CursorLockMode.Locked; // 커서 잠금 
        Cursor.visible = false; // 커서 안보이게
    }

    void Update()
    {
        if (isPeeping || isHandlingRoutine)
        {
            CheckExitPeeping();
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
        if (lighterObject != null && lighterObject.activeSelf)
        {
            if (leftArmBone != null)
            {
                float cameraAngle = currentCameraRotationX * armFollowSensitivity;
                leftArmBone.localRotation *= Quaternion.Euler(armBoneRot + (cameraAngle*armTrackingAxis));
                leftFrontArmBone.localRotation *= Quaternion.Euler(frontArmBoneRot);
                leftHandBone.localRotation *= Quaternion.Euler(leftHandBoneRot);
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
        
        // 렌즈 카메라 켜기
        currentPeephole.EnableView();
    }

    // [추가] 렌즈 보기 종료 (원래대로 복구)
    void StopPeeping()
    {
        if (currentPeephole != null)
        {
            currentPeephole.DisableView();
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

        // [추가됨 1] 라이터를 들고 있었다면 잠시 숨기기 (주머니에 넣는 연출)
        bool wasHoldingLighter = isHolding; // 루틴 시작 전 상태 기억
        
        if (wasHoldingLighter && lighterObject != null)
        {
            lighterObject.SetActive(false); // 모델 숨기기
            isHolding = false; // 상태 변수 끄기
            if (anim != null) anim.SetBool("isHolding", false); // 팔 드는 애니메이션 해제
            
            // LateUpdate에 있는 팔 IK 로직도 lighterObject.activeSelf 체크 덕분에 자동으로 멈춤
        }

        // 걷기 애니메이션 초기화
        if (anim != null)
        {
            anim.SetBool("isWalking", false);
            anim.SetBool("isRunning", false);
            anim.SetBool("isHolding", false);
            anim.CrossFade("Standing Idle", 0.2f);
        }
        moveVelocity = Vector3.zero;

        // --- 이동 및 시선 고정 (기존 로직) ---
        float elapsed = 0;
        float duration = 1.0f; 
        Vector3 startPos = transform.position;
        Quaternion startRot = transform.rotation;
        float startCamX = currentCameraRotationX;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime * sink.transitionSpeed;
            float t = elapsed / duration;

            transform.position = Vector3.Lerp(startPos, sink.standPoint.position, t);

            Vector3 targetDir = (sink.lookAtPoint.position - playerCamera.transform.position).normalized;
            Quaternion targetFullRot = Quaternion.LookRotation(targetDir);
            float targetY = targetFullRot.eulerAngles.y;
            transform.rotation = Quaternion.Slerp(startRot, Quaternion.Euler(0, targetY, 0), t);

            float targetX = targetFullRot.eulerAngles.x;
            if (targetX > 180) targetX -= 360; 
            currentCameraRotationX = Mathf.Lerp(startCamX, targetX, t);
            playerCamera.transform.localEulerAngles = new Vector3(currentCameraRotationX, 0, 0);

            yield return null;
        }

        // 소리 재생
        sink.PlayWaterSound(); 

        // 대기 (손 씻는 중)
        yield return new WaitForSeconds(sink.routineDuration);

        // 소리 끄기
        sink.StopWaterSound();
        
        // [추가됨 2] 원래 들고 있었다면 다시 꺼내기
        if (wasHoldingLighter && lighterObject != null)
        {
            lighterObject.SetActive(true); // 모델 다시 표시
            isHolding = true; // 상태 복구
            if (anim != null) anim.SetBool("isHolding", true); // 팔 다시 들기
        }

        isHandlingRoutine = false;
        Debug.Log("손 씻기 완료");
    }

    public void StartSleepRoutine(BedInteractable bed)
{
    StartCoroutine(SleepRoutineCoroutine(bed));
}

IEnumerator SleepRoutineCoroutine(BedInteractable bed)
{
    isHandlingRoutine = true;
    moveVelocity = Vector3.zero;

    // 1. 침대 위치로 이동 및 시선 고정 (SinkRoutine 로직 재활용)
    float elapsed = 0;
    float moveDuration = 1.0f;
    Vector3 startPos = transform.position;
    Quaternion startRot = transform.rotation;

    while (elapsed < moveDuration)
    {
        elapsed += Time.deltaTime * 2.0f; // 이동 속도
        float t = elapsed / moveDuration;
        transform.position = Vector3.Lerp(startPos, bed.sleepPoint.position, t);
        
        // 시선 처리 (누운 자세 연출)
        Vector3 targetDir = (bed.lookAtPoint.position - playerCamera.transform.position).normalized;
        transform.rotation = Quaternion.Slerp(startRot, Quaternion.LookRotation(new Vector3(targetDir.x, 0, targetDir.z)), t);
        yield return null;
    }

    // 2. 화면 어두워지기 (Fade Out)
    if (GameManager.Instance != null)
    {
        dayTextUI.text = GameManager.Instance.GetNextDayText(); 
    }
    else
    {
        // 만약 GameManager가 없다면 임시로 표시 (에러 방지용)
        dayTextUI.text = "DAY ?";
    }
    elapsed = 0;
    while (elapsed < bed.fadeDuration)
    {
        elapsed += Time.deltaTime;
        sleepCanvasGroup.alpha = Mathf.Clamp01(elapsed / bed.fadeDuration);
        yield return null;
    }

    // 3. 검은 화면 유지 (여기서 게임 시간이나 날짜 데이터를 넘기면 좋습니다)
    yield return new WaitForSeconds(bed.blackScreenHoldTime);

    // 4. 화면 다시 밝아지기 (Fade In)
    elapsed = 0;
    while (elapsed < bed.fadeDuration)
    {
        elapsed += Time.deltaTime;
        sleepCanvasGroup.alpha = Mathf.Clamp01(1 - (elapsed / bed.fadeDuration));
        yield return null;
    }

    isHandlingRoutine = false;
    Debug.Log("잠자기 완료");
}

    // ----- 상호작용 -----
    [SerializeField] public float interactionDistance = 3f;
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

        // raycast시각 확인용 (미충돌 빨강, 충돌 녹색)   
        Debug.DrawRay(rayOrigin, rayDirection * interactionDistance, Color.red);

        if (Physics.Raycast(rayOrigin, rayDirection, out hitInfo, interactionDistance, interactionLayer))
        {
            if (hitInfo.collider.CompareTag("Door"))
            {
                hitDistance = hitInfo.distance;
                if (hitDistance <= safeInteractionDistance)
                {
                    Debug.DrawRay(rayOrigin, rayDirection * interactionDistance, Color.green);
                    Debug.Log(hitInfo.collider.name + "과(와) 상호작용 가능");
                    if (Input.GetKeyDown(KeyCode.E))
                    {
                        Door doorScript = hitInfo.collider.GetComponentInParent<Door>();
                        if (doorScript.doorType == Door.DoorType.Rotating)
                        {
                            if (doorScript != null)
                            {
                                if (!doorScript.isDoorMoving())
                                {
                                    if (doorScript.IsPlayerInPath(transform.position)) {
                                        float moveAmount = safeInteractionDistance - hitDistance;
                                        Vector3 totalMove = -playerCamera.transform.forward * moveAmount; //카메라의 정면방향의 반대방향으로 물러남
                                        CharacterController controller = GetComponent<CharacterController>();
                                        if (controller != null)
                                        {
                                            StartCoroutine(smoothMovePlayer(controller, totalMove, 0.25f, doorScript));
                                        }
                                    }
                                    else
                                    {
                                        doorScript.doorOpen();
                                    }
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
            else if (hitInfo.collider.CompareTag("Item"))
            {
                hitDistance = hitInfo.distance;
                if (hitDistance <= ItemGetDistance)
                {
                    Debug.DrawRay(rayOrigin, rayDirection * interactionDistance, Color.green);
                    Item itemScript = hitInfo.collider.GetComponent<Item>();
                    if (itemScript.itemName == "Lighter")
                    {
                        Debug.Log(itemScript.itemName + " 획득 가능");
                        if (Input.GetKeyDown(KeyCode.E))
                        {
                            if (itemScript != null && hitDistance <= ItemGetDistance && hasLighter == false)
                            {
                                Debug.Log(itemScript.itemName + " 획득");
                                hitInfo.collider.gameObject.SetActive(false);
                                if (itemScript.linkedSpot != null)
                                {
                                    itemScript.linkedSpot.SetActive(true);
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
                    if (itemScript.itemName == "Candle")
                    {
                        CandleController candle = hitInfo.collider.GetComponentInParent<CandleController>();
                        if (candle != null && hitDistance <= ItemGetDistance) {
                            if (hasLighter && !candle.isFire)
                            {
                                Debug.Log("E : 향 피우기");
                                if (Input.GetKeyDown(KeyCode.E))
                                {
                                    if (itemScript != null && hitDistance <= ItemGetDistance)
                                    {
                                        candle.IgniteCandle();
                                        Debug.Log("향을 피웠습니다.");
                                    }
                                }
                            }
                            else if (candle.isFire)
                            {
                                Debug.Log("E : 향 끄기");
                                if (Input.GetKeyDown(KeyCode.E))
                                {
                                    if (itemScript != null && hitDistance <= ItemGetDistance)
                                    {
                                        candle.ExtinguishCandle();
                                        Debug.Log("향을 껐습니다.");
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
                    Debug.Log("E : 라이터 놓기");

                    if (Input.GetKeyDown(KeyCode.E))
                    {
                        ItemSpot spotScript = hitInfo.collider.GetComponent<ItemSpot>();
                        if (spotScript != null)
                        {
                            lighterObject.SetActive(false);
                            isHolding = false;
                            hasLighter = false;
                            spotScript.originalItem.SetActive(true);
                            hitInfo.collider.gameObject.SetActive(false);
                        }
                    }
                }
            }
            else if (hitInfo.collider.CompareTag("Peephole"))
            {
                hitDistance = hitInfo.distance;
                // 렌즈는 문보다 가까이서 봐야 하므로 거리를 짧게 잡습니다.
                if (hitDistance <= 1.0f) 
                {
                    Debug.DrawRay(rayOrigin, rayDirection * interactionDistance, Color.green);
                    // UI 표시 (예: "E : 살펴보기") 등을 띄우는 코드 추가 가능
                    Debug.Log("E : 살펴보기");
                    if (Input.GetKeyDown(KeyCode.E))
                    {
                        DoorPeephole peepholeScript = hitInfo.collider.GetComponent<DoorPeephole>();
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
                // 냉장고 상호작용 가능 거리 설정 (기존 ItemGetDistance 등 활용 가능)
                if (hitDistance <= 2.0f) 
                {
                    Debug.DrawRay(rayOrigin, rayDirection * interactionDistance, Color.green);
                    Debug.Log("E : 냉장고 열기/닫기");

                    if (Input.GetKeyDown(KeyCode.E))
                    {
                        // 부모 객체에 있는 FridgeManager를 찾음
                        FridgeManager fridge = hitInfo.collider.GetComponentInParent<FridgeManager>();
                        
                        if (fridge != null)
                        {
                            // 현재 Ray가 맞은 문(hitInfo.collider.gameObject)을 넘겨줌
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
                    if (Input.GetKeyDown(KeyCode.E))
                    {
                        SinkInteractable sinkData = hitInfo.collider.GetComponent<SinkInteractable>();
                        if (sinkData != null)
                        {
                            StartSinkRoutine(sinkData);
                        }
                    }
                }
            }
            else if (hitInfo.collider.CompareTag("Bed"))
            {
                hitDistance = hitInfo.distance;
                if (hitDistance <= 2.0f) 
                {
                    if (Input.GetKeyDown(KeyCode.E))
                    {
                        BedInteractable bedData = hitInfo.collider.GetComponent<BedInteractable>();
                        if (bedData != null)
                        {
                            StartSleepRoutine(bedData);
                        }
                    }
                }
            }
        }
    }
    // 문과 상호작용 시 뒤로 밀려나는 코루틴
    IEnumerator smoothMovePlayer(CharacterController controller, Vector3 direction, float duration, Door doorToOpen)
    {
        float elapsedTime = 0; // 경과 시간
        while (elapsedTime < duration)
        {
            controller.Move(direction * (Time.deltaTime / duration));
            elapsedTime += Time.deltaTime;
            yield return null; // 다음 프레임까지 대기 후 Loop
        }
        if (doorToOpen != null)
        {
            doorToOpen.doorOpen();
        }
    }

    // ----- 플레이어 시선 -----
    public void moveCamera()
    {
        // 좌우 (몸 전체 회전)
        // 카메라의 Y는 마우스의 X축을 이동해야 이동
        float cameraY = Input.GetAxisRaw("Mouse X") * lookSensitivity * Time.deltaTime;
        transform.Rotate(Vector3.up * cameraY); // Vector3.up -> (0.1.0)
        // 상하 (카메라만 회전)
        // 카메라의 X는 마우스의 Y축을 이동해야 이동
        float cameraX = Input.GetAxisRaw("Mouse Y") * lookSensitivity * Time.deltaTime;
        currentCameraRotationX -= cameraX;
        currentCameraRotationX = Mathf.Clamp(currentCameraRotationX, -cameraRotationLimitX, cameraRotationLimitX); // 최대값 제한
        playerCamera.transform.localEulerAngles = new Vector3(currentCameraRotationX, 0f, 0f);
    }
}