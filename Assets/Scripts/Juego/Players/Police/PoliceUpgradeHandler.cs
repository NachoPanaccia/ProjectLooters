using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoliceUpgradeHandler : PlayerUpgradeHandler
{
    [SerializeField] private int dinero = 0;
    [SerializeField] private UpgradeData armaInicial;
    private PoliceController _policeController;

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
        _policeController = GetComponent<PoliceController>();
        AplicarUpgrade(armaInicial);
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

        var wupg = upg as WeaponUpgradeData;
        if (wupg is not null) _policeController.CurrentWeapon = wupg;

        equipados[cat] = upg;
        base.AplicarUpgrade(upg);
    }
}
