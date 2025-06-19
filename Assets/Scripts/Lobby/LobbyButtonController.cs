using UnityEngine;
using Photon.Pun;
using UnityEngine.SceneManagement;

public class LobbyButtonController : MonoBehaviour
{
    public void VolverAlMenuPrincipal()
    {
        Debug.Log("Volviendo al menú principal...");
        PhotonNetwork.LeaveRoom();
        StartCoroutine(CargarEscenaTrasSalida("Menu Principal"));
    }
    
    public void SalirDelJuego()
    {
        Debug.Log("Saliendo del juego...");
        PhotonNetwork.LeaveRoom();
        Application.Quit();
    }

    public void ComenzarPartida()
    {
        Debug.Log("Solicitud para comenzar la partida recibida.");

        if (PhotonNetwork.IsMasterClient)
        {
            FindObjectOfType<LobbySlotManager>().GuardarSlotsEnRoomProperties();
            PhotonNetwork.LoadLevel("Cargando");
        }
        else
        {
            Debug.Log("Soy cliente, enviando solicitud al Master para iniciar la partida.");
            PhotonView pv = FindObjectOfType<LobbySlotManager>().GetComponent<PhotonView>();
            pv.RPC("RPC_IniciarPartidaDesdeCliente", RpcTarget.MasterClient);
        }
    }

    private System.Collections.IEnumerator CargarEscenaTrasSalida(string nombreEscena)
    {
        while (PhotonNetwork.InRoom)
        {
            yield return null;
        }

        SceneManager.LoadScene(nombreEscena);
    }
}