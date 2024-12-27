using UnityEngine;

public class WallBehavior : MonoBehaviour
{
    private void Start()
    {
        // Get the collider component
        Collider wallCollider = GetComponent<Collider>();
        
        // If this is a bounce wall (layer 8)
        if (gameObject.layer == 8)
        {
            // Set the bounce physics material
            if (wallCollider != null)
            {
                PhysicMaterial bounceMaterial = Resources.Load<PhysicMaterial>("Materials/BounceWall");
                wallCollider.material = bounceMaterial;
            }
        }
        // If this is a middle wall (layer 9)
        else if (gameObject.layer == 9)
        {
            // Set trigger to allow bullets to pass through
            if (wallCollider != null)
            {
                wallCollider.isTrigger = true;
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Handle bullet reflections for top/bottom walls
        if (gameObject.layer == 8 && collision.gameObject.CompareTag("Bullet"))
        {
            Rigidbody bulletRb = collision.gameObject.GetComponent<Rigidbody>();
            if (bulletRb != null)
            {
                // Calculate reflection angle based on incoming velocity and wall normal
                Vector3 incomingVelocity = bulletRb.velocity;
                Vector3 reflectionDirection = Vector3.Reflect(incomingVelocity.normalized, collision.contacts[0].normal);
                
                // Maintain the same speed but change direction
                bulletRb.velocity = reflectionDirection * incomingVelocity.magnitude;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Middle wall (layer 9) logic
        if (gameObject.layer == 9)
        {
            // Block players but allow bullets through
            if (other.CompareTag("Player"))
            {
                // Get the player's rigidbody
                Rigidbody playerRb = other.GetComponent<Rigidbody>();
                if (playerRb != null)
                {
                    // Stop player movement in x direction
                    Vector3 velocity = playerRb.velocity;
                    velocity.x = 0;
                    playerRb.velocity = velocity;
                    
                    // Push player back slightly
                    Vector3 pushDirection = -transform.right * 0.5f;
                    playerRb.AddForce(pushDirection, ForceMode.Impulse);
                }
            }
        }
    }
}
