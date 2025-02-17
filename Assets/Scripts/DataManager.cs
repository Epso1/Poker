using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataManager : MonoBehaviour
{
    public static DataManager Instance;
    public List<PlayerData> connectedPlayers = new List<PlayerData>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

   
}

[System.Serializable]
public class PlayerData
{
    public string playerName;
    public string playerID;

    public PlayerData(string playerName, string playerID)
    {
        this.playerName = playerName;
        this.playerID = playerID;
    }

    public PlayerData()
    {
       
    }
}
