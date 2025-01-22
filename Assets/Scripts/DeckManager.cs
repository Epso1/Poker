using System.Collections.Generic;
using UnityEngine;

public class DeckManager : MonoBehaviour
{
    [SerializeField] List<Player> players; // Lista de jugadores en la partida
    [SerializeField] List<GameObject> cardPrefabs; // Lista de prefabs de cartas
    [SerializeField] Transform deckSpawnPoint; // Punto donde aparecerá el mazo    
    [SerializeField] List<Transform> communityCardsPositions; // Lista de puntos donde aperecerán las cartas comunitarias
    private List<Card> deck = new List<Card>(); // Lista de cartas en el mazo
    private GameObject deckContainer; // Contenedor del mazo    
    [SerializeField] public List<Card> communityCards; // Lista de cartas comunitarias

    // Clase que representa una carta
    public class Card
    {
        public string suit; // Palo: Clubs, Diamonds, Hearts, Spades
        public string rank; // Rango: 2, 3, ..., J, Q, K, Ace
        public GameObject prefab; // Prefab asociado a la carta
        public GameObject cardObject; // Instancia del prefab 
    }

    private void Awake()
    {
        CreateDeck(); // Crear las cartas en memoria
        ShuffleDeck(); // Barajar el mazo
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
        // Crear un GameObject contenedor llamado "Deck"
        if (deckContainer != null)
        {
            Destroy(deckContainer); // Elimina el contenedor anterior si existe
        }
        deckContainer = new GameObject("Deck");

        float cardHeight = 0.01f;
        float currentY = deckSpawnPoint.position.y;

        foreach (Card card in deck)
        {
            Vector3 cardPosition = new Vector3(deckSpawnPoint.position.x, currentY, deckSpawnPoint.position.z);
            Quaternion cardRotation = Quaternion.Euler(90, 0, 0);

            GameObject newCard = Instantiate(card.prefab, cardPosition, cardRotation);
            newCard.transform.localScale = new Vector3(10f, 10f, 10f);
            newCard.name = card.prefab.name;

            // Hacer que la carta sea hija del contenedor "Deck"
            newCard.transform.SetParent(deckContainer.transform);

            card.cardObject = newCard;
            currentY += cardHeight;
        }
    }

    // Repartir las cartas iniciales
    private void DealInitialCards()
    {
        foreach (Player player in players)
        {
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
        DealFlopCards();
    }

    // Repartir las 3 cartas centrales
    private void DealFlopCards()
    {
        communityCards = new List<Card>();

        for (int i = 0; i < 3; i++) // Reparte 3 cartas
        {
            Card card = DrawCard();
            communityCards.Add(card);
            InstantiateCardAtPosition(card, communityCardsPositions[i]);
        }

        foreach (Card card in communityCards)
        {
            card.cardObject.transform.Rotate(0, 180f, 0);
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
            newCard.transform.localScale = new Vector3(10f, 10f, 10f);
            newCard.name = card.prefab.name;

            card.cardObject = newCard; // Asociar el objeto físico con la carta
        }
        else
        {
            Debug.LogWarning("No se ha asignado una posición para esta carta.");
        }
    }

    // Sacar una carta del mazo y eliminar su GameObject del contenedor
    private Card DrawCard()
    {
        if (deck.Count > 0)
        {
            Card drawnCard = deck[0];
            deck.RemoveAt(0);

            // Eliminar el GameObject de la carta del mazo
            if (drawnCard.cardObject != null)
            {
                Destroy(drawnCard.cardObject);
            }

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
        if (deckContainer != null)
        {
            Destroy(deckContainer); // Elimina el contenedor y todas las cartas dentro
        }
        deck.Clear();
        CreateDeck();
        ShuffleDeck();
        InstantiateDeck();
    }
}
