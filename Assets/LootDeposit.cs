using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LootDeposit : MonoBehaviour
{

    private void OnTriggerEnter2D(Collider2D collision)
    {
        IRobber thief = collision.gameObject.GetComponent<IRobber>();

        if (thief == null) { return; }
        thief.DepositLoot();
        //Algún feedback sonoro?
    }
}
