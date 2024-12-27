using UnityEngine;

public class PlayerController : MonoBehaviourPunCallbacks
{
    public float moveSpeed = 5f;
    private PhotonView photonView;
    private bool isLeftPlayer;
    private float boundaryX; // Left boundary for left player, right boundary for right player

    void Start()
    {
        photonView = GetComponent<PhotonView>();
        // Determine if this is the left player (first to join) or right player
        isLeftPlayer = PhotonNetwork.LocalPlayer.ActorNumber == 1;
        boundaryX = isLeftPlayer ? -14f : 14f; // Set boundary based on player side
    }

    void Update()
    {
        if (!photonView.IsMine) return;

        // Get keyboard input
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        // Convert input into movement vector
        Vector3 moveDirection = new Vector3(horizontal, 0f, vertical);

        // Calculate new position
        Vector3 newPosition = transform.position + moveDirection * moveSpeed * Time.deltaTime;

        // Check boundary violation
        if ((isLeftPlayer && newPosition.x < boundaryX) || (!isLeftPlayer && newPosition.x > boundaryX))
        {
            // Player crossed their boundary - trigger loss
            GameManager.Instance.PlayerOutOfBounds(photonView.Owner.ActorNumber);
            return;
        }

        // Apply movement
        transform.position = newPosition;
    }
}
