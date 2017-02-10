using System;
using UnityEngine;

public class Item : MonoBehaviour, IClickableItem
{
    public int Cost = 0;
    public string Name = string.Empty;
    public float Cooldown = 0.0f;
    public string Description = string.Empty;

    public int PointsToUnlock = int.MaxValue;

    protected float CurrentCooldown = 0.0f;    

    public GemGrid Grid = null;

    private SpriteRenderer sprite = null;

    private TextOverSprite cooldownText = null;

    private GameObject itemBack = null;

    public virtual bool IsInstantUse
    {
        get { return false; }
    }

    void Awake()
    { 
    }

    void Start()
    {       
        this.Grid = GameObject.FindGameObjectWithTag("Grid").GetComponent<GemGrid>();
        this.sprite = this.GetComponent<SpriteRenderer>();

        GameObject cooldownTextObj = Instantiate(GameManager.Instance.GetFromResources("Prefabs/TextOverSprite"));
        cooldownTextObj.transform.SetParent(this.transform, false);
        this.cooldownText = cooldownTextObj.GetComponent<TextOverSprite>();

        // Generate the item backs
        this.itemBack = Instantiate(GameManager.Instance.ItemBacks);
        this.itemBack.transform.parent = this.transform;
        this.itemBack.transform.localPosition = new Vector3(0.0f, 0.0f);
    }

    void Update()
    {        
        if (this.CurrentCooldown > 0.0f)
        {
            this.CurrentCooldown -= Time.deltaTime;
            this.cooldownText.SetText(((int)this.CurrentCooldown).ToString());
            if (this.CurrentCooldown <= 0.0f)
            {
                this.cooldownText.SetText(string.Empty);
                this.CurrentCooldown = 0.0f;
                this.sprite.color = Color.white;
            }
        }        
    }

    public virtual bool TriggerEffect()
    {
        return false;
    }

    void OnMouseEnter()
    {
        GameManager.Instance.Tooltip.SetStats(this.Cost.ToString(), this.Description, ((int)this.Cooldown).ToString());
        GameManager.Instance.Tooltip.SetVisible(true);
    }

    void OnMouseExit()
    {       
        GameManager.Instance.Tooltip.SetVisible(false);
    }

    public virtual void ProcessMouseDown()
    {        
        if (this.Grid.CanMakeMove() && this.CurrentCooldown <= 0.0f)
        {
            SoundManager.Instance.PlaySound(SoundEffects.Use);
            this.TriggerEffect();
            this.CurrentCooldown = this.Cooldown;
            this.sprite.color = Color.black;
        }   
        else
        {
            SoundManager.Instance.PlaySound(SoundEffects.Error);
        }     
    }
}
