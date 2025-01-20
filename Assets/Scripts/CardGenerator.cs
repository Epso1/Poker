using UnityEngine;
using System.Collections.Generic;

public class CardGenerator : MonoBehaviour
{
    public GameObject cardPrefab; // Prefab del objeto carta
    public Sprite[] cardSprites; // Array de sprites para las texturas de las cartas
    public Sprite cardBack; // Sprite para el reverso de la carta
    private List<GameObject> deck = new List<GameObject>(); // Lista de cartas generadas

    void Start()
    {
        CreateDeck();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.KeypadMultiply))
        {
            PrintDeck();
        }
    }

    void CreateDeck()
    {
        for (int i = 0; i < cardSprites.Length; i++)
        {
            // Crea una nueva instancia del prefab para cada carta
            GameObject newCard = Instantiate(cardPrefab);

            // Cambia el sprite frontal y trasero de la carta
            SpriteRenderer[] renderers = newCard.GetComponentsInChildren<SpriteRenderer>();
            foreach (SpriteRenderer renderer in renderers)
            {
                if (renderer.gameObject.name.Equals("Front"))
                {
                    renderer.sprite = cardSprites[i];
                }
                else if (renderer.gameObject.name.Equals("Back"))
                {
                    renderer.sprite = cardBack;
                }
            }
            newCard.name = cardSprites[i].name; // Renombra la carta para identificarla

            Debug.Log($"Card_{i} created with sprite: {cardSprites[i].name}.");
            // Almacena la carta en la lista
            deck.Add(newCard);

            // Desactiva la carta para que no sea visible hasta que se use en PrintDeck
            newCard.SetActive(false);
        }
        ShuffleDeck();
    }

    // Barajar las cartas
    void ShuffleDeck()
    {
        for (int i = 0; i < deck.Count; i++)
        {
            int randomIndex = Random.Range(0, deck.Count);
            // Intercambia las posiciones de las cartas en la lista
            GameObject temp = deck[i];
            deck[i] = deck[randomIndex];
            deck[randomIndex] = temp;
        }
    }

    void PrintDeck()
    {
        float xOffset = 0.6f; // Separación entre cartas
        float yOffset = 0.75f; // Separación en filas
        Vector3 initialOffset = new Vector3(-3.5f, 1.5f, 0);
        int index = 0;

        for (int row = 0; row < 4; row++) // 4 filas (palos)
        {
            for (int col = 0; col < 13; col++) // 13 columnas (valores)
            {
                if (index < deck.Count)
                {
                    // Instancia una nueva carta a partir del prefab almacenado
                    GameObject cardInstance = Instantiate(deck[index], initialOffset + new Vector3(col * xOffset, -row * yOffset, 0), Quaternion.identity);

                    // Activa la carta por si estaba desactivada
                    cardInstance.SetActive(true);

                    // Incrementa el índice
                    index++;
                }
            }
        }

        Debug.Log("Deck count: " + deck.Count);
    }
}
