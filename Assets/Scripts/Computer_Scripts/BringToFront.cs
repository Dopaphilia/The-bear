using UnityEngine;
using UnityEngine.EventSystems; // 클릭 이벤트를 감지하기 위해 필요합니다.

public class BringToFront : MonoBehaviour, IPointerDownHandler
{
    // IPointerDownHandler 인터페이스를 구현하여 클릭(누르는 순간)을 감지합니다.
    public void OnPointerDown(PointerEventData eventData)
    {
        // 1. 이 오브젝트를 부모(Canvas 등) 내에서 가장 마지막 순서로 이동시킵니다.
        // 유니티 UI는 Hierarchy의 맨 아래에 있을수록 화면의 가장 앞에 그려집니다.
        transform.SetAsLastSibling();

        Debug.Log(gameObject.name + " 창이 맨 앞으로 이동되었습니다.");
    }
}