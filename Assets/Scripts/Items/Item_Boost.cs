using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item_Boost : Item
{
    public BoostType Boost = BoostType.Time_20;

    public override bool TriggerEffect()
    {
        if (GameManager.Instance != null)
        {
            if (Boost == BoostType.Time_20)
            {
                GameManager.Instance.BoostTime(20);
                return true;
            }        
            else if (Boost == BoostType.FreeSwap)
            {
                GameManager.Instance.NextSwapFree = true;
                return true;
            }
            else if (Boost == BoostType.CashForPoints_20)
            {
                GameManager.Instance.EnableCashForPoints(20.0f);
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
    FreeSwap,
    CashForPoints_20,
}
