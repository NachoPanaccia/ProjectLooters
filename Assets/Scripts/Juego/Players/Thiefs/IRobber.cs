
using UnityEngine;

public interface IRobber 
{
    public bool GetLoot(int value, Sprite sprite);
    public void DepositLoot();
    public void LoseLoot();

}
