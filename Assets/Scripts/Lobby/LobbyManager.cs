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
            PhotonNetwork.ConnectUsingSettings();
            Debug.Log("Conectando a Photon...");
        }
        else
        {
            JoinOrCreateRoom();
        }
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("Conectado a Photon Master Server.");
        JoinOrCreateRoom();
    }

    private void JoinOrCreateRoom()
    {
        RoomOptions roomOptions = new RoomOptions
        {
            MaxPlayers = 4,
            PlayerTtl = 0,
            EmptyRoomTtl = 0
        };
        string roomName = "SalaLobbyPrincipal";

        PhotonNetwork.JoinOrCreateRoom(roomName, roomOptions, TypedLobby.Default);
        Debug.Log("Intentando unirse o crear la sala...");

        PlayerPrefs.SetString("LastRoomName", roomName);
        PlayerPrefs.Save();
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