using UnityEngine;

public class CardGenerator : MonoBehaviour
{
    public GameObject cardPrefab; // Prefab del objeto carta
    public Sprite[] cardSprites; // Array de sprites para las texturas de las cartas

    void Start()
    {
        GenerateDeck();
    }

    void GenerateDeck()
    {
        float xOffset = 2.0f; // Separación entre cartas
        float zOffset = 2.5f; // Separación en filas
        int index = 0;

        for (int row = 0; row < 4; row++) // 4 filas (palos)
        {
            for (int col = 0; col < 13; col++) // 13 columnas (valores)
            {
                if (index >= cardSprites.Length) return;

                // Instancia una nueva carta
                GameObject newCard = Instantiate(cardPrefab, new Vector3(col * xOffset, 0, row * zOffset), Quaternion.identity);

                // Cambia la textura de la carta
                SpriteRenderer renderer = newCard.GetComponent<SpriteRenderer>();
                if (renderer != null)
                {
                    renderer.sprite = cardSprites[index];
                }

                index++;
            }
        }
    }
}
