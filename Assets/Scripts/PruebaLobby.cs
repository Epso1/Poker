using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using TMPro;

public class PruebaLobby : MonoBehaviour
{
    [Header("Lobby")]
    [SerializeField] private string nombreLobby;
    [SerializeField] private int maxJugadores;

    [Header("UI")]
    [SerializeField] private GameObject panelLobby;
    [SerializeField] private GameObject panelListaPlayers;
    [SerializeField] private GameObject panelPlayer;
    [SerializeField] private TMP_Text idPlayer_text;
    [SerializeField] private TMP_Text nombrePlayer_text;

    private Lobby hostLobby; //variable para guardar el lobby que crearemos (Solo el host tiene acceso a esta variable)

    private Lobby playerLobby; //variable para guardar el lobby al que nos uniremos 

    private float lobbyTimer; //variable para controlar el tiempo de inactividad del lobby

    private string nombrePlayer; //variable para guardar el nombre del player


    private async void Start() //usamos async para poder usar await
    {
        panelLobby.SetActive(false); //Inicializamos el panel de lobby

        InitializationOptions options = new InitializationOptions();
        nombrePlayer = "Player_" + Random.Range(10, 99);
        options.SetProfile(nombrePlayer);
        nombrePlayer_text.text = "Player Name: " + nombrePlayer; //Mostramos el nombre del player en pantalla

        await UnityServices.InitializeAsync(options); //NOPTA IMPORTANTE: Se supone que asi podemos tener varios players en la misma máquina.

        //await UnityServices.InitializeAsync(); 

        AuthenticationService.Instance.SignedIn += () =>
        {
            Debug.Log("Se ha logeado el player nº: " + AuthenticationService.Instance.PlayerId); //Cada vez que se logee un usuario, se ejecutará este código
            idPlayer_text.text = "Player ID: " + AuthenticationService.Instance.PlayerId; //Mostramos el id del player en pantalla
        };

        await AuthenticationService.Instance.SignInAnonymouslyAsync();


    }

    private async void Update()
    {
        if (hostLobby != null) //si hemos creado un lobby
        {
            lobbyTimer -= Time.deltaTime;

            if (lobbyTimer < 0f)
            {
                float timerMax = 15f;
                lobbyTimer = timerMax; //reseteamos el timer

                await LobbyService.Instance.SendHeartbeatPingAsync(hostLobby.Id); //enviamos un ping para mantener el lobby activo

                Debug.Log("Espabilando el lobby para que no se cierre");

            }
        }
    }

