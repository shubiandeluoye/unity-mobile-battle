using UnityEngine;
using Photon.Pun;

public class ShapeManager : MonoBehaviourPun
{
    [Header("Shape Prefabs")]
    public GameObject trianglePrefab;
    public GameObject trapezoidPrefab;
    public GameObject circlePrefab;
    public GameObject rectanglePrefab;

    [Header("Spawn Settings")]
    public float spawnInterval = 10f;
    public float spawnHeight = 5f;  // Height above the center where shapes spawn
    public float spawnWidth = 2f;   // Width of the spawn area (matching map connection point)
    
    private float timer = 0f;
    private Transform spawnPoint;

    void Start()
    {
        // Initialize spawn point at the top center of the game area
        spawnPoint = new GameObject("ShapeSpawnPoint").transform;
        spawnPoint.position = new Vector3(0f, spawnHeight, 0f);
        spawnPoint.parent = transform;
    }

    void Update()
    {
        if (!PhotonNetwork.IsMasterClient) return;

        timer += Time.deltaTime;
        if (timer >= spawnInterval)
        {
            SpawnRandomShape();
            timer = 0f;
        }
    }

    void SpawnRandomShape()
    {
        // Randomly select which shape to spawn
        int shapeIndex = Random.Range(0, 4);
        GameObject prefabToSpawn = null;

        switch (shapeIndex)
        {
            case 0:
                prefabToSpawn = trianglePrefab;
                break;
            case 1:
                prefabToSpawn = trapezoidPrefab;
                break;
            case 2:
                prefabToSpawn = circlePrefab;
                break;
            case 3:
                prefabToSpawn = rectanglePrefab;
                break;
        }

        if (prefabToSpawn != null)
        {
            // Random position within spawn width
            float randomX = Random.Range(-spawnWidth / 2f, spawnWidth / 2f);
            Vector3 spawnPosition = spawnPoint.position + new Vector3(randomX, 0f, 0f);

            // Instantiate over network
            PhotonNetwork.Instantiate(
                prefabToSpawn.name,
                spawnPosition,
                Quaternion.identity
            );
        }
    }

    // Called by individual shapes when they fall off screen or are destroyed
    public void RemoveShape(GameObject shape)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.Destroy(shape);
        }
    }
}
