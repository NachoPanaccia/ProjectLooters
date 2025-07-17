
using Photon.Pun;
using System.Collections.Generic;
using UnityEngine;

public abstract class PlayerUpgradeHandler : MonoBehaviourPunCallbacks
{
    /* Este va a ser el inventario de mejoras para todos */
    private readonly HashSet<UpgradeData> upgrades = new();

    /* Listado de mejoras para ver en inspector*/
    [Header("DEBUG – Upgrades aplicadas")]
    [SerializeField] private List<UpgradeData> debugUpgrades = new();

    protected readonly Dictionary<UpgradeCategory, UpgradeData> equipados = new();

    public bool YaEquipado(UpgradeCategory cat) => equipados.ContainsKey(cat);
    public UpgradeData GetEquipado(UpgradeCategory cat) => equipados.TryGetValue(cat, out var u) ? u : null;


    public abstract bool TieneFondos(int precio);
    public abstract void Cobrar(int precio);

    public virtual void AplicarUpgrade(UpgradeData upg)
    {
        if (upgrades.Contains(upg)) return;

        upg.ApplyUpgrade(gameObject);
        upgrades.Add(upg);
        debugUpgrades.Add(upg);
    }

    protected void BorrarRegistros(UpgradeData upg)
    {
        upgrades.Remove(upg);
        debugUpgrades.Remove(upg);
    }

    public bool HasUpgrade(UpgradeData upg) => upgrades.Contains(upg);
}
