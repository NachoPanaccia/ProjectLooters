using System.Collections;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;


public class ReconnectManager : MonoBehaviourPunCallbacks
{
    [SerializeField] private int maxAttempts = 5;
    [SerializeField] private float retryDelay = 2f;

    private int attempt = 0;
    private bool isTrying = false;
    private string lastRoomName;

    private void Awake()
    {
        if (FindObjectsOfType<ReconnectManager>().Length > 1)
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        if (isTrying) return;

        lastRoomName = PhotonNetwork.CurrentRoom?.Name;
        Debug.Log($"[RECONNECT] Desconectado: {cause}. Sala = {lastRoomName}");

        StartCoroutine(TryReconnect());
    }

    public override void OnJoinedRoom()
    {
        if (isTrying)
            Debug.Log("[RECONNECT] ¡Reconexión exitosa! Reincorporado a la sala.");

        isTrying = false;
        attempt = 0;
        StopAllCoroutines();
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.LogWarning($"[RECONNECT] JoinRoomFailed ({returnCode}) {message}");
    }

    private IEnumerator TryReconnect()
    {
        isTrying = true;
        attempt = 0;

        while (attempt < maxAttempts)
        {
            attempt++;
            Debug.Log($"[RECONNECT] Intento {attempt}/{maxAttempts}…");

            if (PhotonNetwork.ReconnectAndRejoin())
                yield break;

            yield return new WaitForSeconds(retryDelay);

            if (!PhotonNetwork.IsConnected)
            {
                PhotonNetwork.ConnectUsingSettings();
            }

            yield return new WaitUntil(() => PhotonNetwork.IsConnectedAndReady);

            if (PhotonNetwork.IsConnected && !string.IsNullOrEmpty(lastRoomName))
             {
                    if (PhotonNetwork.RejoinRoom(lastRoomName))
                    yield break;
                
                    if (PhotonNetwork.JoinRoom(lastRoomName))
                    yield break;
            }

            yield return new WaitForSeconds(retryDelay);
        }

        Debug.LogWarning("[RECONNECT] Falló la reconexión. Volviendo al menú.");
        UnityEngine.SceneManagement.SceneManager.LoadScene("Menu Principal");
    }
}