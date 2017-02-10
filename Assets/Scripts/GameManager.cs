using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    private int visibleScore = 0;

    private int trueScore = 0;

    /// <summary>
    /// Array of UI elements to hide on casual mode
    /// </summary>
    public GameObject[] HideOnCasual;
        
    public SetabbleText ScoreUI = null;
    public SetabbleText CashUI = null;
    public SetabbleText TimerUI = null;
    public Tooltip TooltipUI = null;    
    public Camera MainCamera = null;

    public ItemPane InventoryPane = null;

    public LevelGoalSign GoalBillboard;
    public SpriteRenderer ScreenTint;
    public GameObject Sparkles;

    public GameObject ItemBacks;

    public float GlowDuration = 20.0f;
    public float GlowActivationWindow = 4.0f;
    public float ParticleDensityMultiplier = 0.25f;

    public GemGrid Grid;

    public GameObject Menu;

    public ParticleSystem MatchParticles;

    public int MaxLevelScoreIncrease = 1000;

    private float cashForPointsDuration = 0;
    public void EnableCashForPoints(float duration)
    {
        this.cashForPointsDuration = duration;
    }

    [HideInInspector]
    public bool NextSwapFree = false;

    public LevelGoal[] LevelGoals;

    private Dictionary<string, GameObject> resourceMap = new Dictionary<string, GameObject>();

    private TimeSpan gameTimer = new TimeSpan();   

    private static GameManager instance = null;
    private  bool isPaused = false;

    public FloatyText FloatyText;

    public void UpdateGlowActicationWindow()
    {
        this.Grid.SetGlowActicationWindow(this.GetTotalGlowActivationWindow());
    }

    public float GetTotalGlowActivationWindow()
    {
        return this.GlowActivationWindow + PlayerManager.Instance.IrrigationTimingWindow;
    }

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
        MainCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
        this.GoalBillboard.DoneAnimating += GoalBillboard_DoneAnimating;

        if (PlayerManager.Instance.GameMode == GameMode.Casual)
        {
            this.HideCasualModeParts();
        }

        SoundManager.Instance.PlayMusic(MusicChoice.Level);
        this.InitializeRound();
    }

    private void HideCasualModeParts()
    {        
        if (this.HideOnCasual != null)
        {
            foreach (GameObject obj in this.HideOnCasual)
            {
                obj.SetActive(false);  
            }
        }
    }

    private void InitializeRound()
    {
        this.ScreenTint.gameObject.SetActive(true);
        LevelGoal goal = null;  
        if (PlayerManager.Instance.CurrentLevel >= this.LevelGoals.Length) 
        {
            goal = this.LevelGoals.Last();
            goal.ScoreGoal += this.MaxLevelScoreIncrease;
        }
        else
        {
            goal = this.LevelGoals[PlayerManager.Instance.CurrentLevel];
        }

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

        this.Grid.PopulateGrid(goal.MaxGems);
        this.SetGameTimeLimit(goal.Time);
        this.SetScore(goal.ScoreGoal);        
        this.GoalBillboard.SetGoal(new LevelGoal(goal.ScoreGoal, goal.Time));
        TooltipUI.SetVisible(false);                 
        this.StartCoroutine(this.GoalBillboard.Animate());
    }

    void GoalBillboard_DoneAnimating(object sender, EventArgs e)
    {
        this.ScreenTint.gameObject.SetActive(false);
    }

    public void GotoMenu() 
    {
        SceneManager.LoadScene("StartMenu");            
    }

    public IEnumerator Lose()
    {
        this.ScreenTint.gameObject.SetActive(true);
        this.GoalBillboard.SetLoss(PlayerManager.Instance.TotalScore);
        this.StartCoroutine(this.GoalBillboard.Animate());
        PlayerManager.Instance.AddScore(PlayerManager.Instance.TotalScore);
        SerializationManager.Instance.Save();
        while (this.IsPaused)
        {
            yield return null;
        }

        this.GotoMenu();
    }    

    public IEnumerator BeatLevel()
    {
        this.ScreenTint.gameObject.SetActive(true);
        this.GoalBillboard.SetSuccess();
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
        List<GameObject> shopItems = PlayerManager.Instance.GetAllAvailableShopItems();
        if (shopItems.Count > 0)
        {
            SceneManager.LoadScene("Shop", LoadSceneMode.Single);
        }
        else
        {
            this.NextLevel();
        }
    }

    // Update is called once per frame
    void Update()
    {
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

        if (this.gameTimer.TotalSeconds <= 0.0f)
        {
            // Game over!
            this.StartCoroutine(this.Lose());
            return;
        }

        this.ScrollScore();

        if (this.visibleScore <= 0 && this.Grid.CanMakeMove())
        {         
            PlayerManager.Instance.CurrentLevel++;
            this.StartCoroutine(this.BeatLevel());
            return;
        }

        if (cashForPointsDuration > 0.0f)
        {
            cashForPointsDuration -= Time.deltaTime;
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

        if (cashForPointsDuration > 0.0f)
        {
            totalVal *= 2;
        }

        GameUtils.LogCoordConcat(string.Format("Gained {0} from {1} matches", totalVal, matches.Count), matches);

        return totalVal;        
    }

    public int GetCashValue(List<Gem> matches)
    {
        if (cashForPointsDuration > 0.0f)
        {
            return 0;
        }

        int totalVal = matches.Count;
        totalVal += matches.Count * PlayerManager.Instance.GoldGainBonus; // gold gain item bonus
        int glowCount = matches.Count(a => a.IsGlowing);
        totalVal += glowCount;
        totalVal += glowCount * PlayerManager.Instance.IrrigationPointsBonus; // irrigation item bonus
        totalVal += matches.Count(a => a.GemColor == GemColor.Purple) * PlayerManager.Instance.PurpleGemBonus; // eggplant item bonus
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
        GameUtils.GenerateFloatyTextAt(scoreValue.ToString(), offsetX, averageMatchY, this.FloatyText, this.Grid.gameObject);
        this.UpdateCash(cashValue);

        if (cashValue > 0 && PlayerManager.Instance.GameMode == GameMode.Normal)
        {
            // No need to display cash in Casual mode
            GameUtils.GenerateFloatyTextAt("$" + cashValue.ToString(), offsetX, averageMatchY + 1.0f, this.FloatyText, this.Grid.gameObject, Color.yellow);
        }
        
        this.GenerateParticleStream(scoreValue, averageMatchX, averageMatchY);        
    }

    private void GenerateParticleStream(int numParticles, float sourceX, float sourceY)
    {
        if (this.MatchParticles != null)
        {
            ParticleSystem particles = Instantiate(this.MatchParticles);
            particles.transform.parent = this.Grid.transform;
            particles.transform.localPosition = new Vector3(sourceX, sourceY);

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