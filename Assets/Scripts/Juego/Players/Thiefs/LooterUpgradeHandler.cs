using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LooterUpgradeHandler : PlayerUpgradeHandler
{
    [SerializeField] private int loot = 0;

    public int Loot => loot;

    public override bool TieneFondos(int precio) => loot >= precio;
    public override void Cobrar(int precio) => loot -= precio;

    public void AgregarLoot(int v) => loot += v;

    public override void AplicarUpgrade(UpgradeData upg)
    {
        var cat = upg.category;
        var anterior = GetEquipado(cat);
        if (anterior != null)
        {
            anterior.Unequip(gameObject);
        }

        equipados[cat] = upg;
        base.AplicarUpgrade(upg);
    }
}
