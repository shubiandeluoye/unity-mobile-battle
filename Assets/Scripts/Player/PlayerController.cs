using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;

    void Update()
    {
        // Get keyboard input
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        // Convert input into movement vector
        Vector3 moveDirection = new Vector3(horizontal, 0f, vertical);

        // Apply movement
        transform.Translate(moveDirection * moveSpeed * Time.deltaTime, Space.World);
    }
}
