using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeStone : Weeds
{
    public int TimeBoost = 10;

    protected override void OnKill()
    {
        GameManager.Instance.BoostTime(this.TimeBoost);
    }

    // Use this for initialization
    void Start()
    {
        this.OnStart();
    }

    // Update is called once per frame
    void Update()
    {
        this.OnUpdate();
    }
}
