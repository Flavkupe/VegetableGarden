using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerManager : MonoBehaviour {

    private List<Item> inventory = new List<Item>();
    
    public int Cash = 0;

    public GameMode GameMode = GameMode.Normal;

    /// <summary>
    /// Price percent in which costs go up per level.
    /// </summary>
    public float LevelCostRamp = 0.1f;
    public float SellValueRatio = 0.25f;    

    public int CurrentLevel = 0;

    public float MusicVol = 1.0f;
    public float SfxVol = 1.0f;

    public List<string> UnlockedItems = new List<string>();
    public List<int> HighScores = new List<int>();
    public int UniversalScore = 0; // total score accross all games
    public int TotalScore = 0;
    public List<Item> Inventory { get { return this.inventory; } }

    private List<GameObject> itemsFromResources = null; 

    // Use this for initialization
    void Awake() {
        DontDestroyOnLoad(this.transform.gameObject);
        instance = this;

        LoadResources();
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
        return (int)((float)item.Cost * inflationRatio);
    }

    private void LoadResources()
    {
        this.itemsFromResources = Resources.LoadAll<GameObject>("Prefabs/Items").ToList();
    }

    // Items which are not in player inventory but are unlocked
    public List<GameObject> GetAllAvailableShopItems()
    {
        return this.itemsFromResources.Where(a => !this.inventory.Any(b => b.name == a.name) &&
                                                  this.UnlockedItems.Any(c => c == a.name)).ToList();                                     
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
        this.Cash -= GetTrueItemCost(item);
        if (item.IsInstantUse)
        {
            this.ApplyInstantItem(item);
        }
        else
        {
            this.AddItem(item);
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
                case EffectType.IrrigationDuration:
                    this.IrrigationDurationBonus += 3.0f;
                    this.IrrigationTimingWindow++;
                    if (GameManager.Instance != null)
                    {
                        GameManager.Instance.UpdateGlowActicationWindow();
                    }

                    break;
                case EffectType.IrrigationPoints:
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