    //===================================================================================================== CREAR LOBBY
    public async void CrearLobby()
    {
        try
        {

            // Consultamos si ya existe un lobby
            QueryResponse queryResponse = await Lobbies.Instance.QueryLobbiesAsync(); //Esto nos devolverá todos los lobbies activos

            if (queryResponse.Results.Count > 0) // Si hay al menos un lobby, no permitimos crear otro
            {
                Debug.Log("Ya existe un lobby activo. No puedes crear otro.");
                return;
            }

            CreateLobbyOptions createLobbyOptions = new CreateLobbyOptions { Player = GetPlayer() }; //CreateLobbyOptions es una clase que nos permite configurar el lobby que vamos a crear

            Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(nombreLobby, maxJugadores, createLobbyOptions); //Creamos el lobby con su nombre, su maximo de jugadores y el player que lo crea

            if (lobby.HostId == AuthenticationService.Instance.PlayerId) // Si el player actual es el host
            {
                hostLobby = lobby; //Esto lo usamos en el update para mantener el lobby activo
            }

            playerLobby = lobby;

            Debug.Log("Lobby creado con éxito: " + lobby.Name + " para " + lobby.MaxPlayers + " jugadores.");

            // Mostramos el panel de lobby
            panelLobby.SetActive(true);
            MostrarPlayers();
        }
        catch (LobbyServiceException e)
        {
            Debug.Log("Error al crear el lobby: " + e);
        }
    }
    //===================================================================================================== LISTAR LOBBIES  
    public async void ListaLobbies()
    {
        try
        {
            QueryResponse queryresponse = await Lobbies.Instance.QueryLobbiesAsync();

            Debug.Log("Encontrados " + queryresponse.Results.Count + " lobiies. ");

            foreach (Lobby l in queryresponse.Results)
            {
                Debug.Log("Lobby: " + l.Name + " con " + l.MaxPlayers + " jugadores.");
            }
        }
        catch (LobbyServiceException e)
        {
            Debug.Log("Error al listar los lobbies: " + e);
        }
    }
    //===================================================================================================== ENTRAR EN EL LOBBY
    public async void JoinLobby()
    {
        // Verificar si ya estamos en un lobby antes de intentar unirnos
        if (playerLobby != null)
        {
            Debug.LogWarning("Ya estamos en un lobby: " + playerLobby.Name);
            return;
        }

        try
        {
            JoinLobbyByIdOptions joinLobbyByIdOptions = new JoinLobbyByIdOptions { Player = GetPlayer() }; //GetPlayer() es una función que devuelve un objeto Player con el nombre del player asignado en el start.

            QueryResponse queryresponse = await Lobbies.Instance.QueryLobbiesAsync(); //Buscamos lobbies disponibles
            Lobby joinedLobby = await Lobbies.Instance.JoinLobbyByIdAsync(queryresponse.Results[0].Id, joinLobbyByIdOptions); //Nos unimos al primer lobby que encontremos (que deberia ser el unico si existe)

            Debug.Log("Entrando al lobby con id: " + joinedLobby.Id);

            //ACTUALIZAMOS EL LOBBY CON LA INFORMACIÓN MÁS RECIENTE
            playerLobby = await Lobbies.Instance.GetLobbyAsync(joinedLobby.Id);

            // Mostramos el panel del lobby
            panelLobby.SetActive(true);
            // Mostramos los players actualizados
            MostrarPlayers();
        }
        catch (LobbyServiceException e)
        {
            Debug.Log("Excepcion al entrar al lobby: " + e);
        }
    }
    private Player GetPlayer()
    {
        return new Player
        {
            Data = new Dictionary<string, PlayerDataObject>
                    {
                        { "PlayerName", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, nombrePlayer) }
                    }
        };
    }
    //===================================================================================================== LISTAR PLAYERS
    public void MostrarPlayers()
    {
        if (playerLobby == null) return;//Si no estamos en un lobby no hacemos nada

        foreach (Transform child in panelListaPlayers.transform) // Eliminamos todos los players de la lista antes de actualizarla
        {
            Destroy(child.gameObject);
        }

        bool soyHost = playerLobby.HostId == AuthenticationService.Instance.PlayerId; // Comprobamos si el player actual es el host (para mostrar o no el botón de baneo)

        Debug.Log("Players en el Lobby " + playerLobby.Name + " : " + playerLobby.Players.Count); // Número de players en el lobby

        foreach (Player p in playerLobby.Players) // Refrescamos la lista de players
        {
            Debug.Log("Player id: " + p.Id + " con nombre: " + p.Data["PlayerName"].Value);

            GameObject aux = Instantiate(panelPlayer, panelListaPlayers.transform); // Añadimos una línea a la lista de players

            aux.GetComponent<PlayerContainer>().SetPlayerName(p.Data["PlayerName"].Value); //Añadimos el nombre a la linea

            if (p.Id == playerLobby.HostId)
            {
                aux.GetComponent<PlayerContainer>().BaneoActivo(false); //al host no se le puede banear
            }
            else
            {
                aux.GetComponent<PlayerContainer>().BaneoActivo(soyHost);// Solo mostramos el botón de baneo si el jugador actual es el host

                if (soyHost) // Añadimos el evento al botón
                {
                    aux.GetComponent<PlayerContainer>().GetBotonBaneo().onClick.AddListener(() => BanearPlayer(p.Id));
                }
            }
        }
    }

    //===================================================================================================== SALIR DEL LOBBY
    public async void SalirDelLobby()
    {
        if (playerLobby == null) // No estamos en ningún lobby
        {
            panelLobby.SetActive(false);
            return;
        }

        try
        {
            string playerId = AuthenticationService.Instance.PlayerId;

            // Caso 1: Nos han baneado
            bool baneado = true; // Inicialmente asumimos que hemos sido baneados
            foreach (var player in playerLobby.Players)
            {
                if (player.Id == AuthenticationService.Instance.PlayerId)
                {
                    baneado = false; // Si encontramos nuestro PlayerId, no hemos sido baneados
                    break;
                }
            }

            if (baneado)
            {
                Debug.Log("Hemos sido baneados o expulsados del lobby.");
                playerLobby = null;
                hostLobby = null;
                panelLobby.SetActive(false);
                return; // Salimos si hemos sido baneados
            }

            // Caso 2: Si somos un jugador normal, simplemente salimos
            await LobbyService.Instance.RemovePlayerAsync(playerLobby.Id, playerId);
            Debug.Log("Saliendo del lobby");

            panelLobby.SetActive(false);
            playerLobby = null;

            // Caso 3: Si éramos el host y éramos el último jugador, eliminamos el lobby
            if (hostLobby != null && hostLobby.Players.Count == 1)
            {
                BorrarLobby();
                hostLobby = null;
                return;
            }

            // Caso 4: Si éramos el host pero aún hay jugadores, el nuevo host debe actualizar su referencia
            if (hostLobby != null)
            {
                Debug.Log("El siguiente jugador ahora es el host. Su cliente debe actualizar hostLobby.");
            }
        }
        catch (LobbyServiceException e)
        {
            Debug.Log("Error al salir del lobby: " + e);
        }
    }

    //===================================================================================================== BORRAR LOBBY
    public void BorrarLobby()
    {
        try
        {
            LobbyService.Instance.DeleteLobbyAsync(hostLobby.Id);
            hostLobby = null;//imprescindible para que el espabilador no siga intentando enviar pings
            Debug.Log("Lobby eliminado");
        }
        catch (LobbyServiceException e)
        {
            Debug.Log("Error al borrar el lobby: " + e);
        }

    }
    //===================================================================================================== ACTUALIZAR LA INFORMACIÓN DEL LOBBY
    public async void ActualizarLobby()
    {
        if (playerLobby == null) return;

        try
        {
            // Intentar obtener el lobby actualizado
            playerLobby = await LobbyService.Instance.GetLobbyAsync(playerLobby.Id);

            bool baneado = true; // Inicialmente asumimos que hemos sido baneados
            foreach (var player in playerLobby.Players)
            {
                if (player.Id == AuthenticationService.Instance.PlayerId)
                {
                    baneado = false; // Si encontramos nuestro PlayerId, no hemos sido baneados
                    break;
                }
            }

            if (baneado)
            {
                Debug.Log("Hemos sido baneados o expulsados del lobby.");
                playerLobby = null;
                hostLobby = null;
                panelLobby.SetActive(false);
                return; // Salimos si hemos sido baneados
            }

            // Verificar si este cliente es el nuevo host
            if (playerLobby.HostId == AuthenticationService.Instance.PlayerId)
            {
                hostLobby = playerLobby;
                Debug.Log("Soy el nuevo host del lobby: " + hostLobby.Name);
            }

            MostrarPlayers();
        }
        catch (LobbyServiceException e)
        {
            Debug.Log("Error al actualizar el lobby: " + e);

            // Manejar casos donde el lobby ya no existe o el jugador fue expulsado
            if (e.Reason == LobbyExceptionReason.LobbyNotFound || e.Reason == LobbyExceptionReason.Forbidden)
            {
                Debug.Log("Lobby no encontrado o ya no pertenecemos a él. Cerrando panel...");
                playerLobby = null;
                hostLobby = null;
                panelLobby.SetActive(false);
            }
        }
    }

    //===================================================================================================== BANEO 
    public void BanearPlayer(string playerId)
    {
        try
        {
            LobbyService.Instance.RemovePlayerAsync(hostLobby.Id, playerId);
            Debug.Log("Player con la id " + playerId + " baneado.");

            //Actualizamos la lista de players
            ActualizarLobby();
        }
        catch (LobbyServiceException e)
        {
            Debug.Log("Error al banear el player: " + e);
        }
    }

    //===================================================================================================== OPCIONALES / PRUEBAS

    public void MostrarPlayers(Lobby lobby)
    {
        Debug.Log("Players en el Lobby " + lobby.Name + " :");
        foreach (Player p in lobby.Players)
        {
            Debug.Log("Player id: " + p.Id + " con nombre: " + p.Data["PlayerName"].Value);
        }
    }
    public void SignOut()
    {
        AuthenticationService.Instance.SignOut(true);
    }

}
