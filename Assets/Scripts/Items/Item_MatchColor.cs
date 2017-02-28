using System;
using System.Collections.Generic;
using UnityEngine;

public class Item_MatchColor : Item
{
    public GemColor GemColorTargets = GemColor.Red;

    public override bool TriggerEffect()
    {
        this.StartCoroutine(this.Grid.MatchByColor(this.GemColorTargets));
        return true;
    }

    public override bool CanTriggerEffect()
    {
        if (GameManager.Instance != null && GameManager.Instance.Grid != null)
        {
            if (GameManager.Instance.Grid.HasMatches(new GemGrid.MatchOverrideRules() { ByColor = GemColorTargets }))
            {
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

