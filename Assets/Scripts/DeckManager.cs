using System.Collections.Generic;
using UnityEngine;

public class DeckManager : MonoBehaviour
{
    [SerializeField] List<GameObject> cardPrefabs; // Lista de prefabs de cartas
    [SerializeField] Transform deckSpawnPoint;    // Punto donde aparecerá el mazo
    [SerializeField] List<Player> players;        // Lista de jugadores en la partida
    private List<Card> deck = new List<Card>(); // Lista de cartas en el mazo

    // Clase que representa una carta
    public class Card
    {
        public string suit;  // Palo: Clubs, Diamonds, Hearts, Spades
        public string rank;  // Rango: 2, 3, ..., J, Q, K, Ace
        public GameObject prefab; // Prefab asociado a la carta
        public GameObject cardObject; // Instancia del prefab (opcional)
    }

    private void Start()
    {
        CreateDeck();   // Crear las cartas en memoria
        ShuffleDeck();  // Barajar el mazo
        InstantiateDeck(); // Instanciar las cartas barajadas
        DealInitialCards(); // Repartir las cartas iniciales
    }

    // Crear las cartas en memoria
    private void CreateDeck()
    {
        foreach (GameObject prefab in cardPrefabs)
        {
            (string suit, string rank) = ParseCardName(prefab.name);
            deck.Add(new Card { suit = suit, rank = rank, prefab = prefab });
        }
    }

    // Barajar el mazo
    private void ShuffleDeck()
    {
        for (int i = deck.Count - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);
            var temp = deck[i];
            deck[i] = deck[randomIndex];
            deck[randomIndex] = temp;
        }
    }

    // Instanciar las cartas en la escena
    private void InstantiateDeck()
    {
        float cardHeight = 0.03f;
        float currentY = deckSpawnPoint.position.y;

        foreach (Card card in deck)
        {
            Vector3 cardPosition = new Vector3(deckSpawnPoint.position.x, currentY, deckSpawnPoint.position.z);
            Quaternion cardRotation = Quaternion.Euler(90, 0, 0);

            GameObject newCard = Instantiate(card.prefab, cardPosition, cardRotation);
            newCard.transform.localScale = new Vector3(15f, 15f, 15f);
            newCard.name = card.prefab.name;

            card.cardObject = newCard;
            currentY += cardHeight;
        }
    }

    // Repartir las cartas iniciales
    private void DealInitialCards()
    {
        foreach (Player player in players)
        {
            Debug.Log("Dealing cards to " + player.name);
            // Repartir la primera carta
            Card firstCard = DrawCard();
            if (firstCard != null)
            {
                player.hand.Add(firstCard);
                InstantiateCardAtPosition(firstCard, player.card1Position);
            }

            // Repartir la segunda carta
            Card secondCard = DrawCard();
            if (secondCard != null)
            {
                player.hand.Add(secondCard);
                InstantiateCardAtPosition(secondCard, player.card2Position);
            }
        }
    }

    // Instanciar una carta en una posición específica
    private void InstantiateCardAtPosition(Card card, Transform position)
    {
        if (position != null)
        {
            Vector3 cardPosition = position.position;
            Quaternion cardRotation = position.rotation;

            GameObject newCard = Instantiate(card.prefab, cardPosition, cardRotation);
            newCard.transform.localScale = new Vector3(15f, 15f, 15f);
            newCard.name = card.prefab.name;

            card.cardObject = newCard; // Asociar el objeto físico con la carta
        }
        else
        {
            Debug.LogWarning("No se ha asignado una posición para esta carta.");
        }
    }

    // Sacar una carta del mazo
    private Card DrawCard()
    {
        if (deck.Count > 0)
        {
            Card drawnCard = deck[0];
            deck.RemoveAt(0);
            return drawnCard;
        }
        else
        {
            Debug.LogWarning("No hay más cartas en el mazo");
            return null;
        }
    }

    // Extraer el rango y el palo del nombre del prefab
    private (string suit, string rank) ParseCardName(string cardName)
    {
        string[] parts = cardName.Split('_');
        if (parts.Length != 2 || string.IsNullOrEmpty(parts[1]))
        {
            Debug.LogError($"Formato de nombre inválido: {cardName}. Se espera 'Card_SuitRank'.");
            return ("Unknown", "Unknown");
        }

        int digitIndex = parts[1].IndexOfAny("0123456789".ToCharArray());
        if (digitIndex == -1)
        {
            return (parts[1].Substring(0, parts[1].Length - 3), parts[1].Substring(parts[1].Length - 3));
        }
        else
        {
            return (parts[1].Substring(0, digitIndex), parts[1].Substring(digitIndex));
        }
    }


    public void ResetDeck()
    {
        foreach (Card card in deck)
        {
            if (card.cardObject != null)
            {
                Destroy(card.cardObject); // Destruye las instancias físicas
            }
        }
        deck.Clear();
        CreateDeck();
        ShuffleDeck();
        InstantiateDeck();
    }

}
