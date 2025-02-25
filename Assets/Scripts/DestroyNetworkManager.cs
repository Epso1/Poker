using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class DestroyNetworkManager : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.Shutdown();
            GameObject nm = GameObject.FindGameObjectWithTag("NetworkManager");
            Destroy(nm);
        }       
    }
}
