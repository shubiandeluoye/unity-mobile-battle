using UnityEngine;

public class TriangleShape : BaseShape
{
    [Header("Triangle Settings")]
    public float rotationAmount = 30f;  // Degrees to rotate on hit

    public override void OnBulletHit(GameObject bullet)
    {
        if (!photonView.IsMine) return;

        // Rotate the shape
        transform.Rotate(0f, 0f, rotationAmount);

        // Sync rotation across network using RPC
        photonView.RPC("SyncRotation", RpcTarget.All, transform.rotation.eulerAngles.z);
    }

    [PunRPC]
    private void SyncRotation(float zRotation)
    {
        transform.rotation = Quaternion.Euler(0f, 0f, zRotation);
    }
}
