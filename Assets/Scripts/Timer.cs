using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Timer<T> where T : struct
{
    /// <summary>
    /// Tick the timer by delta amount. Returns reference to itself
    /// after the tick.
    /// </summary>
    /// <param name="delta">Amount to tick by</param>
    public abstract Timer<T> Tick(T delta);

    public abstract bool IsExpired { get; }

    public abstract void Reset(T? amount);

    /// <summary>
    /// Gets how much time left before expiry
    /// </summary>
    public abstract float TimeLeft { get; }
    public abstract float TimeLeftPercent { get; }    

    public abstract void SetBaseline(T amount);
}

public class CooldownTimer : Timer<float>
{
    private float baseline;
    private float current = 0.0f;

    public CooldownTimer(float baseline, bool startFull)
    {
        this.baseline = baseline;
        if (startFull)
        {
            this.current = this.baseline;
        }
    }

    protected CooldownTimer()
    {
    }

    public override bool IsExpired
    {
        get
        {
            return this.current >= this.baseline;
        }
    }

    public override float TimeLeftPercent
    {
        get
        {
            return baseline == 0.0f ? 0.0f : (1.0f - (this.current / this.baseline));            
        }
    }

    public override float TimeLeft
    {
        get
        {
            return Mathf.Max(0.0f, this.baseline - this.current);
        }
    }

    public override void Reset(float? amount = null)
    {
        this.current = amount ?? 0.0f;
    }

    public override void SetBaseline(float newBaseline)
    {
        this.baseline = newBaseline;
    }

    public override Timer<float> Tick(float delta)
    {
        if (this.current < this.baseline)
        {
            this.current += delta;
        }

        return this;
    }
}
