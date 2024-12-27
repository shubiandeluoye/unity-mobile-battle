using UnityEngine;
using Photon.Pun;

public class CameraShakeManager : MonoBehaviourPunCallbacks
{
    public static CameraShakeManager Instance { get; private set; }
    
    private Camera mainCamera;
    private Vector3 originalPosition;
    private bool isShaking = false;
    
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
        }
        
        mainCamera = Camera.main;
        if (mainCamera != null)
        {
            originalPosition = mainCamera.transform.position;
        }
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

    private System.Collections.IEnumerator ShakeCoroutine(float duration, float magnitude)
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
}
