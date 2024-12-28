using UnityEngine;
using Photon.Pun;
using System.Collections;
using Cinemachine;

public class CameraShakeManager : MonoBehaviourPunCallbacks
{
    public static CameraShakeManager Instance { get; private set; }
    
    [Header("Camera References")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private CinemachineVirtualCamera battleCamera;
    [SerializeField] private CinemachineVirtualCamera zoomCamera;
    
    [Header("Special Attack Settings")]
    [SerializeField] private GameObject bookPrefab;  // Assign BookEffect.prefab in inspector
    [SerializeField] private float bookFlyDuration = 1.5f;
    [SerializeField] private float cameraRotationDuration = 1.0f;
    [SerializeField] private float shakeIntensity = 5f;
    [SerializeField] private float shakeDuration = 0.5f;
    [SerializeField] private float bookSize = 1f;  // Size of the book effect (1x1 units)
    
    private Vector3 originalPosition;
    private bool isShaking = false;
    private bool isSpecialAttackInProgress = false;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        
        if (mainCamera == null)
            mainCamera = Camera.main;
        if (mainCamera != null)
            originalPosition = mainCamera.transform.position;
            
        // Initialize virtual cameras if not set
        if (battleCamera == null)
            Debug.LogWarning("Battle camera not assigned in CameraShakeManager");
        if (zoomCamera == null)
            Debug.LogWarning("Zoom camera not assigned in CameraShakeManager");
    }

    // Interface for triggering screen shake
    public void TriggerShake(float duration, float magnitude)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            photonView.RPC("RPC_TriggerShake", RpcTarget.All, duration, magnitude);
        }
    }

    [PunRPC]
    private void RPC_TriggerShake(float duration, float magnitude)
    {
        if (mainCamera != null && !isShaking)
        {
            StartCoroutine(ShakeCoroutine(duration, magnitude));
        }
    }

    private IEnumerator ShakeCoroutine(float duration, float magnitude)
    {
        isShaking = true;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;

            mainCamera.transform.position = originalPosition + new Vector3(x, y, 0f);

            elapsed += Time.deltaTime;
            yield return null;
        }

        mainCamera.transform.position = originalPosition;
        isShaking = false;
    }
    
    public void TriggerSpecialAttack(Transform player, bool isLeftSidePlayer)
    {
        if (!photonView.IsMine || isSpecialAttackInProgress) return;
        photonView.RPC("RPC_TriggerSpecialAttack", RpcTarget.All, player.position, isLeftSidePlayer);
    }
    
    [PunRPC]
    private void RPC_TriggerSpecialAttack(Vector3 playerPosition, bool isLeftSidePlayer)
    {
        if (isSpecialAttackInProgress) return;
        StartCoroutine(SpecialAttackSequence(playerPosition, isLeftSidePlayer));
    }
    
    private IEnumerator SpecialAttackSequence(Vector3 playerPosition, bool isLeftSidePlayer)
    {
        isSpecialAttackInProgress = true;
        
        // Switch to zoom camera
        if (zoomCamera != null && battleCamera != null)
        {
            // Position zoom camera at player's feet
            zoomCamera.transform.position = new Vector3(playerPosition.x, playerPosition.y - 1f, -10f);
            
            // Activate zoom camera
            battleCamera.Priority = 0;
            zoomCamera.Priority = 10;
            
            // Wait for zoom
            yield return new WaitForSeconds(0.5f);
            
            // Rotate camera around player
            float elapsedTime = 0f;
            Vector3 startPos = zoomCamera.transform.position;
            
            while (elapsedTime < cameraRotationDuration)
            {
                elapsedTime += Time.deltaTime;
                float angle = Mathf.Lerp(0f, 90f, elapsedTime / cameraRotationDuration);
                Vector3 offset = Quaternion.Euler(0, 0, angle) * Vector3.down;
                zoomCamera.transform.position = playerPosition + offset;
                yield return null;
            }
            
            // Return to battle camera
            battleCamera.Priority = 10;
            zoomCamera.Priority = 0;
        }
        
        // Spawn and animate book
        if (bookPrefab != null)
        {
            GameObject book = Instantiate(bookPrefab);
            book.transform.localScale = new Vector3(bookSize, bookSize, 1f);
            
            // Calculate start and end positions based on screen bounds
            Vector3 screenBounds = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, 0));
            float startX = isLeftSidePlayer ? -screenBounds.x - bookSize : screenBounds.x + bookSize;
            Vector3 startPos = new Vector3(startX, 0f, 0f);
            Vector3 endPos = Vector3.zero; // Center of screen
            
            book.transform.position = startPos;
            
            float elapsedTime = 0f;
            while (elapsedTime < bookFlyDuration)
            {
                elapsedTime += Time.deltaTime;
                float t = elapsedTime / bookFlyDuration;
                book.transform.position = Vector3.Lerp(startPos, endPos, t);
                yield return null;
            }
            
            // Trigger screen shake
            TriggerShake(shakeDuration, shakeIntensity);
            
            yield return new WaitForSeconds(0.5f);
            Destroy(book);
        }
        
        isSpecialAttackInProgress = false;
    }
}
