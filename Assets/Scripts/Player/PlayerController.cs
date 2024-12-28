using UnityEngine;
using Photon.Pun;

public class PlayerController : MonoBehaviourPunCallbacks
{
    private Rigidbody2D rb;
    private bool isMoving = false;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void OnMovementInput(Vector2 input)
    {
        if (!photonView.IsMine) return;
        
        Vector2 movement = input.normalized;
        if (rb != null)
        {
            rb.velocity = movement * 5f; // Adjust speed as needed
            isMoving = movement.magnitude > 0.1f;
        }
    }
}
