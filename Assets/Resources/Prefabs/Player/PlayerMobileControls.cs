using UnityEngine;

public class PlayerMobileControls : MonoBehaviour
{
    public float speed = 5.0f;
    public float rotationSpeed = 700.0f;

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        // 锁定玩家对象的旋转
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
    }

    void Update()
    {
        Vector3 movement = new Vector3(Input.acceleration.x, 0.0f, Input.acceleration.y);
        rb.velocity = movement * speed;

        // 处理玩家旋转
        float rotation = Input.acceleration.x * rotationSpeed * Time.deltaTime;
        transform.Rotate(0, rotation, 0);
    }
}