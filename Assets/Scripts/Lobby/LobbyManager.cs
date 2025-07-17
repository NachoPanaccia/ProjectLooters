using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class LobbyManager : MonoBehaviourPunCallbacks
{
    private void Awake()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
    }

    private void Start()
    {
        if (!PhotonNetwork.IsConnected)
        {
            string uid = PlayerPrefs.GetString("UID", System.Guid.NewGuid().ToString());
            PlayerPrefs.SetString("UID", uid);

            PhotonNetwork.AuthValues = new AuthenticationValues(uid);

            PhotonNetwork.ConnectUsingSettings();
            Debug.Log($"Conectando a Photon… (UID: {uid})");
        }
        else
        {
            LaunchRoom();
        }
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("Conectado a Photon Master.");
        LaunchRoom();
    }

    private void LaunchRoom()
    {
        string roomName = string.IsNullOrEmpty(RoomLauncher.PendingRoomName)
                        ? "DefaultLobby"
                        : RoomLauncher.PendingRoomName;

        RoomOptions opts = new RoomOptions
        {
            MaxPlayers = 4,
            PlayerTtl = 300_000,
            EmptyRoomTtl = 0
        };

        if (RoomLauncher.ShouldCreate)
        {
            PhotonNetwork.CreateRoom(roomName, opts, TypedLobby.Default);
            Debug.Log($"Creando sala «{roomName}» …");
        }
        else
        {
            PhotonNetwork.JoinRoom(roomName);
            Debug.Log($"Uniéndose a sala «{roomName}» …");
        }
    }

    public override void OnCreateRoomFailed(short code, string message)
    {
        Debug.LogWarning($"Falló CreateRoom: {message}");
        PhotonNetwork.LoadLevel("Menu Principal");
    }

    public override void OnJoinRoomFailed(short code, string message)
    {
        Debug.LogWarning($"Falló JoinRoom: {message}");
        PhotonNetwork.LoadLevel("Menu Principal");
    }

    public override void OnJoinedRoom()
    {
        PlayerPrefs.SetString("LastRoomName", PhotonNetwork.CurrentRoom.Name);
        PlayerPrefs.Save();

        string mensaje = $"{PhotonNetwork.LocalPlayer.NickName}, te has conectado satisfactoriamente a la sala.";
        Debug.Log(mensaje);
        string mensajeTotal = "Total jugadores en sala: " + PhotonNetwork.CurrentRoom.PlayerCount;
        Debug.Log(mensajeTotal);

        LobbyChat chat = FindObjectOfType<LobbyChat>();
        if (chat != null)
        {
            chat.AgregarMensaje(mensaje);
            chat.AgregarMensaje(mensajeTotal);
        }
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        string mensaje = $"El jugador {newPlayer.NickName} se ha unido a la sala {PhotonNetwork.CurrentRoom.Name}.";
        Debug.Log(mensaje);
        string mensajeTotal = "Total jugadores en sala: " + PhotonNetwork.CurrentRoom.PlayerCount;
        Debug.Log(mensajeTotal);

        LobbyChat chat = FindObjectOfType<LobbyChat>();
        if (chat != null)
        {
            chat.AgregarMensaje(mensaje);
            chat.AgregarMensaje(mensajeTotal);
        }
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        string mensaje = $"El jugador {otherPlayer.NickName} ha abandonado la sala.";
        Debug.Log(mensaje);
        string mensajeTotal = "Total jugadores en sala: " + PhotonNetwork.CurrentRoom.PlayerCount;
        Debug.Log(mensajeTotal);

        LobbyChat chat = FindObjectOfType<LobbyChat>();
        if (chat != null)
        {
            chat.AgregarMensaje(mensaje);
            chat.AgregarMensaje(mensajeTotal);
        }
    }
}