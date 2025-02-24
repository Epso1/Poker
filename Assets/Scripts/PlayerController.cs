using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Unity.Netcode;
using Unity.Collections;



public class PlayerController : NetworkBehaviour
{
    [SerializeField] public string playerName;
    private readonly NetworkVariable<FixedString64Bytes> networkPlayerName = new NetworkVariable<FixedString64Bytes>(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    [SerializeField] public string playerID;
    [SerializeField] public int credit;
    [SerializeField] public Transform card1Position;
    [SerializeField] public Transform card2Position;
    public bool isHuman;
    public string role;
    public List<Card> hand = new List<Card>();
    public int currentBet;
    public bool hasActed = false;
    public bool isPlayerActive = true;

    // UI de mesa del jugador
    [SerializeField] GameObject dealerTableUIIcon;
    [SerializeField] GameObject smallBlindTableUIIcon;
    [SerializeField] GameObject bigBlindTableUIIcon;
    [SerializeField] public Image playerTurnImage;
    [SerializeField] TextMeshProUGUI currentBetTableText;
    [SerializeField] GameObject checkUIIcon;
    [SerializeField] GameObject foldUIIcon;

    // UI para jugador Humano Activo
    TextMeshProUGUI playerNameText;
    public TextMeshProUGUI playerCreditText;
    [SerializeField] Image card1Image;
    [SerializeField] Image card2Image;
    [SerializeField] GameObject dealerUIIcon;
    [SerializeField] GameObject smallBlindUIIcon;
    [SerializeField] GameObject bigBlindUIIcon;

    DeckManager deckManager;
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
        playerNameText = deckManager.playerNameText;
        playerCreditText = deckManager.playerCreditText;
        card1Image = deckManager.card1Image;
        card2Image = deckManager.card2Image;
        dealerUIIcon = deckManager.dealerUIIcon;
        smallBlindUIIcon = deckManager.smallBlindUIIcon;
        bigBlindUIIcon = deckManager.bigBlindUIIcon;

        if (playerNameText != null)
        {
            UpdatePlayerNameText();
        }

        if (playerCreditText != null)
        {
            UpdateCreditText();
        }
    }

    public override void OnNetworkSpawn()
    {
        if (IsClient)
        {
            // Suscribirse a cambios en el nombre
            networkPlayerName.OnValueChanged += OnPlayerNameChanged;

            // Establecer el nombre inicial
            playerName = networkPlayerName.Value.ToString();
            UpdatePlayerNameText();
        }
    }

    // Se ejecuta cuando cambia el nombre del jugador
    private void OnPlayerNameChanged(FixedString64Bytes oldName, FixedString64Bytes newName)
    {
        playerName = newName.ToString();
        UpdatePlayerNameText();
    }

    // Llamado para establecer el nombre del jugador
    public void SetPlayerName(string name)
    {
        if (IsServer)
        {
            networkPlayerName.Value = name; // El servidor establece el nombre directamente
        }
        else
        {
            SetPlayerNameServerRpc(name); // Los clientes piden al servidor que lo haga
        }
    }

    [ServerRpc]
    private void SetPlayerNameServerRpc(string name)
    {
        networkPlayerName.Value = name;
    }  


    public string FormatCurrency(int value)
    {
        return string.Format("${0:N0}", value);
    }

    // Actualiza el texto del nombre en la UI
    public void UpdatePlayerNameText()
    {
        if (!IsOwner) return; // Solo el jugador dueño de este objeto actualiza su UI local

        if (playerNameText != null)
        {
            playerNameText.text = playerName;
            Debug.Log($"Nombre actualizado para el jugador local: {playerName}");
        }
        else
        {
            Debug.LogWarning("¡playerNameText no está asignado en este cliente!");
        }
    }

    public void UpdateCreditText()
    {
        if (!IsOwner) return; // Solo el dueño del objeto actualiza su UI local

        if (playerCreditText != null)
        {
            playerCreditText.text = FormatCurrency(credit);
            Debug.Log($"Crédito actualizado para el jugador local: {credit}");
        }
        else
        {
            Debug.LogWarning("¡playerCreditText no está asignado en este cliente!");
        }
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
