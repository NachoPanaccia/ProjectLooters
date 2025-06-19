using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class SlotChangeButton : MonoBehaviour
{
    [SerializeField] private int targetSlot;
    private Button btn;

    private void Awake()
    {
        btn = GetComponent<Button>();
        btn.onClick.AddListener(() => SolicitarCambioDeSlot());
    }

    public void SetInteractable(bool visible)
    {
        gameObject.SetActive(visible);
    }

    private void SolicitarCambioDeSlot()
    {
        PhotonView pv = FindObjectOfType<LobbySlotManager>().GetComponent<PhotonView>();
        pv.RPC("RPC_SolicitarCambioDeSlot", RpcTarget.MasterClient, PhotonNetwork.LocalPlayer.ActorNumber, targetSlot);
    }

    public int GetTargetSlot()
    {
        return targetSlot;
    }
}