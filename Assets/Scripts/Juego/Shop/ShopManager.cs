using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShopManager : MonoBehaviour
{
    public static ShopManager Instancia;
    
    [Header("Referencias UI")]
    [SerializeField] private GameObject panelShopUI;
    [SerializeField] private Transform content;
    [SerializeField] private GameObject botonPrefab;
    [SerializeField] private Text moneyText;


    private readonly List<Button> botonesActuales = new();
    private EquipmentManager equipmentManager;

    private void Awake()
    {
        Instancia = this;
        equipmentManager = FindObjectOfType<EquipmentManager>();
    }

    public void AbrirUI(GameObject jugadorGO)
    {
        if (equipmentManager == null)
        {
            Debug.LogError("ShopManager ▸ EquipmentManager no encontrado.");
            return;
        }

        RellenarUIPara(jugadorGO);
        panelShopUI.SetActive(true);
    }

    public void CerrarUI() => panelShopUI.SetActive(false);
    
    private void RellenarUIPara(GameObject jugadorGO)
    {
        Limpiar();

        var handler = jugadorGO.GetComponent<PlayerUpgradeHandler>();
        if (handler == null)
        {
            Debug.LogError("ShopManager -62 El jugador carece de PlayerUpgradeHandler.");
            return;
        }
        
        PlayerType tipo = handler is PoliceUpgradeHandler ? PlayerType.Police : PlayerType.Looter;
        List<UpgradeData> upgrades = equipmentManager.ObtenerUpgradesPara(tipo);

        foreach (var upg in upgrades)
        {
            GameObject btnObj = Instantiate(botonPrefab, content);
            var boton = btnObj.GetComponent<Button>();
            botonesActuales.Add(boton);

            Transform icon = btnObj.transform.GetChild(0);
            Transform txtHolder = btnObj.transform.GetChild(1);
            Text nameTxt = txtHolder.GetChild(0).GetComponent<Text>();
            Text priceTxt = txtHolder.GetChild(1).GetComponent<Text>();

            icon.GetComponent<Image>().sprite = upg.icon;
            nameTxt.text = upg.upgradeName;
            priceTxt.text = $"Coste: ${upg.price}";

            bool yaEquipada = handler.GetEquipado(upg.category) == upg;
            boton.interactable = !yaEquipada;

            boton.onClick.AddListener(() =>
            {
                var handler = jugadorGO.GetComponent<PlayerUpgradeHandler>();
                if (handler == null) 
                { 
                    Debug.LogError("Falta PlayerUpgradeHandler");
                    return; 
                }
                
                if (!handler.TieneFondos(upg.price))
                {
                    Debug.Log($"[SHOP] {jugadorGO.name} — sin dinero para {upg.upgradeName}");
                    return;
                }
                
                UpgradeData anterior = handler.GetEquipado(upg.category);

                handler.Cobrar(upg.price);
                handler.AplicarUpgrade(upg);
                boton.interactable = false;

                if (anterior != null && anterior != upg)
                {
                    foreach (var b in botonesActuales)
                    {
                        if (b.name.Contains(anterior.upgradeName))
                        {
                            b.interactable = true;
                            break;
                        }
                    }
                }
                
                ActualizarDinero(handler, tipo);
                
                Debug.Log($"[SHOP] {jugadorGO.name} compró {upg.upgradeName}");
            });
        }

        ActualizarDinero(handler, tipo);

        Debug.Log($"ShopManager -> RellenarUI — botones creados: {botonesActuales.Count}");
    }
    
    private void Limpiar()
    {
        foreach (var b in botonesActuales)
        {
            if (b != null)
            {
                Destroy(b.gameObject);
            }
        }
        botonesActuales.Clear();
    }

    private void ActualizarDinero(PlayerUpgradeHandler h, PlayerType tipo)
    {
        int monto = tipo == PlayerType.Police
                    ? ((PoliceUpgradeHandler)h).Dinero
                    : ((LooterUpgradeHandler)h).Loot;
        moneyText.text = $"Dinero actual: ${monto}";
    }
}
