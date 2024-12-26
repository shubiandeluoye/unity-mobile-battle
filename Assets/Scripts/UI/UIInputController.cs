using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIInputController : MonoBehaviour
{
    public PlayerEventMovement playerMovement;
    public RectTransform joystickBackground;
    public RectTransform joystickHandle;
    
    private bool isDragging = false;
    private Vector2 joystickCenter;
    private float joystickRadius;

    void Start()
    {
        if (joystickBackground != null)
        {
            joystickRadius = joystickBackground.rect.width * 0.5f;
            joystickCenter = joystickBackground.position;
        }

        // Find player if not assigned
        if (playerMovement == null)
        {
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerMovement = player.GetComponent<PlayerEventMovement>();
            }
        }
    }

    public void OnJoystickDragStart(BaseEventData eventData)
    {
        isDragging = true;
        PointerEventData pointerData = (PointerEventData)eventData;
        joystickCenter = joystickBackground.position;
    }

    public void OnJoystickDrag(BaseEventData eventData)
    {
        if (!isDragging) return;

        PointerEventData pointerData = (PointerEventData)eventData;
        Vector2 dragPosition = pointerData.position;
        Vector2 dragDelta = dragPosition - joystickCenter;
        
        // Clamp to joystick radius
        dragDelta = Vector2.ClampMagnitude(dragDelta, joystickRadius);
        
        // Update handle position
        if (joystickHandle != null)
        {
            joystickHandle.position = joystickCenter + dragDelta;
        }

        // Calculate movement direction (-1 to 1 range)
        Vector2 movementDirection = dragDelta / joystickRadius;
        
        // Send movement to player
        if (playerMovement != null)
        {
            playerMovement.OnMove(movementDirection);
        }
    }

    public void OnJoystickDragEnd(BaseEventData eventData)
    {
        isDragging = false;
        if (joystickHandle != null)
        {
            joystickHandle.position = joystickCenter;
        }
        
        // Stop movement when joystick is released
        if (playerMovement != null)
        {
            playerMovement.OnMove(Vector2.zero);
        }
    }

    // Button event handlers
    public void OnRotateLeftButton()
    {
        if (playerMovement != null)
        {
            playerMovement.OnRotate(-1f);
        }
    }

    public void OnRotateRightButton()
    {
        if (playerMovement != null)
        {
            playerMovement.OnRotate(1f);
        }
    }
}
