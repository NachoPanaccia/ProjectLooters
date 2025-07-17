
using Photon.Pun;
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

    //ELIMINAR ESTAS TRES LINEAS CUANDO HAYA UN SOLO THIEFCONTROLLER
    public Thief1Controller _thief1Controller;
    public Thief2Controller _thief2Controller;
    public Thief3Controller _thief3Controller;
    
    //DESCOMENTAR ÉSTA LINEA CUANDO HAYA UN SOLO THIEFCONTROLLER
    //private ThiefController _thiefController
    
    private void Start()
    {
        _uiManager = UIManager.instance;
        
        //DESCOMENTAR ÉSTA LINEA CUANDO HAYA UN SOLO THIEFCONTROLLER
        //_thiefController = GetComponent<ThiefController>();
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
        if(!photonView.IsMine) return;
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
        _uiManager.EmptyLootSlots();
    }

    public void UpdateBackpack(int size)
    {
        back_size = size;
        _uiManager.UpdateAvailableSlots(size);
    }

    public void EnableFirearm()
    {
        //ELIMINAR ESTOS TRES IFS CUANDO HAYA UN SOLO THIEFCONTROLLER
        if (_thief1Controller)
        {
            _thief1Controller.EnableFirearm();
        }
        if (_thief2Controller)
        {
            _thief2Controller.EnableFirearm();
        }
        if (_thief3Controller)
        {
            _thief3Controller.EnableFirearm();
        }
        
        //DESCOMENTAR ÉSTA LINEA CUANDO HAYA UN SOLO THIEFCONTROLLER
        //_thiefController.EnableFirearm
    }
}
