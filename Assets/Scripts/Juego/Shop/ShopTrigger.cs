using UnityEngine;
using Photon.Pun;

public class ShopTrigger : MonoBehaviourPunCallbacks
{
    [SerializeField] private GameObject textoInteractUI;
    private bool dentro = false;
    private GameObject jugadorLocal;

    private bool tiendaAbierta = false;

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
        tiendaAbierta = false;
        jugadorLocal = null;
        textoInteractUI.SetActive(false);
        ShopManager.Instancia.CerrarUI();
    }
    private void Update()
    {
        if (dentro && Input.GetKeyDown(KeyCode.E))
        {
            if (tiendaAbierta)
            {
                textoInteractUI.SetActive(true);
                ShopManager.Instancia.CerrarUI();
                tiendaAbierta = false;
            }
            else
            {
                textoInteractUI.SetActive(false);
                ShopManager.Instancia.AbrirUI(jugadorLocal);
                tiendaAbierta = true;
            }
        }
    }
}
