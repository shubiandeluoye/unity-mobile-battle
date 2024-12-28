using UnityEngine;
using Photon.Pun;
using System.Collections;

public class BulletBehavior : MonoBehaviourPunCallbacks
{
    public enum BulletType
    {
        Small,
        Medium,
        Large
    }

    public BulletType type;
    private Rigidbody rb;

    // Animation and effect parameters
    private const float LARGE_BULLET_ANIMATION_DURATION = 1.5f;
    private const float SCREEN_SHAKE_DURATION = 0.5f;
    private const float SCREEN_SHAKE_MAGNITUDE = 0.3f;
    private const float PUSH_FORCE = 10f;

    // Interface for special animation components
    private Animator specialEffectAnimator;
    private ParticleSystem impactParticleSystem;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        specialEffectAnimator = GetComponent<Animator>();
        impactParticleSystem = GetComponent<ParticleSystem>();
        
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
        HandleCollision(collision.gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        HandleCollision(other.gameObject);
    }

    private void HandleCollision(GameObject hitObject)
    {
        // Check for shape collision first
        BaseShape shape = hitObject.GetComponent<BaseShape>();
        if (shape != null)
        {
            shape.OnBulletHit(gameObject);
            if (photonView.IsMine)
            {
                PhotonNetwork.Destroy(gameObject);
            }
            return;
        }

        // Handle player collision
        if (hitObject.CompareTag("Player"))
        {
            // Handle player hit effects
            switch (type)
            {
                case BulletType.Small:
                    // Small bullet just deals damage
                    break;
                case BulletType.Medium:
                    // Medium bullet stuns for 1 second
                    StartCoroutine(StunPlayer(hitObject));
                    break;
                case BulletType.Large:
                    // Large bullet pushes player back and triggers special effects
                    StartCoroutine(LargeBulletEffects(hitObject));
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
        PlayerEventMovement movement = player.GetComponent<PlayerEventMovement>();
        if (movement != null)
        {
            // Store original speed and temporarily disable movement
            float originalSpeed = movement.speed;
            movement.speed = 0f;
            yield return new WaitForSeconds(1f);
            movement.speed = originalSpeed;
        }
    }

    private System.Collections.IEnumerator LargeBulletEffects(GameObject player)
    {
        // Trigger screen shake
        if (CameraShakeManager.Instance != null)
        {
            CameraShakeManager.Instance.TriggerShake(SCREEN_SHAKE_DURATION, SCREEN_SHAKE_MAGNITUDE);
        }

        // Play special animation if available
        if (specialEffectAnimator != null)
        {
            photonView.RPC("RPC_PlaySpecialAnimation", RpcTarget.All);
        }

        // Play impact particles if available
        if (impactParticleSystem != null)
        {
            photonView.RPC("RPC_PlayImpactParticles", RpcTarget.All);
        }

        // Push player back
        PushPlayer(player);

        yield return new WaitForSeconds(LARGE_BULLET_ANIMATION_DURATION);
    }

    [PunRPC]
    private void RPC_PlaySpecialAnimation()
    {
        if (specialEffectAnimator != null)
        {
            specialEffectAnimator.SetTrigger("PlayLargeBulletEffect");
        }
    }

    [PunRPC]
    private void RPC_PlayImpactParticles()
    {
        if (impactParticleSystem != null)
        {
            impactParticleSystem.Play();
        }
    }

    private void PushPlayer(GameObject player)
    {
        Rigidbody playerRb = player.GetComponent<Rigidbody>();
        if (playerRb != null)
        {
            // Push player back by half the map distance
            Vector3 pushDirection = (player.transform.position - transform.position).normalized;
            playerRb.AddForce(pushDirection * PUSH_FORCE, ForceMode.Impulse);
        }
    }

    private void OnBecameInvisible()
    {
        // Destroy bullet when it leaves the screen
        if (photonView.IsMine)
        {
            PhotonNetwork.Destroy(gameObject);
        }
    }
}
