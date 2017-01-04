using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;

public class GameManager : MonoBehaviour
{
    private int score = 0;
    private int cash = 0;
    private int currentLevel = 0;
    
    private SetabbleText ScoreUI = null;
    private SetabbleText CashUI = null;
    private SetabbleText TimerUI = null;
    private Tooltip TooltipUI = null;    
    private Camera MainCamera = null;
    private AudioSource SoundSource = null;

    private ItemPane InventoryPane = null;
    private ItemPane ShopPane = null;

    public AudioClip PopSound;
    public AudioClip BuzzerSound;
    public AudioClip KachinkSound;
    public LevelGoalSign GoalBillboard;
    public SpriteRenderer ScreenTint;
    public GameObject Sparkles;

    public float GlowDuration = 100.0f;

    public GemGrid Grid;

    public LevelGoal[] LevelGoals;

    private Dictionary<string, GameObject> resourceMap = new Dictionary<string, GameObject>();

    private TimeSpan gameTimer = new TimeSpan();
    
    private List<Item> warehouseInventory = new List<Item>();    

    private static GameManager instance = null;
    private  bool isPaused = false;    

    public static GameManager Instance
    {
        get { return instance; }
    }

    public int Cash
    {
        get { return cash; }
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
        ScoreUI = GameObject.FindGameObjectWithTag("ScoreUI").GetComponent<SetabbleText>();
        CashUI = GameObject.FindGameObjectWithTag("CashUI").GetComponent<SetabbleText>();
        TimerUI = GameObject.FindGameObjectWithTag("TimeUI").GetComponent<SetabbleText>();
        TooltipUI = GameObject.FindGameObjectWithTag("TooltipUI").GetComponent<Tooltip>();
        InventoryPane = GameObject.FindGameObjectWithTag("InventoryPane").GetComponent<ItemPane>();
        ShopPane = GameObject.FindGameObjectWithTag("ShopPane").GetComponent<ItemPane>();
        MainCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
        SoundSource = MainCamera.GetComponent<AudioSource>();
        this.GoalBillboard.DoneAnimating += GoalBillboard_DoneAnimating;
    }

    void Start()
    {
        this.InitializeRound();
    }

    private void InitializeRound()
    {        
        this.ScreenTint.gameObject.SetActive(true);   
        if (currentLevel == this.LevelGoals.Length) 
        {
            this.GotoMenu();
            return;
        }

        LevelGoal goal = this.LevelGoals[currentLevel];
        this.Grid.PopulateGrid(goal.MaxGems);
        this.SetGameTimeLimit(goal.Time);
        this.SetScore(goal.ScoreGoal);        
        this.GoalBillboard.SetGoal(new LevelGoal(goal.ScoreGoal, goal.Time));
        TooltipUI.SetVisible(false);
        this.LoadItemsFromResources();              
        this.StartCoroutine(this.GoalBillboard.Animate());
    }

    void GoalBillboard_DoneAnimating(object sender, EventArgs e)
    {
        this.ScreenTint.gameObject.SetActive(false);   
    }

    private void LoadItemsFromResources()
    {
        GameObject[] objects = Resources.LoadAll<GameObject>("Prefabs/Items");
        foreach (GameObject obj in objects)
        {
            GameObject instance = Instantiate(obj);
            this.ShopPane.AddItem(instance.GetComponent<Item>());
        }
    }

    void GotoMenu() 
    {
        Application.LoadLevel("StartMenu");
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

        if (this.score <= 0)
        {
            // TODO: show message
            this.currentLevel++;            
            this.InitializeRound();
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

        this.gameTimer = this.gameTimer.Subtract(TimeSpan.FromSeconds(Time.deltaTime));
        string timeString = ((int)this.gameTimer.TotalSeconds).ToString();
        TimerUI.SetText(timeString);
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
        this.ScoreUI.SetText(score.ToString());
    }

    public void SetScore(int score)
    {
        this.score = score;
        this.ScoreUI.SetText(score.ToString());
    }

    public void UpdateCash(int increment)
    {
        cash += increment;
        this.CashUI.SetText(cash.ToString());
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
            }
        }

        return totalVal;
    }

    public int GetCashValue(List<Gem> matches)
    {
        return matches.Count + matches.Count(a => a.IsGlowing);
    }

    public void PlaySound(SoundEffects soundEffect)
    {
        switch (soundEffect)
        {
            case SoundEffects.Pop:
                SoundSource.PlayOneShot(this.PopSound);
                break;
            case SoundEffects.Error:
                SoundSource.PlayOneShot(this.BuzzerSound);
                break;
            case SoundEffects.Kachink:
                SoundSource.PlayOneShot(this.KachinkSound);
                break;
            default:
                break;
        }
    }

    public FloatyText GenerateFloatyTextAt(string text, float x, float y, GameObject parent = null, Color? color = null)
    {
        GameObject obj = this.GetFromResources("Prefabs/FloatyScore");
        GameObject newInstance = GameObject.Instantiate(obj);
        if (parent != null)
        {
            newInstance.transform.parent = parent.transform;
        }

        FloatyText floatyText = newInstance.GetComponent<FloatyText>();
        floatyText.SetText(text, color);
        newInstance.transform.localPosition = new Vector3(x, y, -5);
        return floatyText;
    }



    public void PurchaseItem(Item item)
    {
        this.PlaySound(SoundEffects.Kachink);
        this.cash -= item.Cost;
        this.ShopPane.RemoveItem(item);
        this.InventoryPane.AddItem(item);
    }
}

public enum SoundEffects
{
    Pop,
    Error,
    Kachink
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