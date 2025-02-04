using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

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
    [SerializeField] private int currentPlayerIndex = 0;
    [SerializeField] private GameState gameState = GameState.PreFlop;
    [SerializeField] private int currentBet = 0;
    [SerializeField] public int pot = 0;
    [SerializeField] int smallBlindAmount = 20;
    [SerializeField] int bigBlindAmount = 40;
    [SerializeField] TextMeshProUGUI raiseText;
    [SerializeField] TextMeshProUGUI callText;
    [SerializeField] Button callButton;
    [SerializeField] TextMeshProUGUI callButtonText;
    [SerializeField] Button raiseButton;
    [SerializeField] TextMeshProUGUI raiseButtonText;
    List<Player> players = new List<Player>();
    DeckManager deckManager;

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
            pot = 0;
            deckManager.UpdateUIPot(pot);
            foreach (Player player in players)
            {
                player.ResetBet();
            }
            int bigBlindPlayerIndex = GetBigBlindPlayerIndex();
            int smallBlindPlayerIndex = GetSmallBlindPlayerIndex();
            players[smallBlindPlayerIndex].Bet(smallBlindAmount);
            players[bigBlindPlayerIndex].Bet(bigBlindAmount);
            pot += smallBlindAmount + bigBlindAmount;
            deckManager.UpdateUIPot(pot);
            currentPlayerIndex = (GetBigBlindPlayerIndex() + 1) % players.Count();           
            currentBet = bigBlindAmount;            
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
        if (players[currentPlayerIndex].isActive)
        {
            Debug.Log($"Turno del jugador: {players[currentPlayerIndex].playerName} - Apuesta actual del jugador: {players[currentPlayerIndex].currentBet}");
            int callAmount = currentBet - players[currentPlayerIndex].currentBet;
            if (callAmount == 0)
            {
                callButtonText.text = "CHECK";
                raiseButtonText.text = "BET";
                callText.text = "";

            }
            else
            {
                callButtonText.text = "CALL";
                raiseButtonText.text = "RAISE";
                callText.text = "$" + callAmount.ToString();
            }
            
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

            foreach (var player in players)
            {
                player.hasActed = false;
            }

            ProceedToNextGameState();
        }
        else
        {
            StartPlayerTurn();
        }
    }

    private bool IsRoundComplete()
    {
        // Si solo queda un jugador activo, la ronda termina
        if (players.Count(player => player.isActive) == 1) return true;

        // Si todos los jugadores activos han igualado la apuesta más alta y han actuado, la ronda termina
        return players.All(player => !player.isActive || (player.hasActed && player.HasMatchedBet(currentBet)) );
   
    }

    private void ProceedToNextGameState()
    {
        switch (gameState)
        {
            case GameState.PreFlop:
                Debug.Log("***** Gamestate: preflop => flop *****");
                gameState = GameState.Flop;
                DealFlopCards();
                StartBettingRound();
                break;
            case GameState.Flop:
                Debug.Log("***** Gamestate: flop => turn *****");
                gameState = GameState.Turn;
                DealTurnCard();
                StartBettingRound();
                break;
            case GameState.Turn:
                Debug.Log("***** Gamestate: turn => river *****");
                gameState = GameState.River;
                DealRiverCard();
                StartBettingRound();
                break;
            case GameState.River:
                Debug.Log("***** Gamestate: river => showdown *****");
                gameState = GameState.Showdown;
                EvaluateHands();
                Debug.Log("Partida terminada. repartiendo una nueva mano.");
                ProceedToNextGameState();
                break;
            case GameState.Showdown:
                Debug.Log("***** Gamestate: showdown => preflop *****");
                gameState = GameState.PreFlop;
                deckManager.ResetDeck();
                StartBettingRound();
                break;
        }
       
    }

    

    private int GetBigBlindPlayerIndex()
    {
        return players.FindIndex(player => player.GetRole() == "Big Blind");
    }
    private int GetSmallBlindPlayerIndex()
    {
        return players.FindIndex(player => player.GetRole() == "Small Blind");
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

        int callAmount = currentBet - currentPlayer.currentBet;
        currentPlayer.Bet(callAmount);
        pot += callAmount;
        deckManager.UpdateUIPot(pot);

        //GameObject.FindWithTag("BetWindow").SetActive(false);
        currentPlayer.GetComponent<Player>().hasActed = true;
        AdvanceToNextPlayer();
    }

    public void RaiseTextAdd20()
    {
        string textToParse = raiseText.text.Remove(0 ,1); 
        int initialValue = int.Parse(textToParse);        
        int newValue = initialValue + 20;
        raiseText.text = "$" + newValue.ToString();
    }

    public void RaiseTextSubtract20()
    {
        string textToParse = raiseText.text.Remove(0, 1);
        int initialValue = int.Parse(textToParse);
        int newValue = initialValue - 20;
        if (newValue > 20)
        {
            raiseText.text = "$" + newValue.ToString();
        }
        else { raiseText.text = "$20"; }
        
    }

    public void PlayerRaise()
    {
        Player currentPlayer = players[currentPlayerIndex];
        string textToParse = raiseText.text.Remove(0, 1);
        int amount = int.Parse(textToParse);
        int callAmount = currentBet - currentPlayer.currentBet;
        int raiseAmount = amount + callAmount;
        currentPlayer.Bet(raiseAmount);
        currentBet += amount;
        pot += raiseAmount;
        deckManager.UpdateUIPot(pot);

        //GameObject.FindWithTag("BetWindow").SetActive(false);
        currentPlayer.GetComponent<Player>().hasActed = true;
        AdvanceToNextPlayer();
    } 
    public void PlayerFold()
    {
        Player currentPlayer = players[currentPlayerIndex];
        currentPlayer.GetComponent<Player>().Fold();
        currentPlayer.GetComponent<Player>().hasActed = true;
        AdvanceToNextPlayer();
    }
}
