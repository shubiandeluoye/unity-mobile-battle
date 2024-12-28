using UnityEngine;

public class RectangleShape : BaseShape
{
    [Header("Rectangle Settings")]
    public float maxHealth = 100f;           // Total health before destruction
    public float damagePerHit = 10f;        // Damage taken per bullet hit
    public SpriteRenderer spriteRenderer;    // Reference to sprite renderer

    private float currentHealth;

    protected override void Start()
    {
        base.Start();
        currentHealth = maxHealth;
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public override void OnBulletHit(GameObject bullet)
    {
        if (!photonView.IsMine) return;

        // Reduce health and sync across network
        currentHealth -= damagePerHit;
        photonView.RPC("SyncDamage", RpcTarget.All, currentHealth);

        if (currentHealth <= 0)
        {
            var shapeManager = FindObjectOfType<ShapeManager>();
            if (shapeManager != null)
            {
                shapeManager.RemoveShape(gameObject);
            }
        }
    }

    [PunRPC]
    private void SyncDamage(float health)
    {
        currentHealth = health;
        
        // Update visual representation
        if (spriteRenderer != null)
        {
            // Fade based on remaining health
            Color color = spriteRenderer.color;
            color.a = currentHealth / maxHealth;
            spriteRenderer.color = color;
        }
    }
}
