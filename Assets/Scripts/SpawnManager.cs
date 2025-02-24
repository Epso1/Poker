using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;

public class SpawnManager : MonoBehaviour
{
    public static SpawnManager Instance;
    public Transform[] spawnPoints;
    public Button startGameButton; // Referencia al botón de inicio

    private int playersSpawned = 0;

    private void Awake()
    {
        Instance = this;
        startGameButton.gameObject.SetActive(false); // Desactivar el botón al inicio
    }

    public Transform GetSpawnPoint(ulong clientId)
    {
        int index = (int)(clientId % (ulong)spawnPoints.Length);
        return spawnPoints[index];
    }

    public void PlayerSpawned()
    {
        playersSpawned++;

        if (playersSpawned == spawnPoints.Length)
        {
            startGameButton.gameObject.SetActive(true); // Activar botón cuando todos los jugadores estén listos
        }
    }
}

