using UnityEngine;

[CreateAssetMenu(menuName = "Upgrades/Backpack Upgrade")]
public class BackpackUpgradeData : UpgradeData
{
    public int itemCarryLimit;
    public bool can_dash_DashWhileCarrying;

    public override void ApplyUpgrade(GameObject targetPlayer)
    {
        var lootHandler = targetPlayer.GetComponent<LooterUpgradeHandler>();
        lootHandler.back_size = itemCarryLimit;

        var movementHandler = targetPlayer.GetComponent<LooterMovementController>();
        movementHandler.canDash = can_dash_DashWhileCarrying;
    }
}