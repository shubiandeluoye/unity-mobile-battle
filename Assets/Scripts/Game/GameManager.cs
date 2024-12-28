using UnityEngine;
using Photon.Pun;
using System.Collections.Generic;

public class GameManager : MonoBehaviourPunCallbacks
{
    public static GameManager Instance { get; private set; }
    
    private Dictionary<int, int> playerScores = new Dictionary<int, int>();
    private Dictionary<int, int> playerPointsLost = new Dictionary<int, int>();
    private const int SCORE_TO_WIN = 100;
    private const int POINTS_LOST_FOR_SPECIAL = 45;
    
    public const int SMALL_BULLET_SCORE = 1;
    public const int MEDIUM_BULLET_SCORE = 5;
    public const int LARGE_BULLET_SCORE = 20;
    
    public bool IsSpecialAttackAvailable(int playerActorNumber)
    {
        return playerPointsLost.ContainsKey(playerActorNumber) && 
               playerPointsLost[playerActorNumber] >= POINTS_LOST_FOR_SPECIAL;
    }

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
        ResetRoundScores();
    }

    public void ResetRoundScores()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            photonView.RPC("RPC_ResetScores", RpcTarget.All);
        }
    }

    [PunRPC]
    private void RPC_ResetScores()
    {
        playerScores.Clear();
        playerPointsLost.Clear();
        foreach (var player in PhotonNetwork.PlayerList)
        {
            playerScores[player.ActorNumber] = 0;
            playerPointsLost[player.ActorNumber] = 0;
        }
    }

    public void AddScore(int playerActorNumber, int points)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            if (!playerScores.ContainsKey(playerActorNumber))
            {
                playerScores[playerActorNumber] = 0;
                playerPointsLost[playerActorNumber] = 0;
            }
            
            playerScores[playerActorNumber] += points;
            
            // If points are negative (damage taken), track it for special attack
            if (points < 0)
            {
                playerPointsLost[playerActorNumber] += -points;
                photonView.RPC("RPC_SyncPointsLost", RpcTarget.All, playerActorNumber, playerPointsLost[playerActorNumber]);
            }
            
            // Synchronize score across network
            photonView.RPC("RPC_SyncScore", RpcTarget.All, playerActorNumber, playerScores[playerActorNumber]);
            
            // Check for round victory
            if (playerScores[playerActorNumber] >= SCORE_TO_WIN)
            {
                // Notify MatchManager of round win
                MatchManager.Instance.HandleRoundWin(playerActorNumber);
                ResetRoundScores();
            }
        }
    }


    [PunRPC]
    private void RPC_SyncScore(int playerActorNumber, int newScore)
    {
        playerScores[playerActorNumber] = newScore;
    }
    
    [PunRPC]
    private void RPC_SyncPointsLost(int playerActorNumber, int pointsLost)
    {
        playerPointsLost[playerActorNumber] = pointsLost;
    }

    public void PlayerOutOfBounds(int playerActorNumber)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            photonView.RPC("RPC_PlayerOutOfBounds", RpcTarget.All, playerActorNumber);
        }
    }

    [PunRPC]
    private void RPC_PlayerOutOfBounds(int playerActorNumber)
    {
        // Immediate match loss on boundary failure
        MatchManager.Instance.HandleBoundaryFailure(playerActorNumber);
        ResetRoundScores();
        
        // Notify all players about the boundary failure
        if (PhotonNetwork.IsMasterClient)
        {
            // Calculate the opponent's actor number
            var players = PhotonNetwork.PlayerList;
            int opponentActorNumber = -1;
            foreach (var player in players)
            {
                if (player.ActorNumber != playerActorNumber)
                {
                    opponentActorNumber = player.ActorNumber;
                    break;
                }
            }
            
            if (opponentActorNumber != -1)
            {
                // Award the round to the opponent
                MatchManager.Instance.HandleRoundWin(opponentActorNumber);
            }
        }
    }

    public int GetScore(int playerActorNumber)
    {
        return playerScores.ContainsKey(playerActorNumber) ? playerScores[playerActorNumber] : 0;
    }
}
