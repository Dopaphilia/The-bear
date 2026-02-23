using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// 기본 ScrollRect의 기능을 그대로 가져오되, 드래그 관련 기능만 무효화합니다.
public class WheelOnlyScroll : ScrollRect
{
    public override void OnBeginDrag(PointerEventData eventData) { }
    public override void OnDrag(PointerEventData eventData) { }
    public override void OnEndDrag(PointerEventData eventData) { }
}