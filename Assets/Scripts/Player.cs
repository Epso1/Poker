using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using static DeckManager;


public class Player : MonoBehaviour
{
    [SerializeField] public string playerName;
    [SerializeField] public Transform card1Position;
    [SerializeField] public Transform card2Position;
    [SerializeField] TextMeshProUGUI playerNameText;
    [SerializeField] public TextMeshProUGUI playerCreditText;
    [SerializeField] Image card1Image;
    [SerializeField] Image card2Image;
    [SerializeField] public int credit;
    [SerializeField] GameObject dealerTableUIIcon;
    [SerializeField] GameObject smallBlindTableUIIcon;
    [SerializeField] GameObject bigBlindTableUIIcon;
    [SerializeField] public Image playerTurnImage;
    [SerializeField] TextMeshProUGUI currentBetTableText;
    [SerializeField] GameObject checkUIIcon;
    [SerializeField] GameObject foldUIIcon;
    [SerializeField] GameObject dealerUIIcon;
    [SerializeField] GameObject smallBlindUIIcon;
    [SerializeField] GameObject bigBlindUIIcon;

    public bool isHuman;
    public string role;
    public List<Card> hand = new List<Card>();
    DeckManager deckManager;
    public int currentBet;
    public bool hasActed = false;
    public bool isPlayerActive = true;
    Color playerTurnImageInitialColor;
    private void Awake()
    {
        playerTurnImageInitialColor = playerTurnImage.color;
        dealerTableUIIcon.SetActive(false);
        smallBlindTableUIIcon.SetActive(false);
        bigBlindTableUIIcon.SetActive(false);
        checkUIIcon.SetActive(false);
        foldUIIcon.SetActive(false);
        deckManager = FindObjectOfType<DeckManager>();
    }
    private void Start()
    {
        if (playerNameText != null)
        {
            UpdatePlayerNameText();
        }

        if (playerCreditText != null)
        {
            UpdateCreditText();
        }
    }

    public string FormatCurrency(int value)
    {
        return string.Format("${0:N0}", value);
    }

    void UpdatePlayerNameText()
    {
        playerNameText.text = playerName;
    }

    public void UpdateCreditText()
    {
        playerCreditText.text = FormatCurrency(credit);
    }

    public void UpdateTableUICurrentBet()
    {
        currentBetTableText.text = FormatCurrency(currentBet);
    }

    public void UpdateUIHand()
    {
        // Verificar que la mano tenga al menos dos cartas
        if (hand.Count >= 2 && isHuman)
        {
            // Buscar el sprite de la primera carta
            Sprite card1Sprite = deckManager.FindSpriteByName(hand[0].cardObject.name);
            if (card1Sprite != null)
            {
                card1Image.sprite = card1Sprite;
                card1Image.gameObject.SetActive(true); // Mostrar la imagen si estaba oculta
            }
            else
            {
                Debug.LogWarning($"Sprite for card {hand[0].cardObject.name} not found.");
            }

            // Buscar el sprite de la segunda carta
            Sprite card2Sprite = deckManager.FindSpriteByName(hand[1].cardObject.name);
            if (card2Sprite != null)
            {
                card2Image.sprite = card2Sprite;
                card2Image.gameObject.SetActive(true); // Mostrar la imagen si estaba oculta
            }
            else
            {
                Debug.LogWarning($"Sprite for card {hand[1].cardObject.name} not found.");
            }
        }
        else if (hand.Count < 2)
        {
            Debug.LogWarning("Not enough cards in hand to update UI.");
        }
    }
    
    public void ResetStateIcons()
    {
        checkUIIcon.SetActive(false);
        foldUIIcon.SetActive(false);
    }

    public void ResetUIRole()
    {
        dealerTableUIIcon.SetActive(false);
        smallBlindTableUIIcon.SetActive(false);
        bigBlindTableUIIcon.SetActive(false);
        if (isHuman)
        {
            dealerUIIcon.SetActive(false);
            smallBlindUIIcon.SetActive(false);
            bigBlindUIIcon.SetActive(false);
        }
        
    }
    public void SetRole(string newRole)
    {
        role = newRole;
        switch (role)
        {
            case "Dealer":
                dealerTableUIIcon.SetActive(true);
                smallBlindTableUIIcon.SetActive(false);
                bigBlindTableUIIcon.SetActive(false);
                if (isHuman)
                {
                    dealerUIIcon.SetActive(true);
                    smallBlindUIIcon.SetActive(false);
                    bigBlindUIIcon.SetActive(false);
                }
                break;

            case "Small Blind":
                dealerTableUIIcon.SetActive(false);
                smallBlindTableUIIcon.SetActive(true);
                bigBlindTableUIIcon.SetActive(false);
                if (isHuman)
                {
                    dealerUIIcon.SetActive(false);
                    smallBlindUIIcon.SetActive(true);
                    bigBlindUIIcon.SetActive(false);
                }
                break;

            case "Big Blind":
                dealerTableUIIcon.SetActive(false);
                smallBlindTableUIIcon.SetActive(false);
                bigBlindTableUIIcon.SetActive(true);
                if (isHuman)
                {
                    dealerUIIcon.SetActive(false);
                    smallBlindUIIcon.SetActive(false);
                    bigBlindUIIcon.SetActive(true);
                }
                break;

            default:
                dealerTableUIIcon.SetActive(false);
                smallBlindTableUIIcon.SetActive(false);
                bigBlindTableUIIcon.SetActive(false);
                if (isHuman)
                {
                    dealerUIIcon.SetActive(false);
                    smallBlindUIIcon.SetActive(false);
                    bigBlindUIIcon.SetActive(false);
                }
                break;
        }
    }
    
    public string GetRole()
    {
        return role;
    }


    public void StartTurn(int currentBet, int pot)
    {
        // Mostrar UI para que el jugador elija una acción
        //GameObject.FindWithTag("BetWindow").SetActive(true);
        playerTurnImage.color = playerTurnImageInitialColor;
        Debug.Log($"{playerName} puede igualar {currentBet}, aumentar o retirarse. El bote es de : ${pot}.");
    }

    public void Bet(int amount)
    {
        if (amount > credit)
        {
            Debug.LogWarning($"{playerName} no tiene suficientes fichas.");
            return;
        }

        credit -= amount;
        if (playerCreditText != null)
        {
            UpdateCreditText();
        }
        currentBet += amount;
        UpdateTableUICurrentBet();
        foldUIIcon.SetActive(false);
        checkUIIcon.SetActive(true);
        Debug.Log($"{playerName} apuesta {amount}. Total apostado: {currentBet}");
    }

    public void Fold()
    {
        isPlayerActive = false;
        foldUIIcon.SetActive(true);
        checkUIIcon.SetActive(false);
        Debug.Log($"{playerName} se retira de la mano.");
    }

    public bool HasMatchedBet(int currentBet)
    {
        return this.currentBet >= currentBet;
    }

    public void ResetBet()
    {
        currentBet = 0;
    }


}
