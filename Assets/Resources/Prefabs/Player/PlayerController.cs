using UnityEngine;
using Photon.Pun;

public class PlayerController : MonoBehaviour
{
    public float speed = 5.0f;
    public float rotationSpeed = 700.0f;

    private Rigidbody rb;
    private PhotonView photonView;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        photonView = GetComponent<PhotonView>();
        // 锁定玩家对象的旋转
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
    }

    // Public methods for EventSystem to call
    public void OnMove(Vector2 movement)
    {
        if (photonView.IsMine)
        {
            Vector3 moveDirection = new Vector3(movement.x, 0.0f, movement.y);
            rb.velocity = moveDirection * speed;
        }
    }

    public void OnRotate(float rotationAmount)
    {
        if (photonView.IsMine)
        {
            transform.Rotate(0, rotationAmount * rotationSpeed * Time.deltaTime, 0);
        }
    }

    void Update()
    {
        // Movement and rotation now handled by EventSystem through OnMove and OnRotate methods
        if (photonView.IsMine)
        {
            // Keep empty Update method for potential future use
        }
    }
}
