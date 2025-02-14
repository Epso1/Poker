using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerContainer : MonoBehaviour
{
    [SerializeField] private TMP_Text playerName;
    [SerializeField] private GameObject botonBaneo;
    // Start is called before the first frame update
    public void BaneoActivo(bool activar)
    {
        botonBaneo.SetActive(activar);
    }
    public Button GetBotonBaneo()
    {
        return botonBaneo.GetComponent<Button>();
    }

    public void SetPlayerName(string name)
    {
        playerName.text = name;
    }
}
