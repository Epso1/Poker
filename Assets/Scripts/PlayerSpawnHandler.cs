using Unity.Netcode;
using UnityEngine;
using System;

public class PlayerSpawnHandler : NetworkBehaviour
{
    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            Transform spawnPoint = SpawnManager.Instance.GetSpawnPoint(OwnerClientId);
            transform.position = spawnPoint.position;
            transform.rotation = spawnPoint.rotation;

            string uniqueName = GenerateUniqueName();
            GetComponent<PlayerController>().SetPlayerName(uniqueName);

            SpawnManager.Instance.PlayerSpawned();
        }
    }

    private string GenerateUniqueName()
    {
        return $"Player-{Guid.NewGuid().ToString()}"; // Genera un GUID único
    }
}

