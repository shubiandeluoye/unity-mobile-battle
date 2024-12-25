using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class NetworkManager : MonoBehaviourPunCallbacks
{
    public static NetworkManager Instance { get; private set; }
    private const string PHOTON_APP_ID = "19925bab-94bb-4b4d-9e64-ae8411854f9c";

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
    }

    private void Start()
    {
        ConnectToPhoton();
    }

    private void ConnectToPhoton()
    {
        PhotonNetwork.GameVersion = "1.0";
        PhotonNetwork.ConnectUsingSettings();
        Debug.Log("正在连接到Photon服务器...");
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("已连接到Photon服务器！");
        PhotonNetwork.JoinLobby();
    }

    public override void OnJoinedLobby()
    {
        Debug.Log("已加入大厅！");
        PhotonNetwork.JoinOrCreateRoom("Room1", new RoomOptions { MaxPlayers = 4 }, TypedLobby.Default);
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("已加入房间！");
        PhotonNetwork.Instantiate("Resources/PlayerPrefab", Vector3.zero, Quaternion.identity);
    }
}