using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class ShopManager : MonoBehaviour
{
    public static ShopManager Instancia;

    [Header("Referencias UI")]
    [SerializeField] private GameObject panelShopUI;
    [SerializeField] private Transform content;
    [SerializeField] private GameObject botonPrefab;

    private List<Button> botonesActuales = new();

    private void Awake() => Instancia = this;

    public void AbrirUI(GameObject jugadorGO)
    {
        RellenarUIPara(jugadorGO);
        panelShopUI.SetActive(true);
    }
    public void CerrarUI() => panelShopUI.SetActive(false);

    private void RellenarUIPara(GameObject jugadorGO)
    {
        Limpiar();
        PlayerType tipo = jugadorGO.GetComponent<PoliceController>() ? PlayerType.Police : PlayerType.Looter;
        List<UpgradeData> lista = FindObjectOfType<EquipmentManager>().ObtenerUpgradesPara(tipo);

        foreach (var upg in lista)
        {
            GameObject btnObj = Instantiate(botonPrefab, content);
            botonesActuales.Add(btnObj.GetComponent<Button>());

            btnObj.transform.GetChild(0).GetComponent<Image>().sprite = upg.icon;
            btnObj.transform.GetChild(1).GetComponent<Text>().text =
                $"{upg.upgradeName}\n${upg.price}";

            btnObj.GetComponent<Button>().onClick.AddListener(() =>
            {
                IShopClient cliente = jugadorGO.GetComponent<IShopClient>();
                if (cliente == null) { Debug.LogError("El jugador no implementa IShopClient"); return; }

                if (!cliente.Pagar(upg.price))
                {
                    Debug.Log("No tienes dinero suficiente");
                    return;
                }

                upg.ApplyUpgrade(jugadorGO);
                cliente.AñadirUpgrade(upg);
                btnObj.GetComponent<Button>().interactable = false;
            });
        }
    }
    private void Limpiar()
    {
        foreach (var b in botonesActuales) Destroy(b.gameObject);
        botonesActuales.Clear();
    }
}
