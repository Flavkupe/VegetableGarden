using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weeds : Gem
{    
    public int HP = 3;

    public AudioClip[] CarveSounds;
    public AudioClip[] KillSounds;

    protected override void HandleMouseDown()
    {
        if (this.IsFrozen)
        {
            // If frozen, use default frozen behavior.
            base.HandleMouseDown();
            return;
        }

        if (GameManager.Instance.IsQuickMiningEnabled && this.GemType != GemType.Weeds)
        {
            this.HP -= 5;
        }
        else if (GameManager.Instance.IsQuickShovelingEnabled && this.GemType == GemType.Weeds)
        {
            this.HP -= 5;
        }
        else
        {
            this.HP--;
        }

        this.AnimateOnClickParticle();

        if (this.HP <= 0)
        {
            if (this.Grid.CanMakeMove())
            {
                this.Grid.DestroyOnClick(this);
                this.PlayKillSound();
                return;
            }
        }
        
        this.PlayCarveSound();       
    }

    protected virtual void PlayCarveSound()
    {
        SoundManager.Instance.PlaySound(CarveSounds.GetRandom());
    }

    public virtual void PlayKillSound()
    {
        SoundManager.Instance.PlaySound(KillSounds.GetRandom());
    }

    public override bool CanMatchThree { get { return false; } }

    public override IEnumerator Vanish()
    {       
        this.ShowMatchParticles();

        Destroy(this.gameObject);

        yield return null;
    }

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        this.OnUpdate();
	}

    protected override void OnUpdate()
    {
        base.OnUpdate();
    }
}
