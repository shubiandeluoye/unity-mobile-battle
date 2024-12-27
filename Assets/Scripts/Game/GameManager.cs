using UnityEngine;
using Photon.Pun;
using System.Collections.Generic;

public class GameManager : MonoBehaviourPunCallbacks
{
    public static GameManager Instance { get; private set; }
    
    private Dictionary<int, int> playerScores = new Dictionary<int, int>();
    private const int SCORE_TO_WIN = 100;
    
    public const int SMALL_BULLET_SCORE = 1;
    public const int MEDIUM_BULLET_SCORE = 5;
    public const int LARGE_BULLET_SCORE = 20;

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

    public void AddScore(int playerActorNumber, int points)
    {
        if (!playerScores.ContainsKey(playerActorNumber))
        {
            playerScores[playerActorNumber] = 0;
        }
        
        playerScores[playerActorNumber] += points;
        
        // Synchronize score across network
        photonView.RPC("SyncScore", RpcTarget.All, playerActorNumber, playerScores[playerActorNumber]);
        
        // Check for victory
        if (playerScores[playerActorNumber] >= SCORE_TO_WIN)
        {
            photonView.RPC("PlayerWon", RpcTarget.All, playerActorNumber);
        }
    }

    [PunRPC]
    private void SyncScore(int playerActorNumber, int newScore)
    {
        playerScores[playerActorNumber] = newScore;
    }

    [PunRPC]
    private void PlayerWon(int playerActorNumber)
    {
        Debug.Log($"Player {playerActorNumber} won the game with {playerScores[playerActorNumber]} points!");
        // TODO: Implement victory UI and game reset
    }

    public void PlayerOutOfBounds(int playerActorNumber)
    {
        photonView.RPC("PlayerLost", RpcTarget.All, playerActorNumber);
    }

    [PunRPC]
    private void PlayerLost(int playerActorNumber)
    {
        Debug.Log($"Player {playerActorNumber} lost by going out of bounds!");
        // TODO: Implement defeat UI and game reset
    }

    public int GetScore(int playerActorNumber)
    {
        return playerScores.ContainsKey(playerActorNumber) ? playerScores[playerActorNumber] : 0;
    }
}
