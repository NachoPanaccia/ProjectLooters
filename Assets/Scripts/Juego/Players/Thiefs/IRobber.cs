using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IRobber 
{
    public void GetLoot(int value);
    public void DepositLoot();
    public void LoseLoot();

}
