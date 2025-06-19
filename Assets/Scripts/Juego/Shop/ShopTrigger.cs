using UnityEngine;
using Photon.Pun;

public class ShopTrigger : MonoBehaviourPunCallbacks
{
    [SerializeField] private string teclaAbrir = "e";
    [SerializeField] private GameObject textoInteractUI;
    private bool dentro = false;
    private GameObject jugadorLocal;

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (!col.TryGetComponent<PhotonView>(out var pv) || !pv.IsMine) return;
        dentro = true;
        jugadorLocal = col.gameObject;
        textoInteractUI.SetActive(true);
    }
    private void OnTriggerExit2D(Collider2D col)
    {
        if (!col.TryGetComponent<PhotonView>(out var pv) || !pv.IsMine) return;
        dentro = false;
        jugadorLocal = null;
        textoInteractUI.SetActive(false);
        ShopManager.Instancia.CerrarUI();
    }
    private void Update()
    {
        if (dentro && Input.GetKeyDown(teclaAbrir))
        {
            textoInteractUI.SetActive(false);
            ShopManager.Instancia.AbrirUI(jugadorLocal);
        }
    }
}
