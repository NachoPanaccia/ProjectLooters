using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoliceUpgradeHandler : PlayerUpgradeHandler
{
    [SerializeField] private int dinero = 0;

    public int Dinero => dinero;

    public override bool TieneFondos(int precio) => dinero >= precio;
    public override void Cobrar(int precio)
    {
        dinero -= precio;
        _uiManager.UpdateWallet(dinero);
    }
    
    private UIManager _uiManager;

    private void Start()
    {
        _uiManager = UIManager.instance;
    }

    public void AgregarDinero(int v)
    {
        dinero += v;
        _uiManager.UpdateWallet(dinero);
    }

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
