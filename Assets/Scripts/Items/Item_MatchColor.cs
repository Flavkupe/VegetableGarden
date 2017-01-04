using System;
using UnityEngine;

public class Item_MatchColor : Item
{
    public GemColor GemColorTargets = GemColor.Red;

    public override bool TriggerEffect()
    {
        this.StartCoroutine(this.Grid.MatchByColor(this.GemColorTargets));
        return true;
    }

    void OnMouseDown()
    {
        this.ProcessMouseDown();
    }
}

