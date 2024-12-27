using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.OnScreen;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[AddComponentMenu("Input/On-Screen Stick")]
public class UIInputController : OnScreenControl, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    [SerializeField] private RectTransform joystickBackground;
    [SerializeField] private RectTransform joystickHandle;
    [SerializeField] private float movementRange = 50f;

    private Vector2 startPos;
    private Vector2 pointerDownPos;
    private bool isDragging;

    protected override string controlPathInternal
    {
        get => controlPath;
        set => controlPath = value;
    }

    private void Start()
    {
        if (joystickHandle == null)
            Debug.LogError("摇杆手柄未设置");
            
        startPos = joystickHandle.anchoredPosition;
        
        // 确保启用了增强型触摸支持
        EnhancedTouchSupport.Enable();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData == null)
            throw new System.ArgumentNullException(nameof(eventData));

        isDragging = true;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            joystickBackground,
            eventData.position,
            eventData.pressEventCamera,
            out pointerDownPos);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (eventData == null || !isDragging)
            return;

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            joystickBackground,
            eventData.position,
            eventData.pressEventCamera,
            out Vector2 position))
        {
            Vector2 delta = position - pointerDownPos;
            delta = Vector2.ClampMagnitude(delta, movementRange);
            joystickHandle.anchoredPosition = startPos + delta;
            Vector2 newPos = delta / movementRange;
            SendValueToControl(newPos);
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isDragging = false;
        joystickHandle.anchoredPosition = startPos;
        SendValueToControl(Vector2.zero);
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        SendValueToControl(Vector2.zero);
    }
}