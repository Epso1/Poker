using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;

public class SpawnManager : MonoBehaviour
{
    public static SpawnManager Instance;
    public Transform[] spawnPoints;
    public Button startGameButton; // Referencia al bot�n de inicio

    private int playersSpawned = 0;

    private void Awake()
    {
        Instance = this;
        startGameButton.gameObject.SetActive(false); // Desactivar el bot�n al inicio
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
            startGameButton.gameObject.SetActive(true); // Activar bot�n cuando todos los jugadores est�n listos
        }
    }
}

