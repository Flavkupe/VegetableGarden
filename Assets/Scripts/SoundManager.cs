using System;
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
    public AudioClip UseSound;

    public AudioClip MenuMusic;
    public AudioClip LevelMusic;
    public AudioClip ShopMusic;

    public AudioClip RainyMusic;
    public AudioClip SnowyMusic;
    public AudioClip DryMusic;

    public AudioClip Win;
    public AudioClip Lose;

    public Slider MusicSlider;
    public Slider SfxSlider;

    public SpecialSoundEffects SpecialSoundEffects;

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

    public void SetMusicTempo(float tempo = 1.0f)
    {
        this.MusicSource.pitch = tempo;
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

    public void PlaySound(AudioClip clip, float volume = 1.0f)
    {
        if (this.SoundSource == null)
        {
            return;
        }

        float effectiveVolume = PlayerManager.Instance.SfxVol * volume;        

        SoundSource.PlayOneShot(clip, effectiveVolume);
    }

    public void PlaySound(SoundEffects soundEffect, float volume = 1.0f)
    {
        switch (soundEffect)
        {
            case SoundEffects.Pop:
                this.PlaySound(this.PopSound, volume);
                break;
            case SoundEffects.Error:
                this.PlaySound(this.BuzzerSound, volume);
                break;
            case SoundEffects.Kachink:
                this.PlaySound(this.KachinkSound, volume);
                break;
            case SoundEffects.GenericUse:
                this.PlaySound(this.UseSound, volume);
                break;              
            default:
                break;
        }
    }

    public void PlayLevelMusic(Weather weather)
    {
        switch (weather)
        {
            case Weather.Normal:
                MusicSource.clip = LevelMusic;
                break;
            case Weather.Dry:
                MusicSource.clip = DryMusic;
                break;
            case Weather.Snowy:
                MusicSource.clip = SnowyMusic;
                break;
            case Weather.Rainy:
                MusicSource.clip = RainyMusic;
                break;
        }

        MusicSource.Play();
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
            case MusicChoice.Win:
                MusicSource.clip = Win;
                MusicSource.loop = false;
                break;
            case MusicChoice.Lose:
                MusicSource.clip = Lose;
                MusicSource.loop = false;
                break;
        }

        MusicSource.Play();
    }
}

[Serializable]
public class SpecialSoundEffects
{
    public AudioClip[] IceCarveSounds;
    public AudioClip[] IceKillSounds;
}

public enum SoundEffects
{
    Pop,
    Error,
    Kachink,
    GenericUse,
}

public enum MusicChoice
{
    Menu,
    Level,
    Shop,
    Win,
    Lose,
}
