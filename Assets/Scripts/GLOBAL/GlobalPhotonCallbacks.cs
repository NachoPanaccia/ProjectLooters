using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.SceneManagement;

public class GlobalPhotonCallbacks : MonoBehaviourPunCallbacks
{
    void Awake()
    {
        // Persistir incluso al cambiar de escena
        DontDestroyOnLoad(gameObject);
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.Log($"<color=orange>Desconectado de Photon</color>: {cause}");
        // Carga la escena de reconexión
        SceneManager.LoadScene("PantallaReconexion");
    }
}