
using UnityEngine;

public class UpgradeTable : MonoBehaviour
{
    public GameObject thief_upgrade_ui;
    public GameObject police_upgrade_ui;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        IRobber thief = collision.gameObject.GetComponent<IRobber>();

        if (thief != null) { 
            thief_upgrade_ui.SetActive(true); 
            thief_upgrade_ui.GetComponent<UpgradeUI>().my_player = collision.gameObject;
            collision.gameObject.SetActive(false);
        }

        //Lo mismo para el cana, pero se activa la ui del cana

        //Algún feedback sonoro?
    }
}
