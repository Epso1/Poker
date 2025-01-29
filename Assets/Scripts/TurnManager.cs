using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum GameState
{
    PreFlop,
    Flop,
    Turn,
    River,
    Showdown
}

public class TurnManager : MonoBehaviour
{
    private List<Player> players = new List<Player>();
    [SerializeField] private int currentPlayerIndex = 0;
    [SerializeField] private GameState gameState = GameState.PreFlop;
    [SerializeField] private int currentBet = 0;
    [SerializeField] private int pot = 0;
    private DeckManager deckManager;
    [SerializeField] private int bigBlindAmount = 40;

    private void Start()
    {
        deckManager = FindObjectOfType<DeckManager>();
        players = deckManager.players;
        deckManager.StartNewHand();
        Invoke("StartBettingRound", 1f);
    }

    private void StartBettingRound()
    {
        currentPlayerIndex = (GetBigBlindPlayerIndex() + 1)  % players.Count() ;
        foreach (Player player in players)
        {
            player.ResetBet();
        }
        currentBet = GetBigBlindAmount();
        pot = GetBigBlindAmount() + GetSmallBlindAmount();
        StartPlayerTurn();
    }

    private void StartPlayerTurn()
    {
        if (players[currentPlayerIndex].IsActive)
        {
            Debug.Log($"Turno del jugador: {players[currentPlayerIndex].playerName}");
            players[currentPlayerIndex].StartTurn(currentBet, pot);
        }
        else
        {
            AdvanceToNextPlayer();
        }
    }

    private void AdvanceToNextPlayer()
    {
        currentPlayerIndex = (currentPlayerIndex + 1) % players.Count;

        if (IsRoundComplete())
        {
            Debug.Log("Ronda completa. Avanzando a la siguiente fase.");
            ProceedToNextGameState();
        }
        else
        {
            StartPlayerTurn();
        }
    }

    private bool IsRoundComplete()
    {
        return players.All(player => !player.IsActive || player.HasMatchedBet(currentBet));
    }

    private void ProceedToNextGameState()
    {
        switch (gameState)
        {
            case GameState.PreFlop:
                gameState = GameState.Flop;
                DealFlopCards();
                break;
            case GameState.Flop:
                gameState = GameState.Turn;
                DealTurnCard();
                break;
            case GameState.Turn:
                gameState = GameState.River;
                DealRiverCard();
                break;
            case GameState.River:
                gameState = GameState.Showdown;
                EvaluateHands();
                return;
        }
        StartBettingRound();
    }

    public void PlayerAction(string action, int amount = 0)
    {
        Player currentPlayer = players[currentPlayerIndex];

        switch (action.ToLower())
        {
            case "call":
                int callAmount = currentBet - currentPlayer.CurrentBet;
                currentPlayer.Bet(callAmount);
                pot += callAmount;
                break;
            case "raise":
                if (amount > currentBet)
                {
                    int raiseAmount = amount - currentPlayer.CurrentBet;
                    currentPlayer.Bet(raiseAmount);
                    currentBet = amount;
                    pot += raiseAmount;
                }
                else
                {
                    Debug.LogWarning("La cantidad de aumento debe ser mayor que la apuesta actual.");
                    return;
                }
                break;
            case "fold":
                currentPlayer.Fold();
                break;
            default:
                Debug.LogWarning("Acción no válida.");
                return;
        }
        AdvanceToNextPlayer();
    }

    private int GetBigBlindPlayerIndex()
    {
        return players.FindIndex(player => player.GetRole() == "Big Blind");
    }

    private int GetBigBlindAmount()
    {
        return bigBlindAmount;
    }

    private int GetSmallBlindAmount()
    {
        return bigBlindAmount / 2;
    }

    private void DealFlopCards()
    {
        Debug.Log("Repartiendo cartas del Flop.");
    }

    private void DealTurnCard()
    {
        Debug.Log("Repartiendo carta del Turn.");
    }

    private void DealRiverCard()
    {
        Debug.Log("Repartiendo carta del River.");
    }

    private void EvaluateHands()
    {
        Debug.Log("Evaluando manos y determinando el ganador.");
    }
}
