using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;

public class GameInitializer : MonoBehaviourPunCallbacks
{
    [Header("Prefabs por rol")]
    [SerializeField] private GameObject prefabPolicia;
    [SerializeField] private GameObject[] prefabsLadrones;

    [Header("Puntos de aparición")]
    [SerializeField] private Transform spawnPolicia;
    [SerializeField] private Transform[] spawnsLadrones;

    protected new void OnEnable()
    {
        StartCoroutine(EsperarDatosYSpawnear());
    }

    private IEnumerator EsperarDatosYSpawnear()
    {
        int actorID = PhotonNetwork.LocalPlayer.ActorNumber;
        
        yield return new WaitUntil(() =>
            PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey($"slot_{actorID}")
        );

        int slot = (int)PhotonNetwork.CurrentRoom.CustomProperties[$"slot_{actorID}"];
        Debug.Log("Mi slot cargado desde RoomProperties es: " + slot);

        switch (slot)
        {
            case -1:
                PhotonNetwork.Instantiate(prefabPolicia.name, spawnPolicia.position, Quaternion.identity);
                break;
            case 0:
            case 1:
            case 2:
                PhotonNetwork.Instantiate(prefabsLadrones[slot].name, spawnsLadrones[slot].position, Quaternion.identity);
                break;
            default:
                Debug.LogWarning("Slot inválido recibido.");
                break;
        }
    }
}