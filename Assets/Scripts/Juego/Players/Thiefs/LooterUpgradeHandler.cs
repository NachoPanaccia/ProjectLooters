
using System;
using UnityEngine;

public class LooterUpgradeHandler : PlayerUpgradeHandler, IRobber
{

    [Header("Loot")]
    [SerializeField] private int actual_loot;
    [SerializeField] private int loot = 0;
    public int back_size = 1;
    [SerializeField] int back_used = 0;


    public int Loot => loot;

    public override bool TieneFondos(int precio) => loot >= precio;
    public override void Cobrar(int precio)
    {
        loot -= precio;
        _uiManager.UpdateWallet(loot);
    }

    private UIManager _uiManager;

    private void Start()
    {
        _uiManager = UIManager.instance;
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
        _uiManager.UpdateWallet(loot);
    }

    public bool GetLoot(int value, Sprite sprite) 
    {
        if (back_used >= back_size) return false;
        actual_loot += value;
        back_used += 1;
        _uiManager.UpdateLootInSlots(sprite);
        return true;
    }

    public void DepositLoot()
    {
        LevelManager.Instance.LooterDeposited(actual_loot);
        loot += actual_loot;
        actual_loot = 0;
        back_used = 0;
        _uiManager.UpdateWallet(loot);
        _uiManager.EmptyLootSlots();
    }

    public void LoseLoot() 
    {
        actual_loot = 0;
        back_used = 0;
    }

    public void UpdateBackpack(int size)
    {
        back_size = size;
        _uiManager.UpdateAvailableSlots(size);
    }
}
