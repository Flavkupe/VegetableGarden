using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FreezeGem : Weeds
{    
    public override bool CanMatchThree { get { return false; } }

    public float FreezeTimerWait = 5.0f;

    private CooldownTimer freezeTimer;

    // Use this for initialization
    void Start ()
    {
        this.freezeTimer = new CooldownTimer(this.FreezeTimerWait, false);
	}
	
	// Update is called once per frame
	void Update ()
    {
        this.OnUpdate();
    }

    protected override void OnUpdate()
    {
        base.OnUpdate();

        if (this.freezeTimer.Tick(Time.deltaTime).IsExpired)
        {
            this.freezeTimer.Reset();
            this.FreezeNeighbor();
        }
    }

    private void FreezeNeighbor()
    {
        List<Gem> neighbors = this.Grid.GetNeighbors(this);
        while (neighbors.Count > 0)
        {
            Gem neighbor = neighbors.GetRandom();
            if (neighbor.IsFrozen || neighbor.GemType == GemType.FreezeGem)
            {
                neighbors.Remove(neighbor);
                continue;
            }
            else
            {
                neighbor.Freeze();
                return;
            }
        }        
    }
}
