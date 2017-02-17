using System;
using UnityEngine;

public class Item : MonoBehaviour, IClickableItem
{
    public int Cost = 0;
    public string Name = string.Empty;
    public float Cooldown = 0.0f;
    public string Description = string.Empty;

    public CooldownTimer cooldownTimer = new CooldownTimer(100.0f, true);

    public bool CooldownCanBeReset = true;
    public bool CooldownCanBeSpreed = true;

    public int PointsToUnlock = int.MaxValue;
    
    public GemGrid Grid = null;

    private SpriteRenderer sprite = null;

    private TextOverSprite cooldownText = null;

    private GameObject itemBack = null;

    public SpriteRenderer Sprite { get { return this.sprite ?? this.GetComponent<SpriteRenderer>(); } }

    public virtual bool IsInstantUse
    {
        get { return false; }
    }

    void Awake()
    { 
    }

    void Start()
    {
        if (PlayerManager.Instance.HasAchievment(AchievmentType.FlipFloppin))
        {
            this.Cooldown /= 2.0f;
        }

        if (PlayerManager.Instance.HasAchievment(AchievmentType.TiredOfWaiting))
        {
            this.Cooldown--;            
        }

        this.Cooldown = Math.Max(1.0f, this.Cooldown);

        this.cooldownTimer.SetBaseline(this.Cooldown);
        this.sprite = this.GetComponent<SpriteRenderer>();
        if (GameManager.Instance != null)
        {
            this.Grid = GameObject.FindGameObjectWithTag("Grid").GetComponent<GemGrid>();            

            GameObject cooldownTextObj = Instantiate(GameManager.Instance.GetFromResources("Prefabs/TextOverSprite"));
            cooldownTextObj.transform.SetParent(this.transform, false);
            this.cooldownText = cooldownTextObj.GetComponent<TextOverSprite>();

            // Generate the item backs
            this.itemBack = Instantiate(GameManager.Instance.ItemBacks);
            this.itemBack.transform.parent = this.transform;
            this.itemBack.transform.localPosition = new Vector3(0.0f, 0.0f);
        }
    }

    /// <summary>
    /// Whether or not the item can be used while the board is in movement. If false,
    /// item can be used whenever as long as cooldown is ready.
    /// </summary>
    public virtual bool MustWaitForStaticBoard { get { return true; } }

    public void ResetCooldown()
    {
        this.cooldownTimer.Fill();
        this.UpdateCooldownUI();
    }

    private void UpdateCooldownUI()
    {
        this.cooldownText.SetText(((int)this.cooldownTimer.TimeLeft).ToString());
        if (this.cooldownTimer.IsExpired)
        {
            this.cooldownText.SetText(string.Empty);            
            this.sprite.color = Color.white;
        }
    }

    void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.IsPaused)
        {
            return;
        }

        // Progress towards this achievment
        if (!PlayerManager.Instance.HasAchievment(AchievmentType.TiredOfWaiting))
        {
            PlayerManager.Instance.Achievments.TiredOfWaitingProgress += Time.deltaTime;
            if (PlayerManager.Instance.HasAchievment(AchievmentType.TiredOfWaiting))
            {
                AchievmentManager.Instance.AnnounceAchievment(AchievmentManager.Instance.TiredOfWaitingIcon);
            }
        }

        cooldownTimer.Tick(Time.deltaTime);
        UpdateCooldownUI();                            
    }

    public virtual bool TriggerEffect()
    {
        return false;
    }

    void OnMouseEnter()
    {
        if (GameManager.Instance != null && GameManager.Instance.IsPaused)
        {
            return;
        }

        GameManager.Instance.Tooltip.SetStats(this.Cost.ToString(), this.Description, ((int)this.Cooldown).ToString());
        GameManager.Instance.Tooltip.SetVisible(true);
    }

    void OnMouseExit()
    {       
        GameManager.Instance.Tooltip.SetVisible(false);
    }

    public virtual void ProcessMouseDown()
    {
        if (GameManager.Instance != null && GameManager.Instance.IsPaused)
        {
            return;
        }
        
        if (this.cooldownTimer.IsExpired && 
            (!this.MustWaitForStaticBoard || this.Grid.CanMakeMove()))
        {
            SoundManager.Instance.PlaySound(SoundEffects.Use);
            this.TriggerEffect();
            this.cooldownTimer.Reset();
            this.sprite.color = Color.black;
        }        
        else
        {
            SoundManager.Instance.PlaySound(SoundEffects.Error);
        }     
    }
}
