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
    [SerializeField] private Button bulletTypeButton; // 子弹类型切换按钮
    [SerializeField] private Button specialAttackButton; // 特殊攻击按钮
    
    [Header("射击设置")]
    private bool is45Degree = true; // true为45度，false为30度
    private BulletType currentBulletType = BulletType.Small; // 当前子弹类型
    private bool isSpecialAttackEnabled = false; // 特殊攻击是否可用
    
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
        if (bulletTypeButton != null)
            bulletTypeButton.onClick.AddListener(ToggleBulletType);
        if (specialAttackButton != null)
        {
            specialAttackButton.onClick.AddListener(TriggerSpecialAttack);
            specialAttackButton.interactable = false; // 初始时禁用特殊攻击按钮
        }

        // 确保启用了增强型触摸支持
        EnhancedTouchSupport.Enable();
    }

    public void SetShootingAngle(bool is45)
    {
        if (!photonView.IsMine) return;
        is45Degree = is45;
    }

    public void SetBulletType(BulletType type)
    {
        if (!photonView.IsMine) return;
        currentBulletType = type;
    }
    
    public void TriggerShoot(ShootDirection direction)
    {
        if (!photonView.IsMine) return;
        Shoot(direction);
    }

    private void Shoot(ShootDirection direction)
    {
        if (!photonView.IsMine) return;
        
        // 调用发射控制器的发射方法，传递当前角度模式
        fireController.FireBullet(currentBulletType, direction, is45Degree);
    }

    private void Update()
    {
        if (photonView.IsMine && specialAttackButton != null)
        {
            // 检查特殊攻击是否可用
            bool canUseSpecialAttack = GameManager.Instance.IsSpecialAttackAvailable(photonView.Owner.ActorNumber);
            if (canUseSpecialAttack != isSpecialAttackEnabled)
            {
                isSpecialAttackEnabled = canUseSpecialAttack;
                specialAttackButton.interactable = isSpecialAttackEnabled;
            }
        }
    }

    private void TriggerSpecialAttack()
    {
        if (!photonView.IsMine || !isSpecialAttackEnabled) return;
        
        // 判断玩家位置
        bool isLeftSidePlayer = transform.position.x < 0;
        
        // 触发特殊攻击效果
        CameraShakeManager.Instance.TriggerSpecialAttack(transform, isLeftSidePlayer);
        
        // 禁用按钮，防止连续使用
        isSpecialAttackEnabled = false;
        specialAttackButton.interactable = false;
    }

    private void ToggleAngle()
    {
        if (!photonView.IsMine) return;
        is45Degree = !is45Degree;
        // 通知发射控制器角度改变
        fireController.ToggleAngle();
    }

    private void ToggleBulletType()
    {
        if (!photonView.IsMine) return;
        currentBulletType = (currentBulletType == BulletType.Small) ? BulletType.Medium : BulletType.Small;
        // 更新按钮显示状态
        if (bulletTypeButton != null)
        {
            var image = bulletTypeButton.GetComponent<Image>();
            if (image != null)
            {
                // 根据子弹类型更新按钮颜色
                image.color = (currentBulletType == BulletType.Small) ? Color.white : Color.yellow;
            }
        }
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
        if (bulletTypeButton != null)
            bulletTypeButton.onClick.RemoveListener(ToggleBulletType);
        if (specialAttackButton != null)
            specialAttackButton.onClick.RemoveListener(TriggerSpecialAttack);
    }
}
