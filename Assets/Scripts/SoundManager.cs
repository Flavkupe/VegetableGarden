using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour {

    private AudioSource SoundSource = null;

    public AudioClip PopSound;
    public AudioClip BuzzerSound;
    public AudioClip KachinkSound;

    private static SoundManager instance = null;

    public static SoundManager Instance
    {
        get
        {
            return instance;
        }
    }

    // Use this for initialization
    void Awake () {        
        instance = this;
        SoundSource = Camera.main.GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update () {
		
	}

    public void PlaySound(SoundEffects soundEffect)
    {
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
}

public enum SoundEffects
{
    Pop,
    Error,
    Kachink
}
