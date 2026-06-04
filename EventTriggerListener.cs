using UnityEngine;
using UnityEngine.EventSystems;
using System;

// UI 버튼의 PointerDown / PointerUp 이벤트를 코드에서 쉽게 사용할 수 있도록 만든 보조 스크립트
// FurnitureUI에서 회전 버튼을 누르고 있는 동안 가구가 계속 회전하도록 처리할 때 사용
public class EventTriggerListener : EventTrigger
{
    public Action<GameObject> onPointerDown; // 버튼을 눌렀을 때 실행할 이벤트
    public Action<GameObject> onPointerUp;   // 버튼에서 손을 뗐을 때 실행할 이벤트

    // 대상 GameObject에 EventTriggerListener가 없으면 추가하고, 있으면 기존 컴포넌트를 반환
    public static EventTriggerListener Get(GameObject go)
    {
        EventTriggerListener listener = go.GetComponent<EventTriggerListener>();

        if (listener == null)
            listener = go.AddComponent<EventTriggerListener>();

        return listener;
    }

    // UI 오브젝트를 눌렀을 때 호출
    public override void OnPointerDown(PointerEventData eventData)
    {
        onPointerDown?.Invoke(gameObject);
    }

    // UI 오브젝트에서 손을 뗐을 때 호출
    public override void OnPointerUp(PointerEventData eventData)
    {
        onPointerUp?.Invoke(gameObject);
    }
}