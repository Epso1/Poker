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
    private int currentPlayerIndex = 0;
    private GameState gameState = GameState.PreFlop;
    private int currentBet = 0;
    public int pot = 0;
    [SerializeField] int smallBlindAmount = 20;
    [SerializeField] int bigBlindAmount = 40;
    [SerializeField] TextMeshProUGUI raiseText;
    [SerializeField] TextMeshProUGUI callText;
    [SerializeField] TextMeshProUGUI callButtonText;
    [SerializeField] TextMeshProUGUI raiseButtonText;
    [SerializeField] TextMeshProUGUI gameInfoText;
    [SerializeField] GameObject nextRoundButton;
    List<PlayerController> players = new List<PlayerController>();
    DeckManager deckManager;
    private void Start()
    {
        gameInfoText.text = "";
        nextRoundButton.SetActive(false);
        deckManager = FindObjectOfType<DeckManager>();
        players = deckManager.players;
        //deckManager.StartNewHand();
        //AssignPlayersToSeats();
        //StartBettingRound();
    }

    public void InitializeGame()
    {
        deckManager.StartNewHand();
        //AssignPlayersToSeats();
        StartBettingRound();
    }
    void AssignPlayersToSeats()
    {
        List<PlayerData> playersData = DataManager.Instance.connectedPlayers;

        for (int i = 0; i < playersData.Count; i++)
        {
            players[i].playerName = playersData[i].playerName;

            // Actualizar la UI con el nombre del jugador
            players[i].UpdatePlayerNameText();
        }
    }
    private void StartBettingRound()
    {
        
        if (gameState == GameState.PreFlop)
        {
            pot = 0;
            deckManager.UpdateUIPot(pot);
            foreach (PlayerController player in players)
            {
                player.ResetBet();
                player.UpdateTableUICurrentBet();
                if (!player.isPlayerActive) player.isPlayerActive = true;
                player.ResetStateIcons();
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
            currentPlayerIndex = GetFirstActivePlayerAfterDealerOrSmallBlind();
            StartPlayerTurn();
        }
    }

    private void StartPlayerTurn()
    {
        gameInfoText.text = "";
        if (players[currentPlayerIndex].isPlayerActive)
        {
            gameInfoText.text = $"{players[currentPlayerIndex].playerName}'s turn";
            Debug.Log($"Turno del jugador: {players[currentPlayerIndex].playerName}\nApuesta actual del jugador: {players[currentPlayerIndex].currentBet}");
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
            foreach (PlayerController player in players)
            {
                Color alphaColor = new Color(0, 0, 0, 0);
                player.playerTurnImage.color = alphaColor;
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
            if (players.Count(player => player.isPlayerActive) == 1)
            {
                gameState = GameState.River;
                Debug.Log("Ronda completa con un sólo jugador activo. Finalizando partida.");
            }
            else
            {
                Debug.Log("Ronda completa. Avanzando a la siguiente fase.");
                foreach (var player in players)
                {
                    player.hasActed = false;
                }               
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
        // Si todos los jugadores activos han igualado la apuesta más alta y han actuado, la ronda termina
        return players.All(player => !player.isPlayerActive || (player.hasActed && player.HasMatchedBet(currentBet)) );   
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
                DetermineWinner();
                //Debug.Log("Partida terminada. repartiendo una nueva mano.");
                //ProceedToNextGameState();
                break;
            case GameState.Showdown:
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

    private int GetFirstActivePlayerAfterDealerOrSmallBlind()
    {
        List<PlayerController> activePlayers = players.Where(player => player.isPlayerActive).ToList();

        if (activePlayers.Count == 2)
        {
            // Si solo hay dos jugadores, el que era Small Blind empieza la ronda postflop
            return players.FindIndex(player => player.GetRole() == "Small Blind");
        }

        int dealerIndex = players.FindIndex(player => player.GetRole() == "Dealer");

        // Buscar el primer jugador activo a la izquierda del dealer
        for (int i = 1; i < players.Count; i++)
        {
            int index = (dealerIndex + i) % players.Count;
            if (players[index].isPlayerActive)
            {
                return index;
            }
        }

        return dealerIndex; // Si por algún motivo no hay otro activo, devolver el dealer (evitar errores)
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

    private void DetermineWinner()
    {
        Debug.Log("Evaluando manos y determinando el ganador.");
        var (winner, bestHand) = deckManager.EvaluateHands();
        gameInfoText.text = $"{winner.playerName} wins the hand \nwith {bestHand}\nand takes a pot of {FormatCurrency(pot)}";
        Debug.Log($"{winner.playerName} gana la mano \ncon {bestHand}\ny se lleva un bote de ${FormatCurrency(pot)}.");
        foreach(PlayerController player in players)
        {
            if (player == winner)
            {
                player.credit += pot;
                if (player.playerCreditText != null) player.UpdateCreditText();
            }
        }
        nextRoundButton.SetActive(true);
    }
    public string FormatCurrency(int value)
    {
        return string.Format("${0:N0}", value);
    }

    public void PlayerCall()
    {
        PlayerController currentPlayer = players[currentPlayerIndex];

        int callAmount = currentBet - currentPlayer.currentBet;
        currentPlayer.Bet(callAmount);
        pot += callAmount;
        deckManager.UpdateUIPot(pot);

        //GameObject.FindWithTag("BetWindow").SetActive(false);
        currentPlayer.GetComponent<PlayerController>().hasActed = true;
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
        PlayerController currentPlayer = players[currentPlayerIndex];
        string textToParse = raiseText.text.Remove(0, 1);
        int amount = int.Parse(textToParse);
        int callAmount = currentBet - currentPlayer.currentBet;
        int raiseAmount = amount + callAmount;
        currentPlayer.Bet(raiseAmount);
        currentBet += amount;
        pot += raiseAmount;
        deckManager.UpdateUIPot(pot);

        //GameObject.FindWithTag("BetWindow").SetActive(false);
        currentPlayer.GetComponent<PlayerController>().hasActed = true;
        AdvanceToNextPlayer();
    } 
    public void PlayerFold()
    {
        PlayerController currentPlayer = players[currentPlayerIndex];
        currentPlayer.GetComponent<PlayerController>().Fold();
        currentPlayer.GetComponent<PlayerController>().hasActed = true;
        AdvanceToNextPlayer();
    }

    public void ToNextRound()
    {
        gameState = GameState.PreFlop;
        deckManager.ResetDeck();
        StartBettingRound();
        nextRoundButton.SetActive(false);
    }
}
