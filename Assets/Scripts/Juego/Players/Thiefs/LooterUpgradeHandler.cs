using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LooterUpgradeHandler : PlayerUpgradeHandler, IRobber
{

    [Header("Loot")]
    [SerializeField] private int actual_loot;
    [SerializeField] private int loot = 0;

    public int Loot => loot;

    public override bool TieneFondos(int precio) => loot >= precio;
    public override void Cobrar(int precio) => loot -= precio;

    public void AgregarLoot(int v) => loot += v;

    public override void AplicarUpgrade(UpgradeData upg)
    {
        var cat = upg.category;
        var anterior = GetEquipado(cat);

        if (anterior != null && anterior != upg)
        {
            anterior.Unequip(gameObject);
            BorrarRegistros(anterior);
        }

        equipados[cat] = upg;
        base.AplicarUpgrade(upg);
    }

    public void GetLoot(int value) => actual_loot += value;

    public void DepositLoot()
    {
        LevelManager.Instance.LooterDeposited(actual_loot);
        loot += actual_loot;
        actual_loot = 0;
    }

    public void LoseLoot() => actual_loot = 0;
}
