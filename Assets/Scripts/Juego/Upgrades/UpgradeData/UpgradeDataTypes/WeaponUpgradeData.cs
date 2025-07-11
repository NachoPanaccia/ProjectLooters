using UnityEngine;

[CreateAssetMenu(menuName = "Upgrades/Weapon Upgrade")]
public class WeaponUpgradeData : UpgradeData
{
    public int magazineSize;
    public float reloadTime;
    public float fireRate;
    public float spread;
    public int projectileNumber;

    public override void ApplyUpgrade(GameObject targetPlayer)
    {
        // Se va a aplicar la mejora al jugador. Aun no esta creado
    }
}