using UnityEngine;
using System.Collections.Generic;

public class TrapezoidShape : BaseShape
{
    [Header("Trapezoid Settings")]
    public float heightThreshold = 0.5f;    // Determines top vs bottom hit
    public float bulletSpawnOffset = 0.5f;  // Distance between spawned bullets
    public GameObject bulletPrefab;         // Reference to bullet prefab

    public override void OnBulletHit(GameObject bullet)
    {
        if (!photonView.IsMine) return;

        // Determine if hit was on top or bottom portion
        float localY = transform.InverseTransformPoint(bullet.transform.position).y;
        
        if (localY > heightThreshold)
        {
            // Top hit: Spawn 3 bullets at angles
            SpawnBullets(new float[] { -30f, 0f, 30f });
        }
        else
        {
            // Bottom hit: Spawn 3 straight bullets
            SpawnStraightBullets();
        }
    }

    private void SpawnBullets(float[] angles)
    {
        if (!PhotonNetwork.IsMasterClient) return;

        foreach (float angle in angles)
        {
            Vector3 spawnPos = transform.position;
            Quaternion rotation = Quaternion.Euler(0f, 0f, angle);
            
            PhotonNetwork.Instantiate(
                bulletPrefab.name,
                spawnPos,
                rotation
            );
        }
    }

    private void SpawnStraightBullets()
    {
        if (!PhotonNetwork.IsMasterClient) return;

        for (int i = -1; i <= 1; i++)
        {
            Vector3 spawnPos = transform.position + new Vector3(i * bulletSpawnOffset, 0f, 0f);
            
            PhotonNetwork.Instantiate(
                bulletPrefab.name,
                spawnPos,
                Quaternion.identity
            );
        }
    }
}
