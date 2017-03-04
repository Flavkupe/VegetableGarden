using UnityEngine;
using System.Collections;
using System;

public enum GemType
{
    White,
    Red, 
    Orange,
    Yellow,
    Green,
    Blue,
    Purple,

    Tomato,
    Pumpkin,
    Carrot,
    Onion,
    Eggplant,
    Potato,
    Beet,
    Broccoli,
    GreenPepper,
    Cucumber,

    Weeds,
    FreezeGem,
    ValuableOre,
}

public enum GemColor
{
    Green, // Brolli, Cucumber, GreenPepper
    Orange, // Punkin and Carrot
    Brown, // Onion, Potato
    Red, // Beet, Tomato
    Purple, // Eggplant

    Half, // Any gems (half of board)

    None,
}

public class Gem : MonoBehaviour 
{
    public GemType GemType = GemType.White;
    public GemColor GemColor = GemColor.Brown;

    public GameObject Sparkles;

    public GemGrid Grid;

    public int GemId;

    public int GridX;
    public int GridY;    

    public Transform Hover;
    public Transform Frame;

    public GameObject Ice;

    public bool InTransition = false;

    private GameObject sparkles = null;

    private CooldownTimer GlowTimer;

    public virtual bool CanMatchThree { get { return !this.IsFrozen; } }

    public ParticleSystem OnClickParticle;

    public bool IsGlowing {  get { return (this.GlowTimer != null && !this.GlowTimer.IsExpired); } }

    private bool mouseReleased = false;

    private Vector3? destination = null;

    public void Freeze()
    {
        GameObject ice = Instantiate(GameManager.Instance.WeedSettings.IceGraphic);
        this.Ice = ice;
        ice.transform.position = this.transform.position;
        ice.transform.SetParent(this.transform);
        this.freezeHp = 3;
    }

    public ParticleSystem MatchParticles;

    public int BasePointValue = 25;
    public int BaseMoneyValue = 1;

    private int freezeHp = 0;
    public bool IsFrozen { get { return this.freezeHp > 0; } }

    public bool IsRock { get { return this.GemType == GemType.ValuableOre ||
                                      this.GemType == GemType.FreezeGem; } }

    // Use this for initialization
    void Start () 
    {
        this.GlowTimer = new CooldownTimer(GameManager.Instance.GetTotalGlowDuration(), true);
        this.sparkles = Instantiate(GameManager.Instance.Sparkles);
        this.sparkles.transform.position = new Vector3(this.transform.position.x, this.transform.position.y, -1);
        this.sparkles.transform.parent = this.transform;
        this.sparkles.SetActive(false);
	}
	
	// Update is called once per frame
	void Update () 
    {
        this.OnUpdate();
    }

    protected virtual void AnimateOnClickParticle()
    {
        if (this.OnClickParticle != null && this.gameObject != null)
        {
            ParticleSystem system = Instantiate(this.OnClickParticle);
            system.transform.position = this.transform.position;
            system.Play();
            Destroy(system.gameObject, 4.0f);
        }
    }

    protected virtual void OnUpdate()
    {
        if (GameManager.Instance == null || GameManager.Instance.IsPaused)
        {
            return;
        }

        mouseReleased = Input.GetMouseButtonUp(0);

        if (this.sparkles != null && this.GlowTimer.Tick(Time.deltaTime).IsExpired)
        {
            // If glow expires
            this.sparkles.SetActive(false);
        }

        if (this.InTransition)
        {
            Debug.Assert(destination != null, "destination cannot be null if in transition");
            if (!this.transform.localPosition.IsNear(destination.Value))
            {
                this.transform.localPosition = Vector3.MoveTowards(this.transform.localPosition, destination.Value,
                    Time.deltaTime * GameManager.Instance.Grid.GetTotalSlideSpeed());
            }
            else
            {
                // snap into place once near enough
                this.transform.localPosition = destination.Value;
                this.InTransition = false;
                destination = null;
            }
        }
    }

    public void SlideGemTo(Vector3 destination)
    {
        this.destination = destination;
        this.InTransition = true;
        
    }

    public void SetSelected(bool selected)
    {
        if (selected)
        {
            this.Grid.Selected = this;
        }

        this.Grid.Selected.Frame.gameObject.SetActive(selected);
    }

    public void SetGridLoc(int x, int y)
    {
        this.GridX = x;
        this.GridY = y;
    }

