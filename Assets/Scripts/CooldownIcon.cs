using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CooldownIcon : MonoBehaviour
{
    private CooldownTimer timer;
    public Image Circle;
    public Image Icon;

    public ItemPane ParentPane;

    public void SetTimer(CooldownTimer timer)
    {        
        this.timer = timer;
    }

    public void SetIcon(Sprite sprite)
    {
        this.Icon.sprite = sprite;
    }

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		if (timer.IsExpired)
        {
            if (this.ParentPane != null)
            {
                this.ParentPane.RemoveItem(this.gameObject);
            }

            Destroy(this.gameObject);
        }
        else
        {
            this.Circle.fillAmount = timer.TimeLeftPercent;
        }
	}

    public CooldownTimer GetTimer()
    {
        return this.timer;
    }
}
