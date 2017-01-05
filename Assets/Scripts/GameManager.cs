using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    private int score = 0;

    public SetabbleText ScoreUI = null;
    public SetabbleText CashUI = null;
    public SetabbleText TimerUI = null;
    public Tooltip TooltipUI = null;    
    public Camera MainCamera = null;

    public ItemPane InventoryPane = null;

    public LevelGoalSign GoalBillboard;
    public SpriteRenderer ScreenTint;
    public GameObject Sparkles;

    public float GlowDuration = 20.0f;

    public GemGrid Grid;

    public LevelGoal[] LevelGoals;

    private Dictionary<string, GameObject> resourceMap = new Dictionary<string, GameObject>();

    private TimeSpan gameTimer = new TimeSpan();   

    private static GameManager instance = null;
    private  bool isPaused = false;

    public FloatyText FloatyText;

    public float GetTotalGlowDuration()
    {
        return this.GlowDuration + PlayerManager.Instance.IrrigationDurationBonus;
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
        get { return this.GoalBillboard.Animating || this.isPaused; }
    }

    // Use this for initialization
    void Awake()
    {
        instance = this;
        MainCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
        this.GoalBillboard.DoneAnimating += GoalBillboard_DoneAnimating;
    }

    void Start()
    {
        this.InitializeRound();
    }

    private void InitializeRound()
    {        
        this.ScreenTint.gameObject.SetActive(true);   
        if (PlayerManager.Instance.CurrentLevel == this.LevelGoals.Length) 
        {
            this.GotoMenu();
            return;
        }

        foreach (Item item in PlayerManager.Instance.Inventory)
        {
            Item clone = Instantiate(item);
            this.InventoryPane.AddItem(clone);
        }

        LevelGoal goal = this.LevelGoals[PlayerManager.Instance.CurrentLevel];
        this.Grid.PopulateGrid(goal.MaxGems);
        this.SetGameTimeLimit(goal.Time);
        this.SetScore(goal.ScoreGoal);        
        this.GoalBillboard.SetGoal(new LevelGoal(goal.ScoreGoal, goal.Time));
        TooltipUI.SetVisible(false);
        //this.LoadItemsFromResources();              
        this.StartCoroutine(this.GoalBillboard.Animate());
    }

    void GoalBillboard_DoneAnimating(object sender, EventArgs e)
    {
        this.ScreenTint.gameObject.SetActive(false);   
    }

    void GotoMenu() 
    {
        SceneManager.LoadScene("StartMenu");            
    }

    void DoShop()
    {        
        SceneManager.LoadScene("Shop", LoadSceneMode.Single);               
    }

    // Update is called once per frame
    void Update()
    {
        if (this.gameTimer.TotalSeconds <= 0.0f)
        {
            // Game over!
            // TODO: show message
            this.GotoMenu();
            return;
        }

        if (this.score <= 0 && this.Grid.CanMakeMove())
        {           
            // TODO: show message
            PlayerManager.Instance.CurrentLevel++;            
            this.DoShop();
            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            if (this.GoalBillboard.Animating)
            {
                this.GoalBillboard.CurrentMovementSpeed *= 5.0f;
                this.GoalBillboard.CurrentLingerTime = 0.0f;
            }
        }

        if (this.IsPaused)
        {
            return;
        }

        double secondsToPass = Time.deltaTime * PlayerManager.Instance.SlowTimeMultiplierBonus;
        this.gameTimer = this.gameTimer.Subtract(TimeSpan.FromSeconds(secondsToPass));
        this.UpdateTimerText();
    }    

    private void UpdateTimerText()
    {
        string timeString = ((int)this.gameTimer.TotalSeconds).ToString();
        TimerUI.SetText(timeString);
    }

    public void BoostTime(int seconds)
    {
        this.gameTimer = this.gameTimer.Add(TimeSpan.FromSeconds(seconds));
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
        this.gameTimer = new TimeSpan(TimeSpan.TicksPerSecond * seconds);
    }


    public void UpdateScore(int decrement)
    {
        this.score -= decrement;
        this.score = Math.Max(0, this.score);
        this.ScoreUI.SetText(score.ToString());
    }

    public void SetScore(int score)
    {
        this.score = score;
        this.score = Math.Max(0, this.score);
        this.ScoreUI.SetText(score.ToString());
    }

    public void UpdateCash(int increment)
    {
        PlayerManager.Instance.Cash += increment;
        this.CashUI.SetText(PlayerManager.Instance.Cash.ToString());
    }

    public int GetScoreValue(List<Gem> matches)
    {
        int totalVal = 0;
        int matchCount = matches.Count;
        if (matchCount <= 3)
        {
            totalVal = 100;
        }
        else
        {
            totalVal = (matchCount - 3) * 200;
        }

        foreach (Gem gem in matches)
        {
            if (gem.IsGlowing)
            {
                totalVal += 25;
                totalVal += 25 * PlayerManager.Instance.IrrigationPointsBonus; // irrigation item bonus
            }

            if (gem.GemColor == GemColor.Purple)
            {
                totalVal += 100 * PlayerManager.Instance.PurpleGemBonus; // eggplant bonus
            }
        }

        return totalVal;
    }

    public int GetCashValue(List<Gem> matches)
    {
        int totalVal = matches.Count;
        totalVal += matches.Count * PlayerManager.Instance.GoldGainBonus; // gold gain item bonus
        int glowCount = matches.Count(a => a.IsGlowing);
        totalVal += glowCount;
        totalVal += glowCount * PlayerManager.Instance.IrrigationPointsBonus; // irrigation item bonus
        totalVal += matches.Count(a => a.GemColor == GemColor.Purple) * PlayerManager.Instance.PurpleGemBonus; // eggplant item bonus
        return totalVal;
    }

    public FloatyText GenerateFloatyTextAt(string text, float x, float y, GameObject parent = null, Color? color = null)
    {
        FloatyText newFloatyText = GameObject.Instantiate(this.FloatyText);
        if (parent != null)
        {
            newFloatyText.transform.parent = parent.transform;
        }
        
        newFloatyText.SetText(text, color);
        newFloatyText.transform.localPosition = new Vector3(x, y, 100);
        return newFloatyText;
    }

    public void ProcessMatchRewards(List<Gem> matches)
    {
        // Calculate score, raise score bubble
        int scoreValue = this.GetScoreValue(matches);
        int cashValue = this.GetCashValue(matches);
        float averageMatchX = matches.Average(a => a.transform.localPosition.x);
        float averageMatchY = matches.Average(a => a.transform.localPosition.y);
        float randomOffset = UnityEngine.Random.Range(-0.5f, 0.5f);
        float offsetX = averageMatchX + randomOffset;
        this.UpdateScore(scoreValue);
        this.GenerateFloatyTextAt(scoreValue.ToString(), offsetX, averageMatchY, this.Grid.gameObject);
        this.UpdateCash(cashValue);
        this.GenerateFloatyTextAt("$" + cashValue.ToString(), offsetX, averageMatchY + 1.0f, this.Grid.gameObject, Color.yellow);
    }
}

[Serializable]
public class LevelGoal
{
    public int ScoreGoal = 0;
    public int Time = 0;
    public int MaxGems = 6;

    public LevelGoal(int scoreGoal, int time)
    {
        this.ScoreGoal = scoreGoal;
        this.Time = time;
    }
}