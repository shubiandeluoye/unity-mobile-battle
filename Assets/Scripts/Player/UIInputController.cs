using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.OnScreen;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Photon.Pun;

[AddComponentMenu("Input/On-Screen Controls")]
public class UIInputController : MonoBehaviourPunCallbacks
{
    [Header("移动控制")]
    [SerializeField] private OnScreenStick movementStick;
    
    [Header("射击按钮")]
    [SerializeField] private Button upShootButton;
    [SerializeField] private Button downShootButton;
    [SerializeField] private Button straightShootButton;
    [SerializeField] private Button angleToggleButton;
    
    [Header("射击设置")]
    private bool is45Degree = true; // true为45度，false为30度
    
    private PlayerFireController fireController;
    private void Start()
    {
        // 获取玩家的发射控制器组件
        fireController = GetComponent<PlayerFireController>();
        if (fireController == null)
        {
            Debug.LogError("未找到PlayerFireController组件");
            return;
        }

        // 设置按钮监听
        if (angleToggleButton != null)
            angleToggleButton.onClick.AddListener(ToggleAngle);
        if (upShootButton != null)
            upShootButton.onClick.AddListener(() => Shoot(ShootDirection.Up));
        if (downShootButton != null)
            downShootButton.onClick.AddListener(() => Shoot(ShootDirection.Down));
        if (straightShootButton != null)
            straightShootButton.onClick.AddListener(() => Shoot(ShootDirection.Straight));

        // 确保启用了增强型触摸支持
        EnhancedTouchSupport.Enable();
    }

    private void ToggleAngle()
    {
        if (!photonView.IsMine) return;
        
        is45Degree = !is45Degree;
        // 可以在这里添加UI反馈，比如改变按钮颜色等
    }

    private void Shoot(ShootDirection direction)
    {
        if (!photonView.IsMine) return;

        // 获取当前选择的子弹类型（将在后续实现）
        BulletType currentBulletType = BulletType.Small; // 临时默认值
        
        // 调用发射控制器的发射方法，传递当前角度模式
        fireController.FireBullet(currentBulletType, direction, is45Degree);
    }

    private void OnDestroy()
    {
        // 移除按钮监听
        if (angleToggleButton != null)
            angleToggleButton.onClick.RemoveListener(ToggleAngle);
        if (upShootButton != null)
            upShootButton.onClick.RemoveListener(() => Shoot(ShootDirection.Up));
        if (downShootButton != null)
            downShootButton.onClick.RemoveListener(() => Shoot(ShootDirection.Down));
        if (straightShootButton != null)
            straightShootButton.onClick.RemoveListener(() => Shoot(ShootDirection.Straight));
    }
}
