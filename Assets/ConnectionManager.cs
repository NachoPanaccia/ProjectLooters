using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConnectionManager : MonoBehaviourPunCallbacks
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        int actorID = PhotonNetwork.LocalPlayer.ActorNumber;

        if ((int)PhotonNetwork.CurrentRoom.CustomProperties[$"slot_{actorID}"] != -1) return;

        photonView.RPC("UnpauseGame", RpcTarget.All);
    }
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        Debug.Log("Jugador desconectado: " + otherPlayer.NickName);

        int actorID = PhotonNetwork.LocalPlayer.ActorNumber;

        if ((int)PhotonNetwork.CurrentRoom.CustomProperties[$"slot_{actorID}"] != -1) return;

        photonView.RPC("PauseGame", RpcTarget.All);
        //PAUSA
    }

    [PunRPC]
    public void PauseGame()
    {
        Time.timeScale = 0;
    }

    [PunRPC]
    public void UnpauseGame()
    {
        Time.timeScale = 1;
    }
}
