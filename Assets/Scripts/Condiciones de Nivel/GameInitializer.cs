using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using Photon.Pun.Demo.PunBasics;

public class GameInitializer : MonoBehaviourPunCallbacks
{
    [Header("Prefabs por rol")]
    [SerializeField] private GameObject prefabPolicia;
    [SerializeField] private GameObject[] prefabsLadrones;

    [Header("Puntos de aparición")]
    [SerializeField] private Transform spawnPolicia;
    [SerializeField] private Transform[] spawnsLadrones;

    [SerializeField] private Sprite[] spriteLadrones;

    GameManager gameManager;
    protected new void OnEnable()
    {     
        gameManager = GameManager.instance;
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
                GameObject ladron = PhotonNetwork.Instantiate(prefabsLadrones[0].name, spawnsLadrones[slot].position, Quaternion.identity);
                ladron.GetComponent<SpriteRenderer>().sprite = spriteLadrones[slot];
                break;
            default:
                Debug.LogWarning("Slot inválido recibido.");
                break;
        }

        yield return new WaitForSeconds(2);
        if (slot >= -1 && slot <= 2)
        {
            GameManager.instance.connected_player[slot + 1] = true;
            if (slot == -1) GameManager.instance.policeName = PhotonNetwork.LocalPlayer.NickName;
        }
    }
}