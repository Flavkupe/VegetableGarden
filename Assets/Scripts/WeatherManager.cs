using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeatherManager : Singleton<WeatherManager>
{
    public SpriteRenderer Backdrop;

    public Sprite NormalBackdrop;
    public Sprite WinterBackdrop;
    public Sprite DryBackdrop;
    public Sprite RainBackdrop;

    public SpriteRenderer WinterTint;
    public SpriteRenderer DryTint;
    public SpriteRenderer RainTint;

    public GameObject SnowParticles;
    public GameObject DryParticles;
    public GameObject RainParticles;

    // Use this for initialization
    void Start ()
    {
        instance = this;		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void SetupWeather(Weather weather)
    {
        this.SnowParticles.SetActive(false);
        this.DryParticles.SetActive(false);
        this.WinterTint.gameObject.SetActive(false);
        this.DryTint.gameObject.SetActive(false);
        this.RainParticles.gameObject.SetActive(false);
        this.RainTint.gameObject.SetActive(false);

        switch (weather)
        {
            case Weather.Normal:
                this.SetupNormalWeather();
                break;                            
            case Weather.Snowy:
                this.SetupSnowyWeather();
                break;
            case Weather.Dry:
                this.SetupDryWeather();
                break;
            case Weather.Rainy:
                this.SetupRainWeather();
                break;
            default:
                break;
        }
    }

    private void SetupSnowyWeather()
    {
        this.Backdrop.sprite = this.WinterBackdrop;
        this.WinterTint.gameObject.SetActive(true);
        this.SnowParticles.SetActive(true);
    }

    private void SetupNormalWeather()
    {
        this.Backdrop.sprite = this.NormalBackdrop;        
    }

    private void SetupRainWeather()
    {
        this.Backdrop.sprite = this.RainBackdrop;
        this.RainTint.gameObject.SetActive(true);
        this.RainParticles.SetActive(true);
    }

    private void SetupDryWeather()
    {
        this.Backdrop.sprite = this.DryBackdrop;
        this.DryTint.gameObject.SetActive(true);
        this.DryParticles.SetActive(true);
    }
}
