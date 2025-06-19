using UnityEngine;

[CreateAssetMenu(menuName = "Upgrades/Boots Upgrade")]
public class BootsUpgradeData : UpgradeData
{
    public bool enableDash;
    public float dashCooldown;

    public override void ApplyUpgrade(GameObject targetPlayer)
    {
        // Aca se va a crear el dash
    }
}