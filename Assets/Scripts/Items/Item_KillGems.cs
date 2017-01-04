using System;
using UnityEngine;

public class Item_KillGems : Item
{
    public GemColor GemColor = GemColor.Red;

    public override bool TriggerEffect()
    {
        this.StartCoroutine(this.Grid.RemoveAllGems(this.GemColor));
        return true;
    }

    void OnMouseDown()
    {
        this.ProcessMouseDown();
    }
}

