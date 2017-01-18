﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SoundManager : MonoBehaviour {

    private AudioSource SoundSource = null;
    public AudioSource MusicSource = null;

    public AudioClip PopSound;
    public AudioClip BuzzerSound;
    public AudioClip KachinkSound;

    public AudioClip MenuMusic;
    public AudioClip LevelMusic;
    public AudioClip ShopMusic;

    public Slider MusicSlider;
    public Slider SfxSlider;

    private static SoundManager instance = null;

    public static SoundManager Instance
    {
        get
        {
            return instance;
        }
    }

    // Use this for initialization
    void Awake ()
    {
        instance = this;
        SoundSource = Camera.main.GetComponent<AudioSource>();
    }

    void Start()
    {
        if (this.MusicSlider != null)
        {
            this.MusicSlider.value = PlayerManager.Instance.MusicVol;
        }

        if (this.SfxSlider != null)
        {
            this.SfxSlider.value = PlayerManager.Instance.SfxVol;
        }

        if (this.SoundSource != null)
        {
            this.SoundSource.volume = PlayerManager.Instance.SfxVol;
        }

        if (this.MusicSource != null)
        {
            this.MusicSource.volume = PlayerManager.Instance.MusicVol;
        }
    }

    // Update is called once per frame
    void Update () {
		
	}

    public void SetMusicVolFromSlider()
    {
        if (this.MusicSlider != null)
        {
            PlayerManager.Instance.MusicVol = this.MusicSlider.value;
            if (this.MusicSource != null)
            {
                this.MusicSource.volume = PlayerManager.Instance.MusicVol;
            }
        }
    }

    public void SetSfxVolFromSlider()
    {        
        if (this.SfxSlider != null)
        {
            PlayerManager.Instance.SfxVol = this.SfxSlider.value;
            if (this.SoundSource != null)
            {
                this.SoundSource.volume = PlayerManager.Instance.SfxVol;
            }
        }
    }

    public void PlaySound(SoundEffects soundEffect)
    {
        if (this.SoundSource == null)
        {
            return;
        }

        switch (soundEffect)
        {
            case SoundEffects.Pop:
                SoundSource.PlayOneShot(this.PopSound);
                break;
            case SoundEffects.Error:
                SoundSource.PlayOneShot(this.BuzzerSound);
                break;
            case SoundEffects.Kachink:
                SoundSource.PlayOneShot(this.KachinkSound);
                break;
            default:
                break;
        }
    }

    public void PlayMusic(MusicChoice choice)
    {
        if (this.MusicSource == null)
        {
            return;
        }

        switch (choice)
        {
            case MusicChoice.Level:
                MusicSource.clip = LevelMusic;                
                break;
            case MusicChoice.Menu:
                MusicSource.clip = MenuMusic;                
                break;
            case MusicChoice.Shop:
                MusicSource.clip = ShopMusic;
                break;
        }

        MusicSource.Play();
    }
}

public enum SoundEffects
{
    Pop,
    Error,
    Kachink
}

public enum MusicChoice
{
    Menu,
    Level,
    Shop,
}
