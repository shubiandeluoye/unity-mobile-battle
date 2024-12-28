using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Photon.Pun;

public class MovementJoystick : MonoBehaviourPunCallbacks, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    [SerializeField] private RectTransform joystickBackground;
    [SerializeField] private RectTransform joystickHandle;
    [SerializeField] private float moveThreshold = 0.1f;
    [SerializeField] private float moveRange = 100f;
    
    private Vector2 inputVector;
    private Vector2 joystickCenter;
    private bool isDragging = false;
    private PlayerController playerController;

    private void Start()
    {
        playerController = GetComponentInParent<PlayerController>();
        if (joystickBackground == null)
            joystickBackground = GetComponent<RectTransform>();
        if (joystickHandle == null)
            joystickHandle = transform.GetChild(0).GetComponent<RectTransform>();
            
        joystickCenter = joystickBackground.position;
        inputVector = Vector2.zero;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!photonView.IsMine) return;
        
        isDragging = true;
        OnDrag(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!photonView.IsMine || !isDragging) return;

        Vector2 direction = eventData.position - joystickCenter;
        inputVector = direction.magnitude > moveRange ? 
            direction.normalized * moveRange : direction;
        
        // 更新手柄位置
        joystickHandle.position = joystickCenter + inputVector;
        
        // 发送移动输入到PlayerController
        Vector2 normalizedInput = inputVector.magnitude > moveThreshold ? 
            inputVector / moveRange : Vector2.zero;
        if (playerController != null)
        {
            playerController.OnMovementInput(normalizedInput);
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!photonView.IsMine) return;
        
        isDragging = false;
        inputVector = Vector2.zero;
        joystickHandle.position = joystickCenter;
        
        // 停止移动
        if (playerController != null)
        {
            playerController.OnMovementInput(Vector2.zero);
        }
    }
}
