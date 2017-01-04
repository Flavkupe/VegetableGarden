using System;
using UnityEngine;

public class Item : MonoBehaviour 
{
    public int Cost = 0;
    public string Name = string.Empty;
    public float Cooldown = 0.0f;
    public string Description = string.Empty;
    
    protected float CurrentCooldown = 0.0f;

    public bool Purchased = false;

    public GemGrid Grid = null;

    private SpriteRenderer sprite = null;

    private TextOverSprite cooldownText = null;    

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
    }

    void Update()
    {
        if (this.Purchased)
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
        else 
        {
            if (this.Cost > GameManager.Instance.Cash)
            {
                this.sprite.color = Color.red;
            }
            else
            {
                this.sprite.color = Color.white;
            }
        }
    }

    private void ProcessCooldown()
    {
        
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

    protected virtual void ProcessMouseDown()
    {
        if (this.Purchased)
        {
            if (this.Grid.CanMakeMove() && this.CurrentCooldown <= 0.0f)
            {
                this.TriggerEffect();
                this.CurrentCooldown = this.Cooldown;
                this.sprite.color = Color.black;
            }
        }
        else
        {
            if (GameManager.Instance.Cash >= this.Cost)
            {
                GameManager.Instance.PurchaseItem(this);
                this.Purchased = true;
            }
            else
            {
                GameManager.Instance.PlaySound(SoundEffects.Error);
            }
        }
    }
}
