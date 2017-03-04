using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeatherManager : Singleton<WeatherManager>
{
    public SpriteRenderer Backdrop;

    public Sprite NormalBackdrop;
    public Sprite WinterBackdrop;

    public SpriteRenderer WinterTint;
    public SpriteRenderer DryTint;

    public GameObject SnowParticles;
    public GameObject DryParticles;

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

    private void SetupDryWeather()
    {
        this.Backdrop.sprite = this.NormalBackdrop;
        this.DryTint.gameObject.SetActive(true);
        this.DryParticles.SetActive(true);
    }
}
