﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class GameManager : MonoBehaviour
{
    private int visibleScore = 0;

    private int trueScore = 0;    

    /// <summary>
    /// Array of UI elements to hide on casual mode
    /// </summary>
    public GameObject[] HideOnCasual;

    public ItemPane CooldownPane;
    public CooldownIcon CooldownIcon;

    public GameObject TutorialDialog;

    public SetabbleText ScoreUI = null;
    public SetabbleText CashUI = null;
    public SetabbleText TimerUI = null;
    public Tooltip TooltipUI = null;

    public Text LevelText;

    public Camera MainCamera = null;

    public ItemPane InventoryPane = null;

    public LevelGoalSign GoalBillboard;
    public SpriteRenderer ScreenTint;
    public GameObject Sparkles;

    public GameObject ItemBacks;

    public float SlideSpeed = 6.0f;

    public float GlowDuration = 10.0f;
    public float GlowActivationWindow = 4.0f;

    public float HintCalloutDuration = 2.0f;  

    public float ParticleDensityMultiplier = 0.25f;

    public GemGrid Grid;

    public GameObject Menu;

    public ParticleSystem MatchParticles;

    public GameTextSettings TextSettings;

    public WeedSettings WeedSettings;

    public TutorialAnimation HintCalloutPointerTemplate;
    private List<TutorialAnimation> activeHintCallouts = new List<TutorialAnimation>();

    public int MaxLevelScoreIncrease = 1000;    

    /// <summary>
    /// Level at which difficulty starts ramping up quite a bit!
    /// </summary>
    public int FirstMajorRampLevel = 15;
    public int MajorRampTimeDecrease = 3;
    public int MajorRampScoreIncrease = 1000;

    private LevelGoal cachedLevelGoalInstance = null;

    public Weather CurrentWeather = Weather.Normal;
    public CooldownTimer weatherEffectTimer;

    public bool IsColorSwapEnabled { get { return !this.colorSwapTimer.IsExpired; } }

    private List<CooldownTimer> itemCooldownTimers = new List<CooldownTimer>();
    private CooldownTimer cashForPointsTimer = new CooldownTimer(20.0f, true);
    private CooldownTimer colorSwapTimer = new CooldownTimer(20.0f, true);
    private CooldownTimer itemSpreeTimer = new CooldownTimer(20.0f, true);
    private CooldownTimer pickaxeTimer = new CooldownTimer(20.0f, true);
    private CooldownTimer shovelTimer = new CooldownTimer(20.0f, true);
    private CooldownTimer harvestStaffTimer = new CooldownTimer(20.0f, true);
    private CooldownTimer fertilityGuideTimer = new CooldownTimer(5.0f, true);

    private void ActivateItemWithTimer(float duration, Item_Boost item, CooldownTimer timer)
    {
        timer.SetBaseline(duration);
        timer.Reset();
        this.UpdateOrCreateCooldownIcon(item.Sprite.sprite, timer);
    }

    public void ActivateMagicFlask()
    {
        this.Grid.IrrigateAll();
    }

    public void ActivateHarvestStaff(float duration, Item_Boost item)
    {
        this.ActivateItemWithTimer(duration, item, this.harvestStaffTimer);
    }

    public void ActivateShovel(float duration, Item_Boost item)
    {
        this.ActivateItemWithTimer(duration, item, this.shovelTimer);
    }

    public void ActivatePickaxe(float duration, Item_Boost item)
    {
        this.ActivateItemWithTimer(duration, item, this.pickaxeTimer);
    }

    public void EnableCashForPoints(float duration, Item_Boost item)
    {
        this.ActivateItemWithTimer(duration, item, this.cashForPointsTimer);       
    }

    public void ActivateItemSpree(float duration, Item_Boost item)
    {
        this.ActivateItemWithTimer(duration, item, this.itemSpreeTimer);
        foreach (GameObject obj in this.InventoryPane.Items)
        {
            // add temp timers
            Item objItem = obj.GetComponent<Item>();
            if (objItem != null && objItem.CooldownCanBeSpreed)
            {
                objItem.cooldownTimer.SetTempBaseline(1.0f);
            }
        }

        this.itemSpreeTimer.OnTimerExpired += ItemSpreeTimer_OnTimerExpired;
    }

    private void ItemSpreeTimer_OnTimerExpired(object sender, EventArgs e)
    {
        // remove temp timers
        foreach (GameObject obj in this.InventoryPane.Items)
        {
            Item objItem = obj.GetComponent<Item>();
            if (objItem != null && objItem.CooldownCanBeSpreed)
            {
                objItem.cooldownTimer.RemoveTempBaseline();
                if (objItem.CooldownCanBeReset)
                {
                    objItem.cooldownTimer.Fill();
                }
            }
        }

        this.itemSpreeTimer.OnTimerExpired -= ItemSpreeTimer_OnTimerExpired;
    }

    public void ActivateFreeSwap()
    {
        this.NextSwapFree = true;
        PlayerManager.Instance.ProgressTowardsAchievment(AchievmentType.FlipFloppin,
            ref PlayerManager.Instance.Achievments.FlipFloppinProgress, 1, AchievmentManager.Instance.FlipFloppinIcon);
        if (this.Grid.Selected != null)
        {
            this.Grid.Selected.SetSelected(false);
            this.Grid.Selected = null;
        }
    }

    public void EnableColorSwap(float duration, Item_Boost item)
    {
        this.ActivateItemWithTimer(duration, item, this.colorSwapTimer);                
    }

    // typeId is just to identify existing cooldowns
    private void UpdateOrCreateCooldownIcon(Sprite icon, CooldownTimer timer)
    {
        // Search if one exists first
        if (this.CooldownPane.Items.Any(a => a.GetComponent<CooldownIcon>().GetTimer() == timer))
        {
            // Leave it alone; it will reset on its own
            return;
        }

        CooldownIcon cooldownIcon = Instantiate(this.CooldownIcon);
        if (icon != null)
        {
            cooldownIcon.SetIcon(icon);
        }
        cooldownIcon.ParentPane = this.CooldownPane;
        cooldownIcon.SetTimer(timer);
        this.CooldownPane.AddItem(cooldownIcon.gameObject, true);        
    }

    public void DestroyCooldowns()
    {
        foreach (GameObject obj in this.InventoryPane.Items)
        {
            Item item = obj.GetComponent<Item>();
            if (item != null && item.CooldownCanBeReset)
            {
                item.ResetCooldown();
            }
        }
    }

    public void TriggerTradeCash(int amountPerDollar)
    {
        int cash = PlayerManager.Instance.Cash;        
        int points = cash * amountPerDollar;
        this.UpdateScore(points);
        this.UpdateCash(-cash);
        Vector3 pos = this.CashUI.transform.position;
        this.GenerateParticleStream(points, pos.x, pos.y, false);        
    }

    [HideInInspector]
    public bool NextSwapFree = false;

    public bool IsQuickMiningEnabled { get { return !this.pickaxeTimer.IsExpired; } }

    public bool IsQuickShovelingEnabled { get { return !this.shovelTimer.IsExpired; } }

    public bool IsHarvestStaffEnabled { get { return !this.harvestStaffTimer.IsExpired; } }

    public LevelGoal[] LevelGoals;

    private Dictionary<string, GameObject> resourceMap = new Dictionary<string, GameObject>();

    private TimeSpan gameTimer = new TimeSpan();   

    private static GameManager instance = null;
    private  bool isPaused = false;

    public FloatyText FloatyText;

    public void UpdateGlowActivationWindow()
    {
        this.Grid.SetGlowActicationWindow(this.GetTotalGlowActivationWindow());
    }

    public float GetTotalGlowActivationWindow()
    {
        return this.GlowActivationWindow + PlayerManager.Instance.Bonuses.IrrigationTimingWindow;
    }

    public float GetTotalGlowDuration()
    {
        return this.GlowDuration + PlayerManager.Instance.Bonuses.IrrigationDurationBonus;
    }

    public static GameManager Instance
    {
        get { return instance; }
    }

    public Tooltip Tooltip
    {
        get { return TooltipUI; }
    }

    public bool IsPaused
    {
        get { return this.GoalBillboard.Animating || 
                this.TutorialDialog.gameObject.activeSelf || 
                this.isPaused; }
    }

    // Use this for initialization
    void Awake()
    {
        if (PlayerManager.Instance == null)
        {
            this.GotoMenu();
            return;
        }

        instance = this;
    }

    void Start()
    {
        instance = this;

        this.itemCooldownTimers = new List<CooldownTimer>()
        {
            this.colorSwapTimer,
            this.itemSpreeTimer,
            this.pickaxeTimer,
            this.shovelTimer,
            this.cashForPointsTimer,
            this.harvestStaffTimer,
        };

        MainCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
        this.GoalBillboard.DoneAnimating += GoalBillboard_DoneAnimating;

        if (PlayerManager.Instance.GameMode == GameMode.Casual)
        {
            this.HideCasualModeParts();
        }
        
        this.InitializeRound();
    }

    private void HideCasualModeParts()
    {        
        if (this.HideOnCasual != null)
        {
            foreach (GameObject obj in this.HideOnCasual)
            {
                if (obj != null)
                {
                    obj.SetActive(false);
                }
            }
        }
    }    

    public LevelGoal GetLevelGoal()
    {
        if (cachedLevelGoalInstance != null)
        {
            return cachedLevelGoalInstance;
        }

        LevelGoal goal = null;
        if (PlayerManager.Instance.CurrentLevel >= this.LevelGoals.Length)
        {
            // "Generate" a level
            goal = this.LevelGoals.Last();            

            if (PlayerManager.Instance.CurrentLevel > FirstMajorRampLevel)
            {
                int ramp = PlayerManager.Instance.CurrentLevel - FirstMajorRampLevel;
                goal.ScoreGoal += MajorRampScoreIncrease * ramp;
                goal.Time -= MajorRampTimeDecrease;
            }
            else
            {
                goal.ScoreGoal += this.MaxLevelScoreIncrease;
            }

            goal.Weather = (Weather)UnityEngine.Random.Range(0, 3);

            goal.WeedsProbability = 0.0f;
            goal.RedLeafProbability = 0.0f;
            goal.PoisonOreProbability = 0.0f;
            goal.FreezeGemProbability = 0.0f;
            goal.RedOreProbability = 0.01f;
            goal.TimeStoneProbability = 0.01f;

            switch (UnityEngine.Random.Range(1, 4))
            {
                case 1:
                    goal.WeedsProbability = 0.1f;
                    break;
                case 2:
                    goal.FreezeGemProbability = 0.1f;
                    break;
                case 3:
                    goal.PoisonOreProbability = 0.1f;
                    break;
                case 4:
                    goal.RedLeafProbability = 0.1f;
                    break;
            }

            switch (UnityEngine.Random.Range(1, 4))
            {
                case 1:
                    goal.WeedsProbability += 0.05f;
                    break;
                case 2:
                    goal.FreezeGemProbability = 0.05f;
                    break;
                case 3:
                    goal.PoisonOreProbability = 0.05f;
                    break;
                case 4:
                    goal.RedLeafProbability = 0.05f;
                    break;
            }
        }
        else
        {
            goal = this.LevelGoals[PlayerManager.Instance.CurrentLevel];
        }

        cachedLevelGoalInstance = goal;

        return goal;
    }

    private void InitializeRound()
    {
        this.cachedLevelGoalInstance = null;
        this.ClearHintCallouts();
        this.ScreenTint.gameObject.SetActive(true);

        this.LevelText.text = "Level " + (PlayerManager.Instance.CurrentLevel + 1).ToString();

        SoundManager.Instance.SetMusicTempo(1.0f);

        LevelGoal goal = this.GetLevelGoal();

        this.CurrentWeather = goal.Weather;
        float time = goal.WeatherEffectTimer == 0.0f? float.MaxValue : goal.WeatherEffectTimer;
        this.weatherEffectTimer = new CooldownTimer(time, false);

        WeatherManager.Instance.SetupWeather(this.CurrentWeather);

        SoundManager.Instance.PlayLevelMusic(this.CurrentWeather);

        this.InventoryPane.ClearList();
        if (PlayerManager.Instance.Inventory.Count > 0)
        {
            this.InventoryPane.gameObject.SetActive(true);
            foreach (Item item in PlayerManager.Instance.Inventory)
            {
                Item clone = Instantiate(item);
                this.InventoryPane.AddItem(clone.gameObject);
            }
        }
        else
        {
            // Hide pane if there is no inventory, to avoid clutter
            this.InventoryPane.gameObject.SetActive(false);
        }

        this.CashUI.SetText(PlayerManager.Instance.Cash.ToString());
        this.Grid.PopulateGrid(goal.MaxGems);
        this.SetGameTimeLimit(goal.Time);
        this.SetScore(goal.ScoreGoal);        
        this.GoalBillboard.SetGoal(new LevelGoal(goal.ScoreGoal, goal.Time));
        TooltipUI.SetVisible(false);
        this.StartCoroutine(ShowLevelStartSigns());               
    }

    private IEnumerator ShowLevelStartSigns()
    {
        yield return this.GoalBillboard.Animate();
        while (this.GoalBillboard.Animating)
        {
            yield return null;
        }

        TutorialManager.Instance.ShowCurrentLevelTutorial();
    }

    public void AfterIrrigation(CooldownTimer timeAfterMatch)
    {
        this.UpdateOrCreateCooldownIcon(null, timeAfterMatch);
    }

    void GoalBillboard_DoneAnimating(object sender, EventArgs e)
    {
        this.ScreenTint.gameObject.SetActive(false);
    }

    public void GotoMenu() 
    {        
        SceneManager.LoadScene("StartMenu");            
    }

    public void ExitClicked()
    {
        this.isPaused = false;
        this.Menu.SetActive(false);      
        StartCoroutine(Lose());
    }

    public IEnumerator Lose()
    {
        this.ScreenTint.gameObject.SetActive(true);
        this.GoalBillboard.SetLoss(PlayerManager.Instance.TotalScore);
        SoundManager.Instance.PlayMusic(MusicChoice.Lose);
        this.StartCoroutine(this.GoalBillboard.Animate());
        PlayerManager.Instance.AddScore(PlayerManager.Instance.TotalScore);
        SerializationManager.Instance.Save();
        while (this.IsPaused)
        {
            yield return null;
        }

        PlayerManager.Instance.JustFinishedGame = true;

        this.GotoMenu();
    }

    public IEnumerator BeatLevel()
    {
        PlayerManager.Instance.CurrentLevel++;

        PlayerManager.Instance.MaxLevel = Math.Max(PlayerManager.Instance.CurrentLevel, PlayerManager.Instance.MaxLevel);

        this.CheckLevelBeatAchievments();

        this.ScreenTint.gameObject.SetActive(true);
        this.GoalBillboard.SetSuccess();
        SoundManager.Instance.PlayMusic(MusicChoice.Win);
        this.StartCoroutine(this.GoalBillboard.Animate());        
        while (this.IsPaused)
        {
            yield return null;
        }

        // Check if new items unlocked
        List<Item> items = PlayerManager.Instance.GetAllLockedItems().Select(a => a.GetComponent<Item>()).ToList();
        foreach (Item item in items)
        {
            if (item.PointsToUnlock < PlayerManager.Instance.UniversalScore)
            {
                // Show unlock for each unlocked item
                PlayerManager.Instance.UnlockedItems.Add(item.name);

                // Boutique achievement
                if (!PlayerManager.Instance.Achievments.Boutique &&
                PlayerManager.Instance.UnlockedItems.Contains("TimeBoost"))
                {
                    PlayerManager.Instance.Achievments.Boutique = true;
                    AchievmentManager.Instance.AnnounceAchievment(AchievmentManager.Instance.Boutique);
                }

                this.ScreenTint.gameObject.SetActive(true);
                this.GoalBillboard.SetNewItem(item);
                this.StartCoroutine(this.GoalBillboard.Animate());
                while (this.IsPaused)
                {
                    yield return null;
                }
            }
        }        
        
        SerializationManager.Instance.Save();
                
        if (PlayerManager.Instance.GameMode == GameMode.Normal)
        {
            this.DoShop();
        }
        else
        {
            this.NextLevel();
        }
    }

    private void NextLevel()
    {
        this.InitializeRound();
    }

    private void DoShop()
    {
        // Only load shop if there are shop items
        List<Item> shopItems = PlayerManager.Instance.GetAllAvailableShopItems();
        if (shopItems.Count > 0)
        {
            SceneManager.LoadScene("Shop", LoadSceneMode.Single);
        }
        else
        {
            this.NextLevel();
        }
    }

    private void CheckLevelBeatAchievments()
    {
        // Checks only happen in Normal mode
        if (PlayerManager.Instance.GameMode == GameMode.Normal)
        {
            // NOTE: these assume level was just incremented 
            if (!PlayerManager.Instance.Achievments.BigScore &&
                    PlayerManager.Instance.CurrentLevel == 6)
            {
                // Big score achievement, for reaching level 7.
                AchievmentManager.Instance.AnnounceAchievment(AchievmentManager.Instance.BigScoreIcon);
                PlayerManager.Instance.Achievments.BigScore = true;
            }

            if (!PlayerManager.Instance.Achievments.BiggerScore &&
                    PlayerManager.Instance.CurrentLevel == 9)
            {
                // Big score achievement, for reaching level 10.
                AchievmentManager.Instance.AnnounceAchievment(AchievmentManager.Instance.BiggerScoreIcon);
                PlayerManager.Instance.Achievments.BiggerScore = true;
            }

            if (!PlayerManager.Instance.Achievments.BiggestScore &&
                    PlayerManager.Instance.CurrentLevel == 19)
            {
                // Big score achievement, for reaching level 20.
                AchievmentManager.Instance.AnnounceAchievment(AchievmentManager.Instance.BiggestScoreIcon);
                PlayerManager.Instance.Achievments.BiggestScore = true;
            }
        }
    }    

    // Update is called once per frame
    void Update()
    {        
        if (Input.GetMouseButtonDown(0))
        {
            if (this.GoalBillboard.Animating)
            {
                this.GoalBillboard.CurrentMovementSpeed *= 3.0f;
                this.GoalBillboard.CurrentLingerTime = 0.0f;
            }
        }        

        if (Input.GetKeyUp(KeyCode.Escape))
        {
            if (!this.Menu.activeSelf)
            {                
                this.OpenMenu(true);
            }
        }

        if (this.IsPaused)
        {
            return;
        }

        if (this.gameTimer.TotalSeconds <= 0.0f)
        {
            // Game over!
            this.StartCoroutine(this.Lose());
            return;
        }

        this.ScrollScore();

        if (this.trueScore <= 0 && this.Grid.CanMakeMove())
        {                                 
            this.StartCoroutine(this.BeatLevel());
            return;
        }

        foreach (CooldownTimer timer in this.itemCooldownTimers)
        {
            if (!timer.IsExpired)
            {
                timer.Tick(Time.deltaTime);
            }
        }

        if (this.CurrentWeather != Weather.Normal && 
            this.weatherEffectTimer.Tick(Time.deltaTime).IsExpired)
        {
            this.weatherEffectTimer.Reset();
            this.ActivateWeatherEffect();
        }

        if (PlayerManager.Instance.Bonuses.FertilityGuide)
        {
            this.ActivateFertilityGuide();
        }

        float weatherTimeMultiplier = this.CurrentWeather == Weather.Rainy ? 1.05f : 1.0f;
        double secondsToPass = Time.deltaTime * PlayerManager.Instance.Bonuses.SlowTimeMultiplierBonus * weatherTimeMultiplier;
        this.gameTimer = this.gameTimer.Subtract(TimeSpan.FromSeconds(secondsToPass));

        if (this.gameTimer.TotalSeconds < 15.0f)
        {
            SoundManager.Instance.SetMusicTempo(1.2f);
        }

        this.UpdateTimerText();
    }

    private void ActivateFertilityGuide()
    {
        if (this.fertilityGuideTimer.Tick(Time.deltaTime).IsExpired)
        {
            this.fertilityGuideTimer.Reset();
            Gem randomGem = this.Grid.ActiveGems.Where(a => !a.IsAWeed && !a.IsRock && !a.IsGlowing)
                                                .ToList().GetRandom();
            if (randomGem != null)
            {
                randomGem.Irrigate();
            }
        }
    }

    private void ActivateWeatherEffect()
    {
        if (this.CurrentWeather == Weather.Snowy)
        {
            Gem gem = this.Grid.ActiveGems.GetRandom();
            if (!gem.IsFrozen && !gem.IsRock)
            {
                gem.Freeze();
            }
        }

        if (this.CurrentWeather == Weather.Rainy)
        {
            Gem gem = this.Grid.ActiveGems.GetRandom();
            if (!gem.IsFrozen && !gem.IsRock && !gem.IsAWeed)
            {
                gem.Irrigate();
            }
        }

        if (this.CurrentWeather == Weather.Dry)
        {
            Gem gem = this.Grid.ActiveGems.GetRandom();
            if (gem != null && !gem.IsRock && !gem.IsAWeed)
            {
                gem.Rot();
            }
        }
    }

    private void UpdateTimerText()
    {
        string timeString = ((int)this.gameTimer.TotalSeconds).ToString();
        TimerUI.SetText(timeString);
    }

    public void BoostTime(int seconds)
    {
        PlayerManager.Instance.ProgressTowardsAchievment(AchievmentType.TimeToWaste,
            ref PlayerManager.Instance.Achievments.TimeToWasteProgress, 1, AchievmentManager.Instance.TimeToWasteIcon);

        Vector3 pos = this.TimerUI.transform.position;
        FloatyText text = GameUtils.GenerateFloatyTextAt("+" + seconds, pos.x, pos.y - 1.0f, this.FloatyText, null, Color.white);
        text.MoveSpeed = 0.25f;        
        this.gameTimer = this.gameTimer.Add(TimeSpan.FromSeconds(seconds));

        if (this.gameTimer.Seconds > 15.0f)
        {
            SoundManager.Instance.SetMusicTempo(1.0f);
        }

        this.UpdateTimerText();
    }

    public GameObject GetFromResources(string resourcePath)
    {
        if (this.resourceMap.ContainsKey(resourcePath))
        {
            return this.resourceMap[resourcePath];
        }

        GameObject resource = Resources.Load<GameObject>(resourcePath);
        this.resourceMap[resourcePath] = resource;
        return resource;
    }

    public void SetGameTimeLimit(long seconds)
    {
        if (PlayerManager.Instance.HasAchievment(AchievmentType.TimeToWaste))
        {
            seconds += 10;
        }

        this.gameTimer = new TimeSpan(TimeSpan.TicksPerSecond * seconds);
    }


    public void UpdateScore(int scoreGain)
    {
        this.trueScore -= scoreGain;        
        PlayerManager.Instance.TotalScore += scoreGain;
        PlayerManager.Instance.UniversalScore += scoreGain;
        this.trueScore = Math.Max(0, this.trueScore);        
    }

    public void ScrollScore()
    {
        if (this.visibleScore > this.trueScore)
        {
            this.visibleScore -= 3;
            this.visibleScore = Math.Max(this.trueScore, this.visibleScore);
            this.ScoreUI.SetText(this.visibleScore.ToString());
        }        
    }

    public void SetScore(int score)
    {
        this.visibleScore = score;        
        this.visibleScore = Math.Max(0, this.visibleScore);
        this.trueScore = this.visibleScore;
        this.ScoreUI.SetText(score.ToString());
    }

    public void UpdateCash(int increment)
    {
        PlayerManager.Instance.Cash += increment;
        if (!PlayerManager.Instance.Achievments.CashMoney &&
            PlayerManager.Instance.Cash >= PlayerManager.Instance.AchievmentGoals.CashMoney)
        {
            PlayerManager.Instance.Achievments.CashMoney = true;
            AchievmentManager.Instance.AnnounceAchievment(AchievmentManager.Instance.CashMoneyIcon);
        }

        this.CashUI.SetText(PlayerManager.Instance.Cash.ToString());
    }

    public int GetScoreValue(List<Gem> matches)
    {
        int totalVal = 0;
        int matchCount = matches.Count(a => !a.IsRotten); // discount rotten ones from count
        int glowValue = 0;
        totalVal += matches.Sum(a => a.IsRotten ? 1 : a.BasePointValue);
        if (matchCount > 3)
        {
            totalVal += (matchCount - 3) * 100;
        }        

        foreach (Gem gem in matches)
        {
            if (gem.IsRotten)
            {
                continue;
            }

            if (PlayerManager.Instance.Bonuses.SoupCauldron && matches.Count >= 3)
            {
                // Soup cauldron earns extra points per vegetable for matches of 3 or more
                totalVal += 5;
            }

            if (gem.IsGlowing)
            {
                glowValue += 10;

                glowValue += 10 * PlayerManager.Instance.Bonuses.IrrigationPointsBonus; // irrigation item bonus

                if (PlayerManager.Instance.HasAchievment(AchievmentType.IrrigationStation))
                {
                    glowValue += 5;
                }                
                
                totalVal += glowValue;
            }

            if (gem.GemColor == GemColor.Purple)
            {
                totalVal += gem.BasePointValue * PlayerManager.Instance.Bonuses.PurpleGemBonus; // eggplant bonus
            }

            if (gem.GemType == GemType.Tomato && PlayerManager.Instance.HasAchievment(AchievmentType.Mato))
            {
                totalVal += 10;
            }

            if (gem.GemType == GemType.Pumpkin && PlayerManager.Instance.HasAchievment(AchievmentType.Punkin))
            {
                totalVal += 10;
            }

            if (PlayerManager.Instance.Bonuses.BonusWeedValueEnabled)
            {
                // Prospector satchel value!
                if (gem.GemType == GemType.Weeds)
                {
                    totalVal += 3;
                }
                else if (gem.IsRock)
                {
                    totalVal += 10;
                }
            }
        }

        if (!cashForPointsTimer.IsExpired)
        {
            // DoubleScore item bonus
            totalVal *= 2;
        }        

        if (PlayerManager.Instance.Achievments.BigScore)
        {
            // BigScore bonus
            totalVal = (int)((float)totalVal * 1.05f);
        }
        if (PlayerManager.Instance.Achievments.BiggerScore)
        {
            // BiggerScore bonus
            totalVal = (int)((float)totalVal * 1.05f);
        }
      
        //GameUtils.LogCoordConcat(string.Format("Gained {0} from {1} matches", totalVal, matches.Count), matches);

        return totalVal;        
    }

    public int GetCashValue(List<Gem> matches)
    {
        if (!cashForPointsTimer.IsExpired)
        {
            return 0;
        }

        int totalVal = matches.Sum(a => a.BaseMoneyValue);
        if (totalVal > 0)
        {
            totalVal += matches.Count * PlayerManager.Instance.Bonuses.GoldGainBonus; // gold gain item bonus
            int glowCount = matches.Count(a => a.IsGlowing);
            totalVal += glowCount;
            totalVal += glowCount * PlayerManager.Instance.Bonuses.IrrigationPointsBonus; // irrigation item bonus
            totalVal += matches.Count(a => a.GemColor == GemColor.Purple) * PlayerManager.Instance.Bonuses.PurpleGemBonus; // eggplant item bonus

            if (PlayerManager.Instance.Achievments.CashMoney)
            {
                totalVal++;
            }
        }

        foreach (Gem gem in matches)
        {
            if (PlayerManager.Instance.Bonuses.BonusWeedValueEnabled)
            {
                // Prospector satchel value!
                if (gem.GemType == GemType.Weeds)
                {
                    totalVal += 1;
                }
                else if (gem.IsRock)
                {
                    totalVal += 10;
                }
            }
        }

        return totalVal;
    }

    public void OpenMenu (bool open)
    {
        if (this.Menu != null && !this.GoalBillboard.Animating)
        {
            this.isPaused = open;
            this.Menu.SetActive(open);
        }
    }

    private void CheckAchievmentMatchProgress(List<Gem> matches)
    {
        // Tomato achievments
        int matoCount = matches.Count(a => a.GemType == GemType.Tomato);
        PlayerManager.Instance.ProgressTowardsAchievment(AchievmentType.Mato,
            ref PlayerManager.Instance.Achievments.MatoProgress, matoCount, AchievmentManager.Instance.MatoIcon);

        // Pumpkin achievments          
        int punkinCount = matches.Count(a => a.GemType == GemType.Pumpkin);
        PlayerManager.Instance.ProgressTowardsAchievment(AchievmentType.Punkin,
            ref PlayerManager.Instance.Achievments.PunkinProgress, punkinCount, AchievmentManager.Instance.PunkinIcon);
    }

    public void ProcessMatchRewards(List<Gem> matches)
    {
        this.CheckAchievmentMatchProgress(matches);

        // Calculate score, raise score bubble
        int scoreValue = this.GetScoreValue(matches);
        int cashValue = this.GetCashValue(matches);
        float averageMatchX = matches.Average(a => a.transform.localPosition.x);
        float averageMatchY = matches.Average(a => a.transform.localPosition.y);
        float randomOffset = UnityEngine.Random.Range(-0.5f, 0.5f);
        float offsetX = averageMatchX + randomOffset;
        this.UpdateScore(scoreValue);
        GameUtils.GenerateSuperFloatyTextAt(scoreValue.ToString(), offsetX, averageMatchY, this.TextSettings.ScoreTextTemplate, this.Grid.gameObject);
        this.UpdateCash(cashValue);

        if (cashValue > 0 && PlayerManager.Instance.GameMode == GameMode.Normal)
        {
            // No need to display cash in Casual mode
            GameUtils.GenerateSuperFloatyTextAt("$" + cashValue.ToString(), offsetX, averageMatchY + 1.0f, this.TextSettings.MoneyTextTemplate, this.Grid.gameObject);
        }
        
        if (PlayerManager.Instance.Bonuses.WorkBoots && matches.Count >= 3)
        {
            // Only time boost on actual matches
            this.BoostTime(1);
        }

        this.GenerateParticleStream(scoreValue, averageMatchX, averageMatchY);        
    }

    private bool clearLock = false;
    public void ClearHintCallouts()
    {
        if (clearLock) return;

        clearLock = true;

        foreach (TutorialAnimation callout in this.activeHintCallouts)
        {
            if (callout != null && callout.gameObject != null)
            {
                Destroy(callout.gameObject);
            }
        }

        activeHintCallouts.Clear();

        clearLock = false;
    }

    public void CreateHintCallout(Gem gem)
    {
        
        TutorialAnimation callout = Instantiate(this.HintCalloutPointerTemplate);
        
        callout.MoveAction.XInit = gem.transform.position.x;
        callout.MoveAction.XFinish = gem.transform.position.x;
        callout.MoveAction.YInit = gem.transform.position.y - 0.50f;
        callout.MoveAction.YFinish = gem.transform.position.y - 0.25f;
        callout.InitAction();
        this.activeHintCallouts.Add(callout);
    }

    private void GenerateParticleStream(int numParticles, float sourceX, float sourceY, bool fromGrid = true)
    {
        if (this.MatchParticles != null)
        {
            ParticleSystem particles = Instantiate(this.MatchParticles);
            if (fromGrid)
            {
                particles.transform.parent = this.Grid.transform;
                particles.transform.localPosition = new Vector3(sourceX, sourceY);
            }
            else
            {
                particles.transform.position = new Vector3(sourceX, sourceY);
            }

            Vector3 target = new Vector3(this.ScoreUI.transform.position.x, this.ScoreUI.transform.position.y);
            Vector3 flatPos = new Vector3(particles.transform.position.x, particles.transform.position.y);
            Vector3 direction = target - flatPos;
            particles.transform.rotation = Quaternion.LookRotation(direction, Vector3.up);
            ParticleSystem.MainModule main = particles.main;

            main.maxParticles = (int)((float)numParticles * this.ParticleDensityMultiplier);
            main.startLifetime = direction.magnitude / main.startSpeed.constant; // t = d/v  

            var emission = particles.emission;
            emission.rateOverTime = new ParticleSystem.MinMaxCurve((numParticles / main.duration) * this.ParticleDensityMultiplier);

            particles.Play();
            Destroy(particles, main.duration + 5.0f);
        }
    }
}

