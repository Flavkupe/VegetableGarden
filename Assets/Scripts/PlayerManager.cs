using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerManager : MonoBehaviour {

    private List<Item> inventory = new List<Item>();
    
    public int Cash = 0;

    public GameMode GameMode = GameMode.Normal;

    public Achievments Achievments = new Achievments();

    /// <summary>
    /// Price percent in which costs go up per level.
    /// </summary>
    public float LevelCostRamp = 0.1f;
    public float SellValueRatio = 0.25f;    

    public int CurrentLevel = 0;

    public float MusicVol = 0.5f;
    public float SfxVol = 0.5f;

    public List<string> UnlockedItems = new List<string>();
    public HashSet<string> PermanentItemsPurchased = new HashSet<string>();
    public List<int> HighScores = new List<int>();
    public int UniversalScore = 0; // total score accross all games
    public int TotalScore = 0;
    public List<Item> Inventory { get { return this.inventory; } }

    private List<GameObject> itemsFromResources = null; 

    // Use this for initialization
    void Awake() {
        if (instance != null)
        {
            // Only one may exist!
            Destroy(this.gameObject);
            return;
        }

        DontDestroyOnLoad(this.transform.gameObject);
        instance = this;

        LoadResources();
    }

    public void InitializeGame()
    {
        this.Cash = 0;
        this.inventory.Clear();
        this.CurrentLevel = 0;

        if (Achievments.BigPockets)
        {
            this.inventory.Add(GetAllAvailableShopItems().Select(a => a.GetComponent<Item>())
                                                         .Where(a => !a.IsInstantUse).ToList().GetRandom());
        }
    }
	
	// Update is called once per frame
	void Update () {		
	}

    public void AddScore(int score)
    {
        this.HighScores.Add(score);
        this.HighScores = this.HighScores.OrderByDescending(a => a).ToList();
        this.HighScores = this.HighScores.GetRange(0, Math.Min(10, this.HighScores.Count));
    }

    private static PlayerManager instance = null;

    public static PlayerManager Instance
    {
        get { return instance; }
    }

    public bool LoggingEnabled = false;

    public void SellItem(Item item)
    {
        this.Cash += (int)((float)item.Cost * SellValueRatio);        
        this.inventory.Remove(item);        
    }

    public int GetTrueItemCost(Item item)
    {
        float inflationRatio = (1.0f + (this.LevelCostRamp * (float)(this.CurrentLevel - 1)));
        float inflatedCost = (int)((float)item.Cost * inflationRatio);
        if (Achievments.CoffersProgress <= 0)
        {
            inflatedCost *= 0.9f;
        }

        return (int)inflatedCost;
    }

    private void LoadResources()
    {
        this.itemsFromResources = Resources.LoadAll<GameObject>("Prefabs/Items").ToList();
    }

    // Items which are not in player inventory but are unlocked
    public List<GameObject> GetAllAvailableShopItems()
    {
        return this.itemsFromResources.Where(a => !this.inventory.Any(b => b.name == a.name) &&
                                                  this.UnlockedItems.Any(c => c == a.name) && 
                                                  !this.PermanentItemsPurchased.Contains(a.name)).ToList();                                     
    }

    // Items which are not yet unlocked
    public List<GameObject> GetAllLockedItems()
    {
        return this.itemsFromResources.Where(a => !this.UnlockedItems.Any(b => b == a.name)).ToList();
    }

    public List<GameObject> GetItemsFromResources()
    {
        return this.itemsFromResources;
    }

    public void PurchaseItem(Item item)
    {        
        int cost = GetTrueItemCost(item);
        this.Cash -= cost;
        if (item.IsInstantUse)
        {            
            this.ApplyInstantItem(item);
            this.PermanentItemsPurchased.Add(item.name);
        }
        else
        {
            this.AddItem(item);
        }

        if (this.inventory.Count + this.PermanentItemsPurchased.Count >= 10 &&
            !PlayerManager.Instance.Achievments.BigPockets)
        {
            PlayerManager.Instance.Achievments.BigPockets = true;
            AchievmentManager.Instance.AnnounceAchievment(AchievmentManager.Instance.BigPocketsIcon);
        }

        // Coffers achievment progress
        if (GameUtils.CappedIncrement(ref this.Achievments.CoffersProgress, -cost, 0) &&
            this.Achievments.CoffersProgress == 0)
        {
            AchievmentManager.Instance.AnnounceAchievment(AchievmentManager.Instance.CoffersIcon);            
        }        
    }

    public int GoldGainBonus = 0;
    public float FastDropMultiplierBonus = 1.0f;
    public float IrrigationDurationBonus = 0.0f;
    public float IrrigationTimingWindow = 0.0f;
    public float SlowTimeMultiplierBonus = 1.0f;
    public int IrrigationPointsBonus = 0;

    public int PurpleGemBonus = 0;    

    private void ApplyInstantItem(Item item)
    {
        Item_PermanentEffect useItem = item as Item_PermanentEffect;
        if (useItem != null)
        {
            switch (useItem.Effect)
            {
                case EffectType.ExtraGold:
                    this.GoldGainBonus++;
                    break;
                case EffectType.FastDrop:
                    this.FastDropMultiplierBonus++;
                    break;                
                case EffectType.IrrigationPoints:
                    this.IrrigationDurationBonus += 3.0f;
                    this.IrrigationTimingWindow++;
                    if (GameManager.Instance != null)
                    {
                        GameManager.Instance.UpdateGlowActivationWindow();
                    }

                    this.IrrigationPointsBonus++;
                    break;
                case EffectType.SlowTime:
                    this.SlowTimeMultiplierBonus *= 0.8f;
                    break;
                case EffectType.PurpleGemColorBonus:
                    this.PurpleGemBonus++;
                    break;
            }
        }
    }

    private void AddItem(Item item)
    {
        this.inventory.Add(item);
    }
}

public enum GameMode
{
    Casual,
    Normal
}

[Serializable]
public class Achievments
{
    /// <summary>
    /// Match 1000 punkins
    /// Pumpkins worth 10% more.
    /// </summary>
    public int PunkinProgress = 1000;

    /// <summary>
    /// Match 1000 tomatoes
    /// Tomatoes worth 10% more.
    /// </summary>
    public int MatoProgress = 1000;

    /// <summary>
    /// Have 2000 gold at once.
    /// Get 1 extra gold after each match.
    /// </summary>
    public bool CashMoney = false;

    /// <summary>
    /// Make it to level 6.
    /// Get 10% extra score each match.
    /// </summary>
    public bool BigScore = false;

    /// <summary>
    /// Make it to level 10.
    /// Get 10% extra score each match.
    /// </summary>
    public bool BiggerScore = false;

    /// <summary>
    /// Make it to level 20.
    /// Nothing!
    /// </summary>
    public bool BiggestScore = false;

    /// <summary>
    /// Have 10 items at once.
    /// Start with random item each time.
    /// </summary>
    public bool BigPockets = false;

    /// <summary>
    /// Spend 10000 gold.
    /// Items cost 10% less.
    /// </summary>
    public int CoffersProgress = 10000;

    /// <summary>
    /// Use Time boost 100 times.
    /// 10 extra starting seconds each round.
    /// </summary>
    public int TimeToWasteProgress = 100;

    /// <summary>
    /// Irrigated 20000 times.
    /// 50% more points from irrigation.
    /// </summary>
    public int IrrigationStationProgress = 20000;
}