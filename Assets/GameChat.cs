using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;

public class GameChat : MonoBehaviourPunCallbacks
{
    [SerializeField] GameObject msg_pref;
    [SerializeField] Transform chat_pos;

    GameManager gameManager;

    private void Start()
    {
        gameManager = GameManager.instance;
    }


    [PunRPC]
    public void ShowMSG(string msg)
    {
        GameObject new_msg = Instantiate(msg_pref, chat_pos);
        TMP_Text textoTMP = new_msg.GetComponent<TMP_Text>();

        if (textoTMP != null)
        {
            textoTMP.text = msg;
        }
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        Debug.Log("sgañsdjgfoikvnadofijkgnadofgjkn");
        string mensaje = $"El jugador {otherPlayer.NickName} ha abandonado la sala.";
        Debug.Log(mensaje);
        string mensajeTotal = "Total jugadores en sala: " + PhotonNetwork.CurrentRoom.PlayerCount;
        Debug.Log(mensajeTotal);

        photonView.RPC("ShowMSG", RpcTarget.All, mensaje);
        photonView.RPC("ShowMSG", RpcTarget.All, mensajeTotal);

        int actorID = otherPlayer.ActorNumber;
        int slot = (int)PhotonNetwork.CurrentRoom.CustomProperties[$"slot_{actorID}"];
        gameManager.connected_player[slot + 1] = false;
        gameManager.forcePause.Invoke();
    }
}
