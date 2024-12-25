using UnityEngine;

public class MouseTouchSimulator : MonoBehaviour
{
    public float speed = 5.0f;
    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        // 如果在编辑器中运行，并且没有触摸输入
        if (Application.isEditor && Input.touchCount == 0)
        {
            // 模拟触摸开始
            if (Input.GetMouseButtonDown(0))
            {
                SimulateTouch(Input.mousePosition, TouchPhase.Began);
            }
            // 模拟触摸移动
            else if (Input.GetMouseButton(0))
            {
                SimulateTouch(Input.mousePosition, TouchPhase.Moved);
            }
            // 模拟触摸结束
            else if (Input.GetMouseButtonUp(0))
            {
                SimulateTouch(Input.mousePosition, TouchPhase.Ended);
            }
        }

        // 键盘控制逻辑
        float moveHorizontal = Input.GetAxis("Horizontal");
        float moveVertical = Input.GetAxis("Vertical");

        if (moveHorizontal != 0 || moveVertical != 0)
        {
            Vector3 movement = new Vector3(moveHorizontal, 0.0f, moveVertical);
            // 处理键盘输入的移动逻辑
            HandleMovement(movement);
        }
    }

    void SimulateTouch(Vector3 position, TouchPhase phase)
    {
        Touch touch = new Touch
        {
            fingerId = 0,
            position = position,
            phase = phase,
            type = TouchType.Direct
        };

        // 使用反射将模拟的触摸输入传递给 Unity 的 Input 类
        typeof(Input).GetMethod("SimulateTouch", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)
            .Invoke(null, new object[] { touch });
    }

    void HandleMovement(Vector3 movement)
    {
        // 在这里处理移动逻辑，例如更新玩家位置
        Debug.Log("Handling movement: " + movement);
        rb.velocity = movement * speed;
    }
}