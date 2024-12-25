using Photon.Pun;
using UnityEngine;

public class PlayerNetworkSync : MonoBehaviourPunCallbacks, IPunObservable
{
    private Vector3 networkedPosition;
    private Quaternion networkedRotation;

    void Start()
    {
        if (!photonView.IsMine)
        {
            networkedPosition = transform.position;
            networkedRotation = transform.rotation;
        }
    }

    void Update()
    {
        if (!photonView.IsMine)
        {
            transform.position = Vector3.Lerp(transform.position, networkedPosition, Time.deltaTime * 10);
            transform.rotation = Quaternion.Lerp(transform.rotation, networkedRotation, Time.deltaTime * 10);
        }
        else
        {
            networkedPosition = transform.position;
            networkedRotation = transform.rotation;
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(transform.position);
            stream.SendNext(transform.rotation);
        }
        else
        {
            networkedPosition = (Vector3)stream.ReceiveNext();
            networkedRotation = (Quaternion)stream.ReceiveNext();
        }
    }
}