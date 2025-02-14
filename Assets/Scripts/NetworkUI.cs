using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class NetworkUI : MonoBehaviour
{
    [SerializeField] private Button botonServidor;
    [SerializeField] private Button botonHost;
    [SerializeField] private Button botonCliente;
    [SerializeField] private Button botonStopServer;
    // Start is called before the first frame update
    [SerializeField]
    private void Awake()
    {
        botonServidor.onClick.AddListener(() => { NetworkManager.Singleton.StartServer(); } );
        botonHost.onClick.AddListener(() => { NetworkManager.Singleton.StartHost(); } );
        botonCliente.onClick.AddListener(() => { NetworkManager.Singleton.StartClient(); } );
        botonStopServer.onClick.AddListener(() => { NetworkManager.Singleton.Shutdown(); } );
    }
}
