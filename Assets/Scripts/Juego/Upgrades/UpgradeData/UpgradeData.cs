using UnityEngine;

public enum UpgradeCategory { Weapon, Boots, Backpack, Mask, Special }
public enum PlayerType { Police, Looter }

public abstract class UpgradeData : ScriptableObject
{
    public string upgradeName;
    [TextArea] public string description;
    public Sprite icon;

    public int tier;
    public int price;

    public UpgradeCategory category;
    public PlayerType owner;

    public abstract void ApplyUpgrade(GameObject targetPlayer);
}