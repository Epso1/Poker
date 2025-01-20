using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Player : MonoBehaviour
{
    [SerializeField] public string playerName;
    [SerializeField] public Transform card1Position;
    [SerializeField] public Transform card2Position;
    [SerializeField] TextMeshPro playerNameText;    
    public List<DeckManager.Card> hand = new List<DeckManager.Card>();

    private void Start()
    {
        playerNameText.text = playerName;
    }

}
