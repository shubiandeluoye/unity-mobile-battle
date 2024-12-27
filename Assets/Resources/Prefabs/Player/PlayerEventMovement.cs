using UnityEngine;
using Photon.Pun;
using UnityEngine.EventSystems;

public class PlayerEventMovement : MonoBehaviour
{
    public float speed = 5.0f;
    public float rotationSpeed = 700.0f;

    private Rigidbody rb;
    private PhotonView photonView;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        photonView = GetComponent<PhotonView>();
        
        // Lock rotation to prevent unwanted physics-based rotation
        if (rb != null)
        {
            rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        }
    }

    // Called by UI events (e.g., virtual joystick)
    public void OnMove(Vector2 movement)
    {
        if (!photonView.IsMine) return;

        Vector3 moveDirection = new Vector3(movement.x, 0.0f, movement.y);
        
        // Apply movement using rigidbody for better physics interaction
        if (rb != null)
        {
            rb.velocity = moveDirection * speed;
        }
        else
        {
            transform.Translate(moveDirection * speed * Time.deltaTime, Space.World);
        }

        // Rotate the player to face movement direction if moving
        if (moveDirection != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime
            );
        }
    }

    // Called by UI button or event for rotation
    public void OnRotate(float rotationAmount)
    {
        if (!photonView.IsMine) return;
        transform.Rotate(0, rotationAmount * rotationSpeed * Time.deltaTime, 0);
    }
}