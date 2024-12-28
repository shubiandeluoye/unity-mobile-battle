using UnityEngine;
using Photon.Pun;

public abstract class BaseShape : MonoBehaviourPun
{
    protected Rigidbody2D rb;
    protected bool isInitialized = false;

    [Header("Base Settings")]
    public float fallSpeed = 2f;
    public float destroyBelowY = -6f;  // Y position below which shape is destroyed

    protected virtual void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.gravityScale = 0f;  // We'll control falling manually
            rb.drag = 0.5f;        // Add some drag for smoother movement
            isInitialized = true;
        }
    }

    protected virtual void Update()
    {
        if (!isInitialized) return;

        // Apply constant downward movement
        transform.Translate(Vector3.down * fallSpeed * Time.deltaTime);

        // Check if fallen below destroy threshold
        if (transform.position.y < destroyBelowY)
        {
            var shapeManager = FindObjectOfType<ShapeManager>();
            if (shapeManager != null)
            {
                shapeManager.RemoveShape(gameObject);
            }
        }
    }

    // Called when a bullet hits this shape
    public abstract void OnBulletHit(GameObject bullet);
}
