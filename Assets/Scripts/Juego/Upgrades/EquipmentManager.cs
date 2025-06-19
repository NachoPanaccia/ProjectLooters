using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class EquipmentManager : MonoBehaviour
{
    [Header("Todas las mejoras disponibles")]
    [SerializeField] private List<UpgradeData> allUpgrades;

    private Dictionary<PlayerType, List<UpgradeData>> upgradesPorJugador;

    private void Start()
    {
        Inicializar();
    }

    public void Inicializar()
    {
        StartManager startManager = FindObjectOfType<StartManager>();
        if (startManager == null)
        {
            Debug.LogError("EquipmentManager: No se encontró StartManager en la escena.");
            return;
        }

        if (allUpgrades == null || allUpgrades.Count == 0)
        {
            startManager.ReportarError("Equipment", "No se cargaron mejoras en el EquipmentManager.");
            return;
        }

        upgradesPorJugador = allUpgrades
            .GroupBy(upg => upg.owner)
            .ToDictionary(grp => grp.Key, grp => grp.ToList());

        Debug.Log("EquipmentManager: Se cargaron " + allUpgrades.Count + " mejoras.");
        startManager.ConfirmarValidacion("Equipment");
    }

    public List<UpgradeData> ObtenerUpgradesPara(PlayerType tipo)
    {
        return upgradesPorJugador.TryGetValue(tipo, out var list) ? list : new List<UpgradeData>();
    }

    public List<UpgradeData> ObtenerPorCategoria(PlayerType tipo, UpgradeCategory cat)
    {
        return ObtenerUpgradesPara(tipo).Where(upg => upg.category == cat).ToList();
    }
}
