using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item_PermanentEffect : Item
{
    public EffectType Effect = EffectType.SlowTime;

    public override bool TriggerEffect()
    {        
        return true;
    }

    public override bool IsInstantUse
    {
        get { return true; }
    }
}

public enum EffectType
{
    SlowTime,
    ExtraGold,
    FastDrop,
    IrrigationPoints,
    PurpleGemColorBonus,
    LessRockHP,
    BonusWeedValue,
    LuckyCharm,
    PurplePower,
    WorkBoots,
}