[Serializable]
public class GameTextSettings
{
    public TextMeshPro ScoreTextTemplate;
    public TextMeshPro MoneyTextTemplate;
}

[Serializable]
public class WeedSettings
{
    public Weeds WeedsTemplate;
    public FreezeGem FreezeGemTemplate;
    public Weeds RedOreTemplate;
    public PoisonRock PoisonRockTemplate;
    public Weeds RedWeedsTemplate;
    public TimeStone TimeStoneTemplate;

    public ParticleSystem SmellParticles;
    public GameObject IceGraphic;
    public ParticleSystem IceClickParticles;
    public ParticleSystem IceKillParticles;
}

[Serializable]
public class LevelGoal
{
    public int ScoreGoal = 0;
    public int Time = 0;
    public int MaxGems = 6;

    public Weather Weather = Weather.Normal;
    public float WeatherEffectTimer = 5.0f;

    public float WeedsProbability = 0.10f;
    public float FreezeGemProbability = 0.05f;
    public float RedOreProbability = 0.01f;
    public float PoisonOreProbability = 0.0f;
    public float RedLeafProbability = 0.0f;
    public float TimeStoneProbability = 0.01f;

    public LevelGoal(int scoreGoal, int time)
    {
        this.ScoreGoal = scoreGoal;
        this.Time = time;
    }
}

public enum Weather
{
    Normal = 0,
    Dry = 1,
    Snowy = 2,
    Rainy = 3
}