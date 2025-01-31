using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;
using Unity.Collections;

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
    [SerializeField] public int pot = 0;
    private DeckManager deckManager;
    [SerializeField] private int bigBlindAmount = 40;
    [SerializeField] private TextMeshProUGUI raiseText;
    [SerializeField] private TextMeshProUGUI callText;
    private int hand = 0;

    private void Start()
    {
        deckManager = FindObjectOfType<DeckManager>();
        players = deckManager.players;
        deckManager.StartNewHand();
        Invoke("StartBettingRound", 1f);
    }

    private void StartBettingRound()
    {
        if (gameState == GameState.PreFlop)
        {
            currentPlayerIndex = (GetBigBlindPlayerIndex() + 1) % players.Count();
            foreach (Player player in players)
            {
                player.ResetBet();
            }
            currentBet = GetBigBlindAmount();
            
            pot = GetBigBlindAmount() + GetSmallBlindAmount();
            StartPlayerTurn();
        }
        else
        {
            currentPlayerIndex = (GetBigBlindPlayerIndex() + 1) % players.Count();
            StartPlayerTurn();
        }
    }

    private void StartPlayerTurn()
    {
        if (players[currentPlayerIndex].IsActive)
        {
            Debug.Log($"Turno del jugador: {players[currentPlayerIndex].playerName}");
            callText.text = "$" + currentBet.ToString();
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
                StartBettingRound();
                break;
            case GameState.Flop:
                gameState = GameState.Turn;
                DealTurnCard();
                StartBettingRound();
                break;
            case GameState.Turn:
                gameState = GameState.River;
                DealRiverCard();
                StartBettingRound();
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
        deckManager.DealFlopCards();
    }

    private void DealTurnCard()
    {
        Debug.Log("Repartiendo carta del Turn.");
        deckManager.DealTurnCard();
    }

    private void DealRiverCard()
    {
        Debug.Log("Repartiendo carta del River.");
        deckManager.DealRiverCard();
    }

    private void EvaluateHands()
    {
        Debug.Log("Evaluando manos y determinando el ganador.");
        deckManager.EvaluateHands();
    }

    public void PlayerCall()
    {
        Player currentPlayer = players[currentPlayerIndex];

        int callAmount = currentBet - currentPlayer.CurrentBet; ;
        currentPlayer.Bet(callAmount);
        pot += callAmount;

        //GameObject.FindWithTag("BetWindow").SetActive(false);
        AdvanceToNextPlayer();
    }

    public void PlayerRaise20()
    {
        string textToParse = raiseText.text.Remove(0 ,1); 
        int initialValue = int.Parse(textToParse);        
        int newValue = initialValue + 20;
        raiseText.text = "$" + newValue.ToString();
    }

    public void PlayerRaise100()
    {
        string textToParse = raiseText.text.Remove(0, 1);
        int initialValue = int.Parse(textToParse);
        int newValue = initialValue + 100;
        raiseText.text = "$" + newValue.ToString();
    }

    public void PlayerRaise()
    {
        Player currentPlayer = players[currentPlayerIndex];
        string textToParse = raiseText.text.Remove(0, 1);
        int amount = int.Parse(textToParse);
        int raiseAmount = amount - currentPlayer.CurrentBet;
        currentPlayer.Bet(raiseAmount);
        currentBet += raiseAmount;
        pot += currentBet;

        //GameObject.FindWithTag("BetWindow").SetActive(false);
        AdvanceToNextPlayer();
    } 
    
}
