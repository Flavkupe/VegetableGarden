using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoisonRock : Weeds {

    public float RotTimerWait = 5.0f;

    private CooldownTimer rotTimer;

    // Use this for initialization
    void Start ()
    {
        this.OnStart();
        this.rotTimer = new CooldownTimer(this.RotTimerWait, false);
    }

    void Update()
    {
        this.OnUpdate();
    }

    protected override void OnUpdate()
    {
        base.OnUpdate();

        if (this.rotTimer.Tick(Time.deltaTime).IsExpired)
        {
            this.rotTimer.Reset();
            this.RotNeighbor();
        }
    }

    private void RotNeighbor()
    {
        List<Gem> neighbors = this.Grid.GetNeighbors(this);
        while (neighbors.Count > 0)
        {
            Gem neighbor = neighbors.GetRandom();
            if (neighbor.IsFrozen || neighbor.IsRock || neighbor.IsAWeed || neighbor.IsRotten)
            {
                neighbors.Remove(neighbor);
                continue;
            }
            else
            {
                neighbor.Rot();
                return;
            }
        }
    }
}
