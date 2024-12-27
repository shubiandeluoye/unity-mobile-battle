using UnityEngine;

public class BulletBehavior : MonoBehaviourPunCallbacks
{
    private PhotonView photonView;
    public enum BulletType
    {
        Small,
        Medium,
        Large
    }

    public BulletType type;
    private Rigidbody rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        photonView = GetComponent<PhotonView>();
        
        // Set up bullet properties based on type
        switch (type)
        {
            case BulletType.Small:
            case BulletType.Medium:
                // Small and medium bullets can bounce
                rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
                break;
            case BulletType.Large:
                // Large bullets ignore wall collisions
                Physics.IgnoreLayerCollision(gameObject.layer, 8); // Ignore bounce walls
                Physics.IgnoreLayerCollision(gameObject.layer, 9); // Ignore middle walls
                break;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            // Handle player hit effects
            switch (type)
            {
                case BulletType.Small:
                    // Small bullet just deals damage
                    break;
                case BulletType.Medium:
                    // Medium bullet stuns for 1 second
                    StartCoroutine(StunPlayer(collision.gameObject));
                    break;
                case BulletType.Large:
                    // Large bullet pushes player back
                    PushPlayer(collision.gameObject);
                    break;
            }
            
            // Award points to the shooter
            if (photonView.IsMine)
            {
                int points = type switch
                {
                    BulletType.Small => GameManager.SMALL_BULLET_SCORE,
                    BulletType.Medium => GameManager.MEDIUM_BULLET_SCORE,
                    BulletType.Large => GameManager.LARGE_BULLET_SCORE,
                    _ => 0
                };
                
                GameManager.Instance.AddScore(photonView.Owner.ActorNumber, points);
                PhotonNetwork.Destroy(gameObject);
            }
        }
    }

    private System.Collections.IEnumerator StunPlayer(GameObject player)
    {
        // Implement stun logic here (e.g., disable player movement)
        PlayerController controller = player.GetComponent<PlayerController>();
        if (controller != null)
        {
            controller.enabled = false;
            yield return new WaitForSeconds(1f);
            controller.enabled = true;
        }
    }

    private void PushPlayer(GameObject player)
    {
        Rigidbody playerRb = player.GetComponent<Rigidbody>();
        if (playerRb != null)
        {
            // Push player back by half the map distance
            float pushForce = 10f; // Adjust based on map size
            Vector3 pushDirection = (player.transform.position - transform.position).normalized;
            playerRb.AddForce(pushDirection * pushForce, ForceMode.Impulse);
        }
    }

    private void OnBecameInvisible()
    {
        // Destroy bullet when it leaves the screen
        Destroy(gameObject);
    }
}
