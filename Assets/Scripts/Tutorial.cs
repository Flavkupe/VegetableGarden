using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Tutorial : MonoBehaviour
{
    private int currentFrame = 0;

    /// <summary>
    /// Use to ensure everything is in sync over longer periods of time by always
    /// starting from the beginning.
    /// </summary>
    public bool LoopFromStart = true;

    private CooldownTimer timer;

    public TutorialAnimation[] Animations;

    public string Text;

    public float BorderPosY;
    public float BorderHeight;         

    public TutorialPart[] Views;

    public void InitializeTutorial()
    {        
        this.currentFrame = 0;

        foreach (TutorialPart part in Views)
        {
            part.Image.SetActive(false);
        }

        if (this.Views.Length > 0)
        {
            TutorialPart frame = Views[currentFrame];
            frame.Image.SetActive(true);
            timer = new CooldownTimer(frame.ImageTransitionTime, false);
        }

        foreach (TutorialAnimation anim in Animations)
        {
            anim.InitAction();
        }
    } 

	// Use this for initialization
	void Start ()
    {               
    }
	
	// Update is called once per frame
	void Update () {
        if (timer != null)
        {
            timer.Tick(Time.deltaTime);
            if (timer.IsExpired)
            {
                this.Views[currentFrame].Image.SetActive(false);
                currentFrame++;

                if (currentFrame == Views.Length)
                {                    
                    if (LoopFromStart)
                    {
                        this.InitializeTutorial();
                    }
                    else
                    {
                        currentFrame = 0;
                    }
                }

                this.Views[currentFrame].Image.SetActive(true);
                timer.SetBaseline(this.Views[currentFrame].ImageTransitionTime);
                timer.Reset();
            }
        }
	}
}

[Serializable]
public class TutorialPart
{
    public GameObject Image;
    public float ImageTransitionTime;
}