using UnityEngine;
using UnityEngine.EventSystems;

public sealed class MobileJoystick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    [SerializeField] private RectTransform knob;
    [SerializeField, Range(0f,.9f)] private float deadZone = .12f;
    private int? activePointer;
    public Vector2 Value { get; private set; }
    public void SetKnob(RectTransform target)=>knob=target;

    public void OnPointerDown(PointerEventData data)
    {
        if(activePointer.HasValue)return;
        activePointer=data.pointerId;
        OnDrag(data);
    }
    public void OnDrag(PointerEventData data)
    {
        if(activePointer!=data.pointerId)return;
        var rect=(RectTransform)transform;
        if(!RectTransformUtility.ScreenPointToLocalPointInRectangle(rect,data.position,data.pressEventCamera,out var local))return;
        float radius=Mathf.Min(rect.rect.width,rect.rect.height)*.5f;
        Vector2 raw=Vector2.ClampMagnitude(local/Mathf.Max(radius,1),1);
        float magnitude=raw.magnitude;
        Value=magnitude<deadZone?Vector2.zero:raw.normalized*((magnitude-deadZone)/(1-deadZone));
        if(knob)knob.anchoredPosition=raw*radius*.65f;
    }
    public void OnPointerUp(PointerEventData data)
    {
        if(activePointer==data.pointerId)ResetInput();
    }
    private void OnDisable()=>ResetInput();
    private void OnApplicationFocus(bool focus){if(!focus)ResetInput();}
    private void ResetInput()
    {
        activePointer=null;Value=Vector2.zero;
        if(knob)knob.anchoredPosition=Vector2.zero;
    }
}
