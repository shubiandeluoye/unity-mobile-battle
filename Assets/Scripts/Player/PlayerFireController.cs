using UnityEngine;
using Photon.Pun;

public class PlayerFireController : MonoBehaviourPunCallbacks
{
    [Header("Bullet Prefabs")]
    public GameObject smallBulletPrefab;
    public GameObject mediumBulletPrefab;
    public GameObject largeBulletPrefab;

    [Header("Firing Rates")]
    public float smallBulletFireRate = 2.0f;
    public float mediumBulletFireRate = 1.0f;
    private float nextFireTime = 0f;

    [Header("Angle Settings")]
    private bool is45Degree = true; // true for 45°, false for 30°
    private Transform bulletSpawnPoint;
    private Transform opponentTransform;
    private bool isLeftSidePlayer;

    private void Start()
    {
        bulletSpawnPoint = transform;
        DeterminePlayerPosition();
        // We'll need to find the opponent's transform when the match starts
        // This will be set up through the GameManager
    }

    private void DeterminePlayerPosition()
    {
        if (!photonView.IsMine) return;
        
        // 获取玩家的屏幕位置
        Vector3 screenPos = Camera.main.WorldToScreenPoint(transform.position);
        // 如果玩家在屏幕左半边，则为左侧玩家
        isLeftSidePlayer = screenPos.x < Screen.width / 2;
    }

    public void SetOpponentTransform(Transform opponent)
    {
        opponentTransform = opponent;
    }

    public void ToggleAngle()
    {
        is45Degree = !is45Degree;
        // We might want to notify UI or other components about the angle change
    }

    public void FireBullet(BulletType type, ShootDirection direction, bool is45Degree)
    {
        if (!photonView.IsMine || Time.time < nextFireTime)
            return;

        GameObject bulletPrefab = GetBulletPrefab(type);
        if (bulletPrefab == null)
            return;

        float angle = CalculateShootingAngle(direction, is45Degree);
        Vector3 shootDirection = CalculateShootDirection(angle);

        // Use PhotonNetwork.Instantiate for network synchronization
        GameObject bullet = PhotonNetwork.Instantiate(
            bulletPrefab.name,
            bulletSpawnPoint.position,
            Quaternion.LookRotation(shootDirection)
        );

        // Update fire rate cooldown
        nextFireTime = Time.time + (type == BulletType.Small ? 1f/smallBulletFireRate : 1f/mediumBulletFireRate);
    }

    private GameObject GetBulletPrefab(BulletType type)
    {
        switch (type)
        {
            case BulletType.Small:
                return smallBulletPrefab;
            case BulletType.Medium:
                return mediumBulletPrefab;
            case BulletType.Large:
                return largeBulletPrefab;
            default:
                return null;
        }
    }

    private float CalculateShootingAngle(ShootDirection direction, bool is45Degree)
    {
        float angleModifier = is45Degree ? 45f : 30f;
        
        switch (direction)
        {
            case ShootDirection.Up:
                return angleModifier;
            case ShootDirection.Down:
                return -angleModifier;
            case ShootDirection.Straight:
                return 0f; // Will be adjusted to aim at opponent
            default:
                return 0f;
        }
    }

    private Vector3 CalculateShootDirection(float angle)
    {
        Vector3 baseDirection;
        
        if (opponentTransform == null)
        {
            // 如果找不到对手，根据玩家位置发射向对面
            baseDirection = isLeftSidePlayer ? Vector3.right : Vector3.left;
        }
        else
        {
            // 根据对手位置计算基础方向
            baseDirection = (opponentTransform.position - bulletSpawnPoint.position).normalized;
        }

        if (angle == 0f) // Straight shot
        {
            return baseDirection;
        }
        else
        {
            // 根据玩家位置调整角度方向
            float adjustedAngle = isLeftSidePlayer ? angle : -angle;
            return Quaternion.Euler(0, 0, adjustedAngle) * baseDirection;
        }
    }
}

public enum BulletType
{
    Small,
    Medium,
    Large
}

public enum ShootDirection
{
    Up,
    Down,
    Straight
}
