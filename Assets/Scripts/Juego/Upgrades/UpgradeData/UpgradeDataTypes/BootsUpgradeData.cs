using UnityEngine;

[CreateAssetMenu(menuName = "Upgrades/Boots Upgrade")]
public class BootsUpgradeData : UpgradeData
{
    [Header("Dash")]
    public bool habilitaDash = false;
    public float dashCooldown = 6f;
    public float dashFuerza = 7f;

    public override void ApplyUpgrade(GameObject target)
    {
        var dash = target.GetComponent<DashBehaviour>();
        if (dash == null)
            dash = target.AddComponent<DashBehaviour>();

        dash.enabled = habilitaDash;
        dash.cooldown = dashCooldown;
        dash.impulso = dashFuerza;
    }

    public override void Unequip(GameObject target)
    {
        var dash = target.GetComponent<DashBehaviour>();
        if (dash != null) dash.enabled = false;
    }
}