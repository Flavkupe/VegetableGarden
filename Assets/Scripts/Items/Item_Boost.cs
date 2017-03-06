using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item_Boost : Item
{
    public BoostType Boost = BoostType.Time_20;

    public float Magnitude = 20.0f;

    public override bool MustWaitForStaticBoard { get { return false; } }

    protected override void ApplyCooldownBonus()
    {        
        if (this.Boost == BoostType.FreeSwap && PlayerManager.Instance.HasAchievment(AchievmentType.FlipFloppin))
        {
            this.Cooldown /= 2.0f;
        }

        base.ApplyCooldownBonus();
    }

    public override bool TriggerEffect()
    {
        if (GameManager.Instance != null)
        {
            if (Boost == BoostType.Time_20)
            {
                GameManager.Instance.BoostTime((int)Magnitude);
                return true;
            }        
            else if (Boost == BoostType.FreeSwap)
            {                
                GameManager.Instance.ActivateFreeSwap();
                return true;
            }
            else if (Boost == BoostType.CashForPoints_20)
            {
                GameManager.Instance.EnableCashForPoints(Magnitude, this);
                return true;
            }
            else if (Boost == BoostType.TradeCash_10)
            {
                GameManager.Instance.TriggerTradeCash((int)Magnitude);
                return true;
            }
            else if (Boost == BoostType.ColorSwap_20)
            {
                GameManager.Instance.EnableColorSwap(Magnitude, this);
                return true;
            }
            else if (Boost == BoostType.ItemSpree)
            {
                GameManager.Instance.ActivateItemSpree(Magnitude, this);
                return true;
            }
            else if (Boost == BoostType.DestroyCooldowns)
            {
                GameManager.Instance.DestroyCooldowns();
                return true;
            }
            else if (Boost == BoostType.Pickaxe)
            {
                GameManager.Instance.ActivatePickaxe(Magnitude, this);
                return true;
            }
            else if (Boost == BoostType.Shovel)
            {
                GameManager.Instance.ActivateShovel(Magnitude, this);
                return true;
            }
            else if (Boost == BoostType.Magic_Flask)
            {
                GameManager.Instance.ActivateMagicFlask();
                return true;
            }
            else if (Boost == BoostType.HarvestStaff)
            {
                GameManager.Instance.ActivateHarvestStaff(Magnitude, this);
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

    ItemSpree,

    DestroyCooldowns,

    Pickaxe,

    Shovel,

    Magic_Flask,

    HarvestStaff,
}
