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
}
