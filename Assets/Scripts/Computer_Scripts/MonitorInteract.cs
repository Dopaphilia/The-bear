using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using UnityEngine.UI;

public class MonitorInteract : MonoBehaviour
{
    private Camera mainCamera;
    private Camera uiCamera;

    void Start()
    {
        GameObject mainCamObj = GameObject.Find("Main_Camera");
        if (mainCamObj != null) mainCamera = mainCamObj.GetComponent<Camera>();

        GameObject uiCamObj = GameObject.Find("Camera");
        if (uiCamObj != null) uiCamera = uiCamObj.GetComponent<Camera>();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (mainCamera == null || uiCamera == null) { Start(); return; }

            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                if (hit.collider.gameObject == gameObject)
                {
                    Vector2 localPos = hit.textureCoord;
                    Vector2 screenPos = new Vector2(localPos.x * uiCamera.pixelWidth, localPos.y * uiCamera.pixelHeight);

                    PointerEventData eventData = new PointerEventData(EventSystem.current);
                    eventData.position = screenPos;

                    List<RaycastResult> results = new List<RaycastResult>();
                    EventSystem.current.RaycastAll(eventData, results);

                    // 핵심 수정: 가장 위에 있는(첫 번째) UI 요소 하나만 처리합니다.
                    // 이 부분을 아래 내용으로 통째로 교체하세요
                    if (results.Count > 0)
                    {
                        GameObject hitObject = results[0].gameObject;

                        // [수정된 부분 1] 클릭을 실제로 처리할 오브젝트(버튼 등)를 부모 계층에서 찾습니다.
                        GameObject handler = ExecuteEvents.GetEventHandler<IPointerClickHandler>(hitObject);

                        if (handler != null)
                        {
                            // 찾은 핸들러(버튼 등)에 클릭 이벤트를 보냅니다.
                            ExecuteEvents.Execute(handler, eventData, ExecuteEvents.pointerDownHandler);
                            ExecuteEvents.Execute(handler, eventData, ExecuteEvents.pointerUpHandler);
                            ExecuteEvents.Execute(handler, eventData, ExecuteEvents.pointerClickHandler);
                        }

                        // [수정된 부분 2] 입력창(InputField) 처리
                        // InputField도 부모 오브젝트에 있을 수 있으므로 GetComponentInParent를 사용합니다.
                        var legacyInput = hitObject.GetComponentInParent<InputField>();
                        if (legacyInput != null)
                        {
                            EventSystem.current.SetSelectedGameObject(legacyInput.gameObject);
                            legacyInput.OnPointerClick(eventData);
                            legacyInput.ActivateInputField();
                            Debug.Log("5. [Legacy] 입력창 활성화 성공!");
                        }
                    }
                }
            }
        }
    }
}