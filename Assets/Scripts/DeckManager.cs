using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class DeckManager : MonoBehaviour
{
    [SerializeField] public List<Player> players; // Lista de jugadores en la partida
    [SerializeField] List<GameObject> cardPrefabs; // Lista de prefabs de cartas
    [SerializeField] Transform deckSpawnPoint; // Punto donde aparecerá el mazo    
    [SerializeField] List<Transform> communityCardsPositions; // Lista de puntos donde aperecerán las cartas comunitarias
    private List<Card> deck = new List<Card>(); // Lista de cartas en el mazo
    private GameObject deckContainer; // Contenedor del mazo    
    [SerializeField] public List<Card> communityCards; // Lista de cartas comunitarias
    [SerializeField] public Sprite[] cardSprites; // Lista de Sprites de las cartas en miniatura
    [SerializeField] Image[] cardUIImages;

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
       
    }

    void Start() 
    {
        
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.KeypadMultiply))
        {
            ResetDeck();
        }
    }

    // Crear las cartas en memoria
    public void CreateDeck()
    {
        foreach (GameObject prefab in cardPrefabs)
        {
            (string suit, string rank) = ParseCardName(prefab.name);
            deck.Add(new Card { suit = suit, rank = rank, prefab = prefab });
        }
    }

    // Barajar el mazo
    public void ShuffleDeck()
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
    public void InstantiateDeck()
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
    public void DealInitialCards()
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

            player.UpdateUIHand();
        }

        EvaluateHands();
    }

    private void UpdatePlayersUI()
    {
        foreach(Player player in players)
        {
            player.UpdateUIHand();
        }
    }

    // Repartir las 3 cartas centrales
    public void DealFlopCards()
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

        UpdateUICommunityCards();
        EvaluateHands();
    }

    public void DealTurnCard()
    {
        Card card = DrawCard();
        communityCards.Add(card);
        InstantiateCardAtPosition(card, communityCardsPositions[3]);
        card.cardObject.transform.Rotate(0, 180f, 0);

        UpdateUICommunityCards();
        EvaluateHands();
    }

    public void DealRiverCard()
    {
        Card card = DrawCard();
        communityCards.Add(card);
        InstantiateCardAtPosition(card, communityCardsPositions[4]);
        card.cardObject.transform.Rotate(0, 180f, 0);

        UpdateUICommunityCards();
        EvaluateHands();
    }

    public void SetInitialRoles()
    {
        if (players.Count() >= 3)
        {
            players[0].SetRole("Dealer");
            players[1].SetRole("Small Blind");
            players[2].SetRole("Big Blind");
        } 
        else if (players.Count() == 2)
        {
            players[0].SetRole("Small Blind");
            players[1].SetRole("Big Blind");
        }
    }
    public void RotateRoles()
    {
        // Identificar los roles actuales
        int bigBlindIndex = players.FindIndex(player => player.GetRole() == "Big Blind");
        int smallBlindIndex = players.FindIndex(player => player.GetRole() == "Small Blind");

        // Resetear el rol en la IU para todos los jugadores
        foreach (Player player in players) { player.ResetUIRole(); }

        if (players.Count() >= 3)
        {      
            // Calcular los índices para los nuevos roles
            int newBigBlindIndex = (bigBlindIndex + 1) % players.Count;
            int newSmallBlindIndex = (newBigBlindIndex - 1 + players.Count) % players.Count;
            int newDealerIndex = (newSmallBlindIndex - 1 + players.Count) % players.Count;

            // Cambiar roles a los jugadores
            players[newDealerIndex].SetRole("Dealer");
            players[newSmallBlindIndex].SetRole("Small Blind");
            players[newBigBlindIndex].SetRole("Big Blind");
        }
        else
        {
            // Cambiar roles a los jugadores
            players[smallBlindIndex].SetRole("Big Blind");
            players[bigBlindIndex].SetRole("Small Blind");
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
            //Debug.Log("Suit: " + parts[1].Substring(0, GetSecondUppercasePosition(parts[1])) + " - Rank " + parts[1].Substring(GetSecondUppercasePosition(parts[1])));
            return (parts[1].Substring(0, GetSecondUppercasePosition(parts[1])) , parts[1].Substring(GetSecondUppercasePosition(parts[1])));
        }
        else
        {
            //Debug.Log("Suit: " + parts[1].Substring(0, digitIndex) + " - Rank: "+ parts[1].Substring(digitIndex));
            return (parts[1].Substring(0, digitIndex), parts[1].Substring(digitIndex));
        }
    }

    private int GetSecondUppercasePosition(string text)
    {
        int secondUppercase = -1;
        int counter = 0;

        // Loop through the string to find the uppercase letters
        for (int i = 0; i < text.Length; i++)
        {
            if (char.IsUpper(text[i])) // Check if the character is uppercase
            {
                counter++;

                if (counter == 2) // If it's the second uppercase letter
                {
                    secondUppercase = i; // Store the position of the second uppercase
                    break; // No need to continue the loop once we find the second
                }
            }
        }

        return secondUppercase; // Return -1 if the second uppercase letter wasn't found
    }

    public void ResetDeck()
    {
        if (deckContainer != null) Destroy(deckContainer); // Elimina el contenedor y todas las cartas dentro
        deck.Clear();
        communityCards.Clear();
        foreach (Player player in players) 
        {
            player.hand.Clear();
        }       
        GameObject[] cards = GameObject.FindGameObjectsWithTag("Card");
        foreach (GameObject card in cards) Destroy(card.gameObject);  

        CreateDeck(); // Crear las cartas en memoria
        ShuffleDeck(); // Barajar el mazo
        InstantiateDeck(); // Instanciar las cartas barajadas
        RotateRoles(); // Cambia los roles de los jugadores para la nueva mano
        DealInitialCards(); // Repartir las cartas iniciales
    }

    void UpdateUICommunityCards()
    {
        int index = 0;
        foreach (Card card in communityCards)
        {
            Sprite cardSprite = FindSpriteByName(card.cardObject.name);
            cardUIImages[index].sprite = cardSprite;
            cardUIImages[index].gameObject.SetActive(true);
            index++;
        }   
    }

    // Método auxiliar para encontrar un sprite por nombre
    public Sprite FindSpriteByName(string name)
    {
        foreach (Sprite sprite in cardSprites)
        {
            if (sprite.name == name)
            {
                return sprite;
            }
        }
        return null;
    }

    public void EvaluateHands()
    {
        Player bestPlayer = null;
        string bestHandDescription = null;
        List<Card> bestHandCards = null;

        foreach (Player player in players)
        {
            // Combinar cartas personales y comunitarias
            List<Card> combinedCards = new List<Card>(player.hand);
            if (communityCards != null) combinedCards.AddRange(communityCards);

            // Evaluar la mejor jugada y las cartas que la forman
            (string handDescription, List<Card> handCards) = DetermineBestHand(combinedCards);

            // Mostrar la jugada del jugador
            Debug.Log($"Jugador {player.playerName}: {handDescription}");

            //Debug.Log($"Comparando mano del jugador {player.playerName} con la mejor mano actual.");
            // Comparar con la mejor mano actual

            if (bestHandCards != null)
            {
                if (CompareHands(bestHandCards, handCards) == -1)
                {
                    //Debug.Log($"La mano del jugador {player.playerName} es superior a la mejor mano actual.");
                    bestPlayer = player;
                    bestHandDescription = handDescription;
                    bestHandCards = handCards;
                }
            }
            else
            {
                //Debug.Log($"No hay bestHandCards.");
                bestPlayer = player;
                bestHandDescription = handDescription;
                bestHandCards = handCards;
            }
        }

        // Mostrar al ganador
        if (bestPlayer != null)
        {
            Debug.Log($"Mejor mano: {bestHandDescription} de {bestPlayer.playerName}");
        }
    }


    private (string description, List<Card> bestHand) DetermineBestHand(List<Card> cards)
    {
        var sortedCards = cards.OrderByDescending(card => GetCardValue(card.rank)).ToList();

        if (IsRoyalFlush(sortedCards)) return ("EscaleraReal", GetBestRoyalFlush(sortedCards));
        if (IsStraightFlush(sortedCards)) return ($"EscaleradeColor al {GetBestStraightFlush(sortedCards)[0].rank}", GetBestStraightFlush(sortedCards));
        if (IsFourOfAKind(sortedCards)) return ($"Poker de {GetBestFourOfAKind(sortedCards)[0].rank}", GetBestFourOfAKind(sortedCards));
        if (IsFullHouse(sortedCards)) return ($"FullHouse con {GetBestFullHouse(sortedCards)[0].rank} y {GetBestFullHouse(sortedCards)[3].rank}", GetBestFullHouse(sortedCards));
        if (IsFlush(sortedCards)) return ($"Color al {GetBestFlush(sortedCards)[0].rank}", GetBestFlush(sortedCards));
        if (IsStraight(sortedCards)) return ($"Escalera al {GetBestStraight(sortedCards)[0].rank}", GetBestStraight(sortedCards));
        if (IsThreeOfAKind(sortedCards)) return ($"Trio de {GetBestThreeOfAKind(sortedCards)[0].rank}", GetBestThreeOfAKind(sortedCards));
        if (IsTwoPair(sortedCards)) return ($"DoblePareja de {GetBestTwoPair(sortedCards)[0].rank} y {GetBestTwoPair(sortedCards)[2].rank}", GetBestTwoPair(sortedCards));      
        if (IsOnePair(sortedCards)) return ($"Pareja de {GetBestOnePair(sortedCards)[0].rank}", GetBestOnePair(sortedCards));

        return ($"CartaAlta {sortedCards[0].rank}", sortedCards);
    }

    private int CompareHands(List<Card> hand1, List<Card> hand2)
    {
        // Determinar la mejor jugada para ambas manos
        (string description1, List<Card> bestHand1) = DetermineBestHand(hand1);
        (string description2, List<Card> bestHand2) = DetermineBestHand(hand2);

        // Crear un diccionario para jerarquizar las jugadas
        Dictionary<string, int> handRanks = new Dictionary<string, int>
        {
            { "EscaleraReal", 10 },
            { "EscaleradeColor", 9 },
            { "Poker", 8 },
            { "FullHouse", 7 },
            { "Color", 6 },
            { "Escalera", 5 },
            { "Trio", 4 },
            { "DoblePareja", 3 },
            { "Pareja", 2 },
            { "CartaAlta", 1 }
        };

        // Obtener el rango de las jugadas
        int rank1 = handRanks.ContainsKey(description1.Split(' ')[0]) ? handRanks[description1.Split(' ')[0]] : 0;
        int rank2 = handRanks.ContainsKey(description2.Split(' ')[0]) ? handRanks[description2.Split(' ')[0]] : 0;

        // Comparar el rango de las jugadas
        if (rank1 > rank2) return 1;  // Mano 1 es mejor
        if (rank1 < rank2) return -1; // Mano 2 es mejor

        // Si las jugadas son iguales, desempatar por las cartas que forman la jugada
        for (int i = 0; i < Mathf.Min(bestHand1.Count, bestHand2.Count); i++)
        {
            int cardValue1 = GetCardValue(bestHand1[i].rank);
            int cardValue2 = GetCardValue(bestHand2[i].rank);

            if (cardValue1 > cardValue2) return 1;  // Mano 1 es mejor
            if (cardValue1 < cardValue2) return -1; // Mano 2 es mejor
        }

        // Si siguen empatadas, comparar las cartas restantes (kickers)
        var remainingCards1 = hand1.Except(bestHand1).OrderByDescending(card => GetCardValue(card.rank)).ToList();
        var remainingCards2 = hand2.Except(bestHand2).OrderByDescending(card => GetCardValue(card.rank)).ToList();

        for (int i = 0; i < Mathf.Min(remainingCards1.Count, remainingCards2.Count); i++)
        {
            int kickerValue1 = GetCardValue(remainingCards1[i].rank);
            int kickerValue2 = GetCardValue(remainingCards2[i].rank);

            if (kickerValue1 > kickerValue2) return 1;  // Mano 1 es mejor
            if (kickerValue1 < kickerValue2) return -1; // Mano 2 es mejor
        }

        // Si todas las cartas son iguales, es un empate
        return 0;
    }

    // Métodos auxiliares para evaluar manos
    private int GetCardValue(string rank)
    {
        // Asignar valores a cada rango
        switch (rank)
        {
            case "2": return 2;
            case "3": return 3;
            case "4": return 4;
            case "5": return 5;
            case "6": return 6;
            case "7": return 7;
            case "8": return 8;
            case "9": return 9;
            case "10": return 10;
            case "Jack": return 11;
            case "Queen": return 12;
            case "King": return 13;
            case "Ace": return 14;
            default: return 0; // Valor inválido
        }
    }

    private bool IsRoyalFlush(List<Card> cards)
    {
        return IsStraightFlush(cards) && cards.Any(card => card.rank == "Ace");
    }

    private bool IsStraightFlush(List<Card> cards)
    {
        return IsFlush(cards) && IsStraight(cards);
    }

    private bool IsFourOfAKind(List<Card> cards)
    {
        return cards.GroupBy(card => card.rank).Any(group => group.Count() == 4);
    }

    private bool IsFullHouse(List<Card> cards)
    {
        var groups = cards.GroupBy(card => card.rank);
        return groups.Any(group => group.Count() == 3) && groups.Any(group => group.Count() == 2);
    }

    private bool IsFlush(List<Card> cards)
    {
        return cards.GroupBy(card => card.suit).Any(group => group.Count() >= 5);
    }

    private bool IsStraight(List<Card> cards)
    {
        var orderedRanks = cards.Select(card => GetCardValue(card.rank)).Distinct().OrderBy(value => value).ToList();
        for (int i = 0; i <= orderedRanks.Count - 5; i++)
        {
            if (orderedRanks[i + 4] - orderedRanks[i] == 4) return true;
        }
        return false;
    }

    private bool IsThreeOfAKind(List<Card> cards)
    {
        return cards.GroupBy(card => card.rank).Any(group => group.Count() == 3);
    }

    private bool IsTwoPair(List<Card> cards)
    {
        return cards.GroupBy(card => card.rank).Count(group => group.Count() == 2) >= 2;
    }

    private bool IsOnePair(List<Card> cards)
    {
        return cards.GroupBy(card => card.rank).Any(group => group.Count() == 2);
    }

    private List<Card> GetBestRoyalFlush(List<Card> cards)
    {
        return GetBestStraightFlush(cards)?.Where(card => GetCardValue(card.rank) >= 10).ToList();
    }

    private List<Card> GetBestStraightFlush(List<Card> cards)
    {
        var flushCards = GetBestFlush(cards);
        return flushCards != null ? GetBestStraight(flushCards) : null;
    }

    private List<Card> GetBestFourOfAKind(List<Card> cards)
    {
        var groups = cards.GroupBy(card => card.rank).Where(group => group.Count() == 4);
        if (!groups.Any()) return null;

        var fourOfAKind = groups.First().ToList();
        var kicker = cards.Except(fourOfAKind).OrderByDescending(card => GetCardValue(card.rank)).First();

        return fourOfAKind.Concat(new List<Card> { kicker }).ToList();
    }

    private List<Card> GetBestFullHouse(List<Card> cards)
    {
        var groups = cards.GroupBy(card => card.rank).OrderByDescending(group => group.Count())
                          .ThenByDescending(group => GetCardValue(group.Key));
        var threeOfAKind = groups.FirstOrDefault(group => group.Count() >= 3)?.Take(3).ToList();
        var pair = groups.FirstOrDefault(group => group.Count() >= 2 && group.Key != threeOfAKind?[0].rank)?.Take(2).ToList();

        return threeOfAKind != null && pair != null ? threeOfAKind.Concat(pair).ToList() : null;
    }

    private List<Card> GetBestFlush(List<Card> cards)
    {
        var suitedCards = cards.GroupBy(card => card.suit)
                               .FirstOrDefault(group => group.Count() >= 5)?
                               .OrderByDescending(card => GetCardValue(card.rank))
                               .Take(5).ToList();
        return suitedCards;
    }

    private List<Card> GetBestStraight(List<Card> cards)
    {
        var distinctCards = cards.OrderByDescending(card => GetCardValue(card.rank))
                                 .GroupBy(card => GetCardValue(card.rank))
                                 .Select(group => group.First())
                                 .ToList();

        for (int i = 0; i <= distinctCards.Count - 5; i++)
        {
            var possibleStraight = distinctCards.Skip(i).Take(5).ToList();
            if (IsConsecutive(possibleStraight))
            {
                return possibleStraight;
            }
        }

        // Escalera especial: A-2-3-4-5
        var aceLowStraight = new[] { "Ace", "2", "3", "4", "5" };
        if (distinctCards.Select(card => card.rank).Intersect(aceLowStraight).Count() == 5)
        {
            return distinctCards.Where(card => aceLowStraight.Contains(card.rank))
                                .OrderBy(card => GetCardValue(card.rank)).ToList();
        }

        return null;
    }

    private bool IsConsecutive(List<Card> cards)
    {
        for (int i = 0; i < cards.Count - 1; i++)
        {
            if (GetCardValue(cards[i].rank) - GetCardValue(cards[i + 1].rank) != 1)
            {
                return false;
            }
        }
        return true;
    }

    private List<Card> GetBestThreeOfAKind(List<Card> cards)
    {
        var groups = cards.GroupBy(card => card.rank).Where(group => group.Count() == 3);
        if (!groups.Any()) return null;

        var threeOfAKind = groups.First().ToList();
        var kickers = cards.Except(threeOfAKind).OrderByDescending(card => GetCardValue(card.rank)).Take(2);

        return threeOfAKind.Concat(kickers).ToList();
    }

    private List<Card> GetBestTwoPair(List<Card> cards)
    {
        var groups = cards.GroupBy(card => card.rank).Where(group => group.Count() >= 2)
                          .OrderByDescending(group => GetCardValue(group.Key));
        if (groups.Count() < 2) return null;

        var twoPairs = groups.Take(2).SelectMany(group => group.Take(2)).ToList();
        var kicker = cards.Except(twoPairs).OrderByDescending(card => GetCardValue(card.rank)).First();

        return twoPairs.Concat(new List<Card> { kicker }).ToList();
    }

    private List<Card> GetBestOnePair(List<Card> cards)
    {
        var groups = cards.GroupBy(card => card.rank).Where(group => group.Count() >= 2);
        if (!groups.Any()) return null;

        var pair = groups.First().Take(2).ToList();
        var kickers = cards.Except(pair).OrderByDescending(card => GetCardValue(card.rank)).Take(3);
        var bestPair = pair.Concat(kickers).ToList(); 

        return pair.Concat(kickers).ToList();
    }

    private List<Card> GetBestHighCard(List<Card> cards)
    {
        return cards.OrderByDescending(card => GetCardValue(card.rank)).Take(5).ToList();
    }

    public void StartNewHand()
    {
        CreateDeck(); // Crear las cartas en memoria
        ShuffleDeck(); // Barajar el mazo
        InstantiateDeck(); // Instanciar las cartas barajadas   
        SetInitialRoles(); // Establecer los roles iniciales de los jugadores
        DealInitialCards(); // Repartir las cartas iniciales
    }

}


