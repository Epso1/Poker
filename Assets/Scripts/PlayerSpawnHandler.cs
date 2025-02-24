using Unity.Netcode;
using UnityEngine;

public class PlayerSpawnHandler : NetworkBehaviour
{
    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            Transform spawnPoint = SpawnManager.Instance.GetSpawnPoint(OwnerClientId);
            transform.position = spawnPoint.position;
            transform.rotation = spawnPoint.rotation;

            SpawnManager.Instance.PlayerSpawned(); // Notificar al SpawnManager
        }
    }
}
