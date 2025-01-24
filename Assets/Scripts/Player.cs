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
    [SerializeField] TextMeshProUGUI playerCreditText;
    [SerializeField] Image card1Image;
    [SerializeField] Image card2Image;
    [SerializeField] public int credit;
    [SerializeField] GameObject dealerUIIcon;
    [SerializeField] GameObject smallBlindUIIcon;
    [SerializeField] GameObject bigBlindUIIcon;

    public string role;
    public List<Card> hand = new List<Card>();
    DeckManager deckManager;

    private void Awake()
    {
        dealerUIIcon.SetActive(false);
        smallBlindUIIcon.SetActive(false);
        bigBlindUIIcon.SetActive(false);
    }
    private void Start()
    {
        deckManager = FindObjectOfType<DeckManager>();

        if (card1Image != null && card2Image != null)
        {
            UpdateUIHand();
        }     

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

    void UpdateCreditText()
    {
        playerCreditText.text = FormatCurrency(credit);
    }

    void UpdateUIHand()
    {
        // Verificar que la mano tenga al menos dos cartas
        if (hand.Count >= 2)
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
        else
        {
            Debug.LogWarning("Not enough cards in hand to update UI.");
        }
    }

    public void SetRole(string newRole)
    {
        role = newRole;
        switch (role)
        {
            case "Dealer":
                dealerUIIcon.SetActive(true);
                smallBlindUIIcon.SetActive(false);
                bigBlindUIIcon.SetActive(false);
                break;

            case "Small Blind":
                dealerUIIcon.SetActive(false);
                smallBlindUIIcon.SetActive(true);
                bigBlindUIIcon.SetActive(false);
                break;

            case "Big Blind":
                dealerUIIcon.SetActive(false);
                smallBlindUIIcon.SetActive(false);
                bigBlindUIIcon.SetActive(true);
                break;

            default:
                break;
        }
    } 

}
