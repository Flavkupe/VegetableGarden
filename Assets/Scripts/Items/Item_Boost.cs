using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item_Boost : Item
{
    public BoostType Boost = BoostType.Time_20;

    public override bool TriggerEffect()
    {
        if (Boost == BoostType.Time_20)
        { 
            if (GameManager.Instance != null)
            {
                GameManager.Instance.BoostTime(20);
                return true;
            }
        }

        return false;
    }

    void OnMouseDown()
    {
        this.ProcessMouseDown();
    }
}

public enum BoostType
{
    Time_20,
}
