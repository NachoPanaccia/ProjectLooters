using UnityEngine;

[CreateAssetMenu(menuName = "Upgrades/Backpack Upgrade")]
public class BackpackUpgradeData : UpgradeData
{
    public int itemCarryLimit;
    public bool disableDashWhileCarrying;

    public override void ApplyUpgrade(GameObject targetPlayer)
    {
        // Aca se va a aplicar la capacidad de carga a los jugadores
    }
}