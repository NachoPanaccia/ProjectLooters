using UnityEngine;

[CreateAssetMenu(menuName = "Upgrades/Boots Upgrade")]
public class BootsUpgradeData : UpgradeData
{
    [Header("Dash Settings")]
    public bool habilitaDash = true;
    public float dashCooldown = 3f;
    public float dashFuerza = 10f;

    [Header("Speed Settings")]
    public bool modificaVelocidad = false;
    public float nuevaVelocidad = 6f;
    
    public override void ApplyUpgrade(GameObject target)
    {
        var mv = target.GetComponent<LooterMovementController>();
        if (mv == null) return;

        mv.ConfigureDash(habilitaDash, dashCooldown, dashFuerza);

        if (modificaVelocidad)
            mv.SetSpeed(nuevaVelocidad);
    }
    
    public override void Unequip(GameObject target)
    {
        var mv = target.GetComponent<LooterMovementController>();
        if (mv == null) return;

        mv.ResetMovementDefaults();
    }
}