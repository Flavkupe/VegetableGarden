using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TutorialAnimation : MonoBehaviour
{    
    public TutorialAnimationActionType Type;
    public MoveAnimationAction MoveAction;
    public StaticAnimationAction StaticAction;

    public GameObject Visual;

    private TutorialAnimationAction Action
    {
        get
        {
            switch (Type)
            {
                case TutorialAnimationActionType.Movemenet:
                    return MoveAction;                    
                default:
                    return StaticAction;
            }        
        }
    }

    // Use this for initialization
    void Start () {
        this.InitAction();
    }

    public void InitAction()
    {
        this.Action.InitAction(this);
    }
	
	// Update is called once per frame
	void Update ()
    {
        if (this.Action.IsActionComplete(this))
        {
            if (this.Action.Loop)
            {
                this.Action.InitAction(this);
            }            
            else
            {                
                return;
            }
        }

        this.Action.DoAction(this);
	}
}

public enum TutorialAnimationActionType
{
    Movemenet,
    Static,
}

[Serializable]
public abstract class TutorialAnimationAction
{    
    public float Seconds = 1.0f;
    public bool Loop = true;
    public abstract bool IsActionComplete(TutorialAnimation obj);
    public abstract void DoAction(TutorialAnimation obj);
    public abstract void InitAction(TutorialAnimation obj);

    protected void SetImageTransparency(TutorialAnimation obj, float amount)
    {
        Image image = obj.GetComponent<Image>();
        if (image != null)
        {
            image.color = new Color(image.color.r, image.color.g, image.color.b, amount);
        }
    }
}

[Serializable]
public class StaticAnimationAction : TutorialAnimationAction
{
    public CooldownTimer prestartTimer;

    protected bool appeared = false;
    public float PrestartTime = 0.0f;

    public override void DoAction(TutorialAnimation obj)
    {
        if (!appeared)
        {
            prestartTimer.Tick(Time.deltaTime);
            if (prestartTimer.IsExpired)
            {
                appeared = true;
                if (obj.Visual != null)
                {
                    obj.Visual.SetActive(true);
                }
                else
                {
                    SetImageTransparency(obj, 1.0f);
                }
            }
        }
    }

    public override void InitAction(TutorialAnimation obj)
    {
        this.prestartTimer = new CooldownTimer(this.PrestartTime, false);
        if (this.PrestartTime > 0.0f)
        {
            if (obj.Visual != null)
            {
                obj.Visual.SetActive(false);
            }
            else
            {
                SetImageTransparency(obj, 0.0f);
            }

            appeared = false;
        }
        else
        {
            SetImageTransparency(obj, 1.0f);
            appeared = true;
        }     
    }

    public override bool IsActionComplete(TutorialAnimation obj)
    {
        return false;        
    }
}

[Serializable]
public class MoveAnimationAction : StaticAnimationAction
{
    public CooldownTimer finalActionTimer;

    public float XInit;
    public float YInit;

    public float XFinish;
    public float YFinish;    

    public float DelayBeforeLoop = 0.0f;

    public bool FadeOnEnd = true;

    public float FadeSpeed = 10.0f;

    private bool fading = false;

    private bool FinishedMove(TutorialAnimation obj)
    {
        return Mathf.Abs((obj.transform.localPosition - new Vector3(XFinish, YFinish, 0)).magnitude) < ApproxThreshold;
    }

    /// <summary>
    /// How close it can be to target to finish.
    /// </summary>
    public float ApproxThreshold = 2.0f;

    private float percentDistanceTravelled = 0.0f;

    public override bool IsActionComplete(TutorialAnimation obj)
    {
        return FinishedMove(obj) && this.finalActionTimer.IsExpired;
    }
   
    public override void InitAction(TutorialAnimation obj)
    {
        base.InitAction(obj);        
        
        this.finalActionTimer = new CooldownTimer(this.DelayBeforeLoop, false);
        percentDistanceTravelled = 0.0f;
        obj.transform.localPosition = new Vector3(XInit, YInit, 0);
        fading = false;        
    }

    public override void DoAction(TutorialAnimation obj)
    {
        base.DoAction(obj);

        if (!appeared)
        {
            return;
        }

        if (fading)
        {
            Image image = obj.GetComponent<Image>();
            image.color = new Color(image.color.r, image.color.g, image.color.b, image.color.a - Time.deltaTime * this.FadeSpeed);
            this.finalActionTimer.Tick(Time.deltaTime);
            if (this.finalActionTimer.IsExpired)
            {
                fading = false;
            }
        }
        else
        {
            Vector3 pos = obj.transform.localPosition;
            percentDistanceTravelled += Time.deltaTime / this.Seconds;
            obj.transform.localPosition = Vector3.Lerp(new Vector3(XInit, YInit, 0), new Vector3(XFinish, YFinish, 0), percentDistanceTravelled);
            if (FinishedMove(obj))
            {
                this.finalActionTimer.Reset();
                this.fading = true;
            }
        }
    }
}
