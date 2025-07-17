using UnityEngine;

[CreateAssetMenu(menuName = "Upgrades/Special Upgrade")]
public class SpecialUpgradeData : UpgradeData
{
    public string specialEffectNote; // identificador textual

    public override void ApplyUpgrade(GameObject targetPlayer)
    {
        var lootHandler = targetPlayer.GetComponent<LooterUpgradeHandler>();
        lootHandler.EnableFirearm();
    }
}