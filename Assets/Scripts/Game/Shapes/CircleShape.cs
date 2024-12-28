using UnityEngine;
using System.Collections.Generic;

public class CircleShape : BaseShape
{
    [Header("Circle Settings")]
    public int activationThreshold = 20;     // Total hits needed to activate
    public int bulletVolleyCount = 20;       // Number of bullets to fire
    public float minRandomAngle = -60f;      // Minimum random angle
    public float maxRandomAngle = 60f;       // Maximum random angle
    public GameObject bulletPrefab;          // Reference to bullet prefab


    private int leftPlayerHits = 0;
    private int rightPlayerHits = 0;

    public override void OnBulletHit(GameObject bullet)
    {
        if (!photonView.IsMine) return;

        // Determine which side the bullet came from
        bool isFromLeft = bullet.transform.position.x < transform.position.x;
        
        // Update hit counts
        if (isFromLeft)
            leftPlayerHits++;
        else
            rightPlayerHits++;

        // Sync counts across network
        photonView.RPC("SyncHitCounts", RpcTarget.All, leftPlayerHits, rightPlayerHits);

        // Check if threshold reached
        if (leftPlayerHits + rightPlayerHits >= activationThreshold)
        {
            // Fire volley toward player with fewer hits
            bool fireTowardLeft = leftPlayerHits < rightPlayerHits;
            ReleaseVolley(fireTowardLeft);
        }
    }

    private void ReleaseVolley(bool towardLeft)
    {
        if (!PhotonNetwork.IsMasterClient) return;

        float baseAngle = towardLeft ? 180f : 0f;
        
        for (int i = 0; i < bulletVolleyCount; i++)
        {
            float randomAngle = Random.Range(minRandomAngle, maxRandomAngle);
            Quaternion rotation = Quaternion.Euler(0f, 0f, baseAngle + randomAngle);
            
            PhotonNetwork.Instantiate(
                bulletPrefab.name,
                transform.position,
                rotation
            );
        }

        // Destroy self after volley
        var shapeManager = FindObjectOfType<ShapeManager>();
        if (shapeManager != null)
        {
            shapeManager.RemoveShape(gameObject);
        }
    }

    [PunRPC]
    private void SyncHitCounts(int leftHits, int rightHits)
    {
        leftPlayerHits = leftHits;
        rightPlayerHits = rightHits;
    }
}
