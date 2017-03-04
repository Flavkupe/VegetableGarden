using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class PlayerManager : MonoBehaviour
{
    public List<Item> inventory = new List<Item>();
    
    public int Cash = 0;

    public GameMode GameMode = GameMode.Normal;

    public Achievments Achievments;
    public AchievmentGoals AchievmentGoals;

    public string PlayerName = null;

    public bool JustFinishedGame = false;

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

    public bool DebugMode = false;

    public int GoldGainBonus = 0;
    public float FastDropMultiplierBonus = 1.0f;
    public float IrrigationDurationBonus = 0.0f;
    public float IrrigationTimingWindow = 0.0f;
    public float SlowTimeMultiplierBonus = 1.0f;
    public int IrrigationPointsBonus = 0;

    public int HammerBonus = 0;

    public int PurpleGemBonus = 0;

    public bool BonusWeedValueEnabled = false;

    private List<GameObject> itemsFromResources = null;

    public void DeleteProgress()
    {
        this.Achievments = new Achievments();
        this.UnlockedItems.Clear();
        this.UniversalScore = 0;
        this.PlayerName = null;
    }

    public bool HasAchievment(AchievmentType type)
    {
        switch (type)
        {
            case AchievmentType.Punkin:
                return this.Achievments.PunkinProgress >= this.AchievmentGoals.Punkin;
            case AchievmentType.Mato:
                return this.Achievments.MatoProgress >= this.AchievmentGoals.Mato;
            case AchievmentType.CashMoney:
                return this.Achievments.CashMoney;
            case AchievmentType.BigScore:
                return this.Achievments.BigScore;
            case AchievmentType.BiggerScore:
                return this.Achievments.BiggerScore;
            case AchievmentType.BiggestScore:
                return this.Achievments.BiggestScore;
            case AchievmentType.BigPockets:
                return this.Achievments.BigPockets;
            case AchievmentType.Coffers:
                return this.Achievments.CoffersProgress >= this.AchievmentGoals.Coffers;
            case AchievmentType.TimeToWaste:
                return this.Achievments.TimeToWasteProgress >= this.AchievmentGoals.TimeToWaste;
            case AchievmentType.IrrigationStation:
                return this.Achievments.IrrigationStationProgress >= this.AchievmentGoals.IrrigationStation;
            case AchievmentType.FlipFloppin:
                return this.Achievments.FlipFloppinProgress >= this.AchievmentGoals.FlipFloppin;
            case AchievmentType.TiredOfWaiting:
                return this.Achievments.TiredOfWaitingProgress >= this.AchievmentGoals.TiredOfWaiting;
            default:
                return false;
        }
    }

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
        this.JustFinishedGame = false;
        this.Cash = 0;
        if (!DebugMode)
        {
            this.inventory.Clear();
        }

        this.CurrentLevel = 0;
        this.TotalScore = 0;

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

    public string GetExportDataString()
    {
        string items = string.Format("'items':'{0}'", string.Join(",", this.inventory.Select(a => a.name).ToArray()));
        string universalScore = string.Format("'uniscore':'{0}'", UniversalScore);
        string level = string.Format("'level':'{0}'", CurrentLevel);
        string mode = string.Format("'mode':'{0}'", GameMode.ToString());
        string platform = string.Format("'platform':'{0}'", Application.platform.ToString());
        string version = string.Format("'version':'{0}'", Application.version);
        string score = string.Format("'score':'{0}'", this.TotalScore.ToString()); 
        return string.Format("{{ {0} {1} {2} {3} {4} {5} {6} }}", items, universalScore, level, mode, platform, version, score);
    }

    private static PlayerManager instance = null;

    public static PlayerManager Instance
    {
        get { return instance; }
    }

    public bool LoggingEnabled = false;
    public bool LuckyCharmEnabled = false;

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

        if (this.inventory.Count + this.PermanentItemsPurchased.Count >= PlayerManager.Instance.AchievmentGoals.BigPockets &&
            !PlayerManager.Instance.Achievments.BigPockets)
        {
            PlayerManager.Instance.Achievments.BigPockets = true;
            AchievmentManager.Instance.AnnounceAchievment(AchievmentManager.Instance.BigPocketsIcon);
        }

        // Coffers achievment progress
        PlayerManager.Instance.ProgressTowardsAchievment(AchievmentType.Coffers,
            ref PlayerManager.Instance.Achievments.CoffersProgress, cost, AchievmentManager.Instance.CoffersIcon);    
    }

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
                    this.SlowTimeMultiplierBonus *= 0.9f;
                    break;
                case EffectType.PurpleGemColorBonus:
                    this.PurpleGemBonus++;
                    break;
                case EffectType.LessRockHP:
                    this.HammerBonus += 3;
                    break;
                case EffectType.BonusWeedValue:
                    this.BonusWeedValueEnabled = true;
                    break;
                case EffectType.LuckyCharm:
                    this.LuckyCharmEnabled = true;
                    break;

            }
        }
    }

    private void AddItem(Item item)
    {
        this.inventory.Add(item);
    }


    public void ProgressTowardsAchievment(AchievmentType type, ref int field, int amount, AchievmentIcon icon)
    {        
        if (!this.HasAchievment(type))
        {
            field += amount;
            if (this.HasAchievment(type))
            {
                if (AchievmentManager.Instance != null)
                {
                    AchievmentManager.Instance.AnnounceAchievment(icon);
                }
            }
        }
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
    public int PunkinProgress = 0;

    /// <summary>
    /// Match 1000 tomatoes
    /// Tomatoes worth 10% more.
    /// </summary>
    public int MatoProgress = 0;

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
    public int CoffersProgress = 0;

    /// <summary>
    /// Use Time boost 100 times.
    /// 10 extra starting seconds each round.
    /// </summary>
    public int TimeToWasteProgress = 0;

    /// <summary>
    /// Irrigated 20000 times.
    /// 50% more points from irrigation.
    /// </summary>
    public int IrrigationStationProgress = 0;

    public int FlipFloppinProgress = 0;

    public float TiredOfWaitingProgress = 0.0f;
}