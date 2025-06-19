using UnityEngine;

[CreateAssetMenu(menuName = "Upgrades/Mask Upgrade")]
public class MaskUpgradeData : UpgradeData
{
    public float detectionRadius;

    public override void ApplyUpgrade(GameObject targetPlayer)
    {
        // Aca se aplicara la visión de los objetos para los ladrones
    }
}