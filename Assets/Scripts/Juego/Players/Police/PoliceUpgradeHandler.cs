using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoliceUpgradeHandler : PlayerUpgradeHandler
{
    [SerializeField] private int dinero = 0;

    public int Dinero => dinero;

    public override bool TieneFondos(int precio) => dinero >= precio;
    public override void Cobrar(int precio) => dinero -= precio;

    public void AgregarDinero(int v) => dinero += v;

    public override void AplicarUpgrade(UpgradeData upg)
    {
        if (upg.category == UpgradeCategory.Weapon)
        {
            var anterior = GetEquipado(UpgradeCategory.Weapon);
            if (anterior != null)
            {
                anterior.Unequip(gameObject);
            }

            equipados[UpgradeCategory.Weapon] = upg;
        }

        base.AplicarUpgrade(upg);
    }
}
