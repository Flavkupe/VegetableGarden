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
                GameManager.Instance.EnableCashForPoints(20.0f, this);
                return true;
            }
            else if (Boost == BoostType.TradeCash_10)
            {
                GameManager.Instance.TriggerTradeCash(10);
                return true;
            }
            else if (Boost == BoostType.ColorSwap_20)
            {
                GameManager.Instance.EnableColorSwap(this);
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

    /// <summary>
    /// For 20 seconds, no cash but double points
    /// </summary>
    CashForPoints_20,

    /// <summary>
    /// Trade cash 
    /// </summary>
    TradeCash_10,

    /// <summary>
    /// For 20 seconds, can swap vegetables based on color
    /// </summary>
    ColorSwap_20,
}