    public bool IsMatch(Gem other)
    {
        if (other == null || other.GemType != this.GemType)
        {
            return false;
        }

        if (other == this)
        {
            return false;
        }

        return true;
    }

    public bool IsColorMatch(Gem other)
    {
        if (other == null || other.GemColor != this.GemColor)
        {
            return false;
        }

        if (other == this)
        {
            return false;
        }

        return true;
    }    

    void OnMouseDown() 
    {
        if (GameManager.Instance.IsPaused)
        {
            return;
        }

        this.HandleMouseDown();
    }

    protected virtual void HandleMouseDown()
    {
        if (GameManager.Instance.IsPaused)
        {
            return;
        }

        if (this.IsFrozen)
        {
            this.OnClickIce();
            return;            
        }

        if (!this.Grid.CanMakeMove() || this.Grid.SoonAfterMatch())
        {
            this.Irrigate();

            if (!this.Grid.CanMakeMove())
            {
                return;
            }
        }

        if (!this.Grid.TrySwapWith(this.Grid.Selected, this))
        {
            if (this.Grid.Selected != null)
            {
                this.Grid.Selected.Frame.gameObject.SetActive(false);
            }

            this.SetSelected(true);
        }
    }

    private void OnClickIce()
    {
        this.freezeHp--;

        this.freezeHp -= PlayerManager.Instance.HammerBonus;

        ParticleSystem particles = null;
        if (this.freezeHp > 0)
        {
            particles = Instantiate(GameManager.Instance.WeedSettings.IceClickParticles);
            SoundManager.Instance.PlaySound(SoundManager.Instance.SpecialSoundEffects.IceCarveSounds.GetRandom());
        }
        else
        {            
            particles = Instantiate(GameManager.Instance.WeedSettings.IceKillParticles);
            SoundManager.Instance.PlaySound(SoundManager.Instance.SpecialSoundEffects.IceKillSounds.GetRandom());
            if (this.Ice != null)
            {
                Destroy(this.Ice.gameObject);
                this.Ice = null;
            }
        }

        particles.transform.position = this.transform.position;
        particles.Play();
        Destroy(particles, 4.0f);
    }

    public virtual void Irrigate()
    {
        if (this.GlowTimer.IsExpired)
        {
            PlayerManager.Instance.ProgressTowardsAchievment(AchievmentType.IrrigationStation,
                ref PlayerManager.Instance.Achievments.IrrigationStationProgress, 1, AchievmentManager.Instance.IrrigationStationIcon);
        }

        // Irrigate on either condition, but do not continue if move is disallowed
        this.GlowTimer.Reset();
        this.sparkles.SetActive(true);
    }

    void OnMouseOver()
    {
        if (GameManager.Instance.IsPaused)
        {
            return;
        }

        if (mouseReleased)
        {
            this.Grid.TrySwapWith(this.Grid.Selected, this);            
        }
    }

    void OnMouseEnter()
    {
        if (GameManager.Instance.IsPaused)
        {
            return;
        }

        this.Hover.gameObject.SetActive(true);
    }

    void OnMouseExit()
    {
        if (GameManager.Instance.IsPaused)
        {
            return;
        }

        this.Hover.gameObject.SetActive(false);
    }

    protected virtual void ShowMatchParticles()
    {
        if (this.MatchParticles != null)
        {
            ParticleSystem particles = Instantiate(this.MatchParticles);
            particles.transform.position = this.transform.position;
            particles.Play();
            Destroy(particles.gameObject, 4.0f);
        }
    }

    public virtual IEnumerator Vanish()
    {
        if (this.gameObject == null)
        {
            yield return null;
        }

        this.ShowMatchParticles();

        bool vanished = false;       
        while (!vanished)
        {
            try
            {
                if (this.gameObject != null)
                {
                    Vector3 t = this.transform.localScale;
                    this.transform.localScale = new Vector3(t.x - 0.1f, t.y - 0.1f, 1);
                    this.transform.Rotate(0.0f, 0.0f, 36.0f);

                    if (this.transform.localScale.x <= 0.0f)
                    {
                        Destroy(this.gameObject);
                        vanished = true;
                    }
                }
            }            
            catch
            {
                if (this.gameObject != null)
                {
                    Destroy(this.gameObject);
                    vanished = true;
                }
            }

            if (!vanished)
            {
                yield return null;
            }
        }                       
    }
}
