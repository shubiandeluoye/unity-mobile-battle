using UnityEngine;
using Photon.Pun;
using System.Collections.Generic;

public class MatchManager : MonoBehaviourPunCallbacks
{
    public static MatchManager Instance { get; private set; }

    [System.Serializable]
    public class MatchConfig
    {
        public int roundsToWin = 2;  // Default for best-of-three
        public int maxRounds = 3;    // Can be extended to 5 for best-of-five
    }

    public MatchConfig currentConfig = new MatchConfig();
    
    private Dictionary<int, int> playerWins = new Dictionary<int, int>();  // Player ActorNumber -> Wins
    private int currentRound = 1;
    private bool isMatchInProgress = false;

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

    public void StartMatch()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            photonView.RPC("RPC_StartMatch", RpcTarget.All);
        }
    }

    [PunRPC]
    private void RPC_StartMatch()
    {
        isMatchInProgress = true;
        currentRound = 1;
        playerWins.Clear();
        
        // Initialize win counts for all players
        foreach (var player in PhotonNetwork.PlayerList)
        {
            playerWins[player.ActorNumber] = 0;
        }
    }

    public void HandleRoundWin(int winnerActorNumber)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            photonView.RPC("RPC_HandleRoundWin", RpcTarget.All, winnerActorNumber);
        }
    }

    [PunRPC]
    private void RPC_HandleRoundWin(int winnerActorNumber)
    {
        if (!playerWins.ContainsKey(winnerActorNumber))
        {
            playerWins[winnerActorNumber] = 0;
        }

        playerWins[winnerActorNumber]++;
        currentRound++;

        // Check if match is won
        if (playerWins[winnerActorNumber] >= currentConfig.roundsToWin)
        {
            EndMatch(winnerActorNumber);
        }
        else if (currentRound > currentConfig.maxRounds)
        {
            // Handle draw or tiebreaker
            int maxWins = 0;
            int winningPlayer = -1;
            foreach (var kvp in playerWins)
            {
                if (kvp.Value > maxWins)
                {
                    maxWins = kvp.Value;
                    winningPlayer = kvp.Key;
                }
            }
            EndMatch(winningPlayer);
        }
    }

    public void HandleBoundaryFailure(int failedPlayerActorNumber)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            // When a player crosses boundary, the other player wins the round
            var winner = GetOpponentActorNumber(failedPlayerActorNumber);
            if (winner != -1)
            {
                HandleRoundWin(winner);
            }
        }
    }

    private int GetOpponentActorNumber(int playerActorNumber)
    {
        foreach (var player in PhotonNetwork.PlayerList)
        {
            if (player.ActorNumber != playerActorNumber)
            {
                return player.ActorNumber;
            }
        }
        return -1;
    }

    private void EndMatch(int winnerActorNumber)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            photonView.RPC("RPC_EndMatch", RpcTarget.All, winnerActorNumber);
        }
    }

    [PunRPC]
    private void RPC_EndMatch(int winnerActorNumber)
    {
        isMatchInProgress = false;
        Debug.Log($"Match ended! Player {winnerActorNumber} wins!");
        // TODO: Implement match end UI and restart logic
    }

    // Method to change match format (can be called before starting a new match)
    public void SetMatchFormat(int roundsToWin, int maxRounds)
    {
        if (!isMatchInProgress && PhotonNetwork.IsMasterClient)
        {
            currentConfig.roundsToWin = roundsToWin;
            currentConfig.maxRounds = maxRounds;
            photonView.RPC("RPC_UpdateMatchFormat", RpcTarget.All, roundsToWin, maxRounds);
        }
    }

    [PunRPC]
    private void RPC_UpdateMatchFormat(int roundsToWin, int maxRounds)
    {
        currentConfig.roundsToWin = roundsToWin;
        currentConfig.maxRounds = maxRounds;
    }
}
