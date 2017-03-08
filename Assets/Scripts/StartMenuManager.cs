using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Security.Policy;

public class StartMenuManager : MonoBehaviour 
{
    private bool loaded = false;
    private bool loadedLeaderboard = false;

    public GameObject GameModeMenu;

    public GameObject NameMenu;
    public GameObject Menu;
    public GameObject AchievmentsTab;
    public GameObject SoundTab;
    public GameObject HighScoresTab;
    public GameObject ItemsTab;
    public GameObject InnerMenu;
    public GameObject LeaderboardTab;

    public Animator MenuAnimator;
    
    public GameObject ExitSign;
    public GameObject HowToPlay;

    public GameObject CreditScreen;

    public Button SetNameButton;

    public SetabbleText LeaderboardLoadingText;
    public ScoreList LeaderboardScoresLeft;
    public ScoreList LeaderboardScoresRight;

    public GameObject TutorialMenu;

    public UnlockableItem UnlockableItemTemplate;

    public ItemPane UnlockedItemDisplay;
    public SetabbleText UnlockableItemText;

    public void HideItemText()
    {
        if (this.UnlockableItemText != null)
        {
            this.UnlockableItemText.SetText(string.Empty);
        }
    }

    public void ShowItemText(string text)
    {
        if (this.UnlockableItemText != null)
        {
            this.UnlockableItemText.SetText(text);
        }
    }

    public bool MenuOpened { get { return this.Menu.activeSelf || this.TutorialMenu.activeSelf || this.NameMenu.activeSelf; } }    

    public ScoreList ScoresLeft;
    public ScoreList ScoresRight;
    
    private string key1 = "5bsufazvpkqb24guldp8omtg0g8hey6vt4mkb5i5o0szp111cr8o2rv1nisd5rly35xetdah98ib8co1sau0smnmsynsfc6lr64j";        

    static StartMenuManager instance;
    public static StartMenuManager Instance
    { 
        get { return instance; } 
    }

    public void ShowCredits()
    {
        if (this.CreditScreen.activeSelf)
        {
            return;
        }
        else
        {
            this.CreditScreen.SetActive(true);
        }
    }

    public void StartAnew()
    {
        if (this.CreditScreen.activeSelf)
        {
            return;
        }

        SerializationManager.Instance.DeleteProgress();        
        SceneManager.LoadScene("StartMenu");
    }

    public void ShowModeMenu()
    {
        this.GameModeMenu.SetActive(true);
    }

    void Awake()
    {
        instance = this;
    }

	// Use this for initialization
	void Start () 
    {        
        if (Application.platform != RuntimePlatform.Android)
        {
            // Exit sign has no use outside of android.
            this.ExitSign.SetActive(false);

            // Move the How To Play sign up
            this.HowToPlay.transform.localPosition = new Vector3(this.HowToPlay.transform.localPosition.x, 20, 0);
        }        

        SoundManager.Instance.PlayMusic(MusicChoice.Menu);
        SerializationManager.Instance.Load();

        if (string.IsNullOrEmpty(PlayerManager.Instance.PlayerName))
        {
            this.NameMenu.SetActive(true);
        }

        if (PlayerManager.Instance.JustFinishedGame)
        {
            StartCoroutine(this.LogToLeaderboard());
        }
    }

    private IEnumerator LogToLeaderboard()
    {
        //string encScore = PlayerManager.Instance.TotalScore.ToString().Encrypt(key1);
        string encData = PlayerManager.Instance.GetExportDataString().Encrypt(key1);

        WWWForm formData = new WWWForm();
        formData.AddField("Name", PlayerManager.Instance.PlayerName ?? "___Unknown___");
        formData.AddField("Score", 0);
        formData.AddField("Data", encData);

        WWW www = new WWW("https://flaviovegetablegamesserver.azurewebsites.net/api/Player/PostScore/", formData);

        yield return www;

        if (www.error != null)
        {
            Debug.Log(www.error);
        }
        else
        {
            Debug.Log("Form upload complete!");
        }
    }

    public void LoadLeaderboard()
    {
        if (!this.loadedLeaderboard)
        {
            StartCoroutine(this.GetTopTenForLeaderboard());
        }
    }

    private IEnumerator GetTopTenForLeaderboard()
    {
        this.LeaderboardLoadingText.gameObject.SetActive(true);
        this.LeaderboardLoadingText.SetText("Loading...");
        WWW www = new WWW("https://flaviovegetablegamesserver.azurewebsites.net/api/Player/GetTopTen/");        

        yield return www;        
        if (www.error != null)
        {
            this.LeaderboardLoadingText.SetText("Could not contact server!");
            Debug.Log(www.error);
        }
        else
        {
            Debug.Log("http download complete!");
            this.loadedLeaderboard = true;
            this.LeaderboardLoadingText.gameObject.SetActive(false);
            this.ParseLeaderboardText(www.text);
        }
    }

    [Serializable]
    public class ScoreData
    {
        public int Score; 
        public string Name; 
    }

    private void ParseLeaderboardText(string text)
    {
        if (!string.IsNullOrEmpty(text))
        {
            ScoreData[] data = JsonHelper.FromJson<ScoreData>(text);

            if (data != null && data.Length > 0)
            {
                List<string> leaderboardScores = new List<string>();
                foreach (ScoreData item in data)
                {
                    leaderboardScores.Add(string.Format("{0} ({1})", item.Score, item.Name));
                }

                this.LeaderboardScoresLeft.SetScores(leaderboardScores.Take(Math.Min(5, leaderboardScores.Count)).ToList());
                this.LeaderboardScoresRight.SetScores(leaderboardScores.Skip(5).ToList());                           
            }
        }
    }

    public void MenuInit()
    {        
        if (!this.loaded)
        {
            this.LoadScores();
            this.PopulateUnlockableItemUI();
            this.loaded = true;
        }
    }    

    private void PopulateUnlockableItemUI()
    {
        List<GameObject> items = PlayerManager.Instance.GetItemsFromResources();
        foreach (GameObject item in items)
        {
            UnlockableItem newInst = Instantiate(UnlockableItemTemplate);            
            Image uiImage = newInst.GetComponent<Image>();
            bool locked = !PlayerManager.Instance.UnlockedItems.Any(a => a == item.name);
            newInst.SetItem(item.GetComponent<Item>(), !locked);

            uiImage.sprite = item.GetComponent<SpriteRenderer>().sprite;
            if (locked)
            {
                uiImage.color = Color.black;            
            }

            this.UnlockedItemDisplay.AddItem(newInst.gameObject);
        }
    }
	
	// Update is called once per frame
	void Update ()
    {
        if (Input.GetMouseButtonDown(0) && this.CreditScreen.activeSelf)
        {
            this.CreditScreen.SetActive(false);
            return;
        }

        if (Input.GetKeyUp(KeyCode.Escape))
        {
            if (this.Menu.activeSelf)
            {
                // Esc out of menu
                this.Menu.SetActive(false);
            }
            else
            {
                // Quit if esc while open
                Application.Quit();
            }
        }
    }

    public void LoadScores()
    {
        List<string> scores = PlayerManager.Instance.HighScores.Select(a => a.ToString()).ToList();
        
        int leftRange = Mathf.Min(scores.Count, this.ScoresLeft.Rows);
        this.ScoresLeft.SetScores(scores.GetRange(0, leftRange));
        if (scores.Count > this.ScoresLeft.Rows)
        {
            int rightRange = Mathf.Min(this.ScoresRight.Rows, scores.Count - this.ScoresLeft.Rows);
            this.ScoresRight.SetScores(scores.GetRange(this.ScoresLeft.Rows, rightRange));
        }        
    }

    private void StartGame(GameMode mode)
    {
        PlayerManager.Instance.GameMode = mode;
        PlayerManager.Instance.InitializeGame();
        SceneManager.LoadScene("Main", LoadSceneMode.Single);
    }

    public void OnStartNormalGamePressed()
    {
        if (!this.MenuOpened)
        {
            this.StartGame(GameMode.Normal);
        }
    }

    public void OnStartCasualGamePressed()
    {
        if (!this.MenuOpened)
        {
            this.StartGame(GameMode.Casual);
        }
    }

    public void OpenMenuToTab(int num)
    {
        OpenMenuToTab((StartMenuTabs)num);
    }

    public void OpenMenuToTab(StartMenuTabs tab)
    {
        if (InnerMenu != null && InnerMenu.activeSelf)
        {
            return;
        }

        if (tab == StartMenuTabs.Close)
        {
            this.MenuAnimator.Play("Close");
            return;
        }

        this.MenuInit();

        this.Menu.SetActive(true);
        this.AchievmentsTab.SetActive(false);
        this.SoundTab.SetActive(false);
        this.HighScoresTab.SetActive(false);
        this.ItemsTab.SetActive(false);
        this.LeaderboardTab.SetActive(false);
        switch (tab)
        {            
            case StartMenuTabs.Achievments:
                this.AchievmentsTab.SetActive(true);
                break;
            case StartMenuTabs.Items:
                this.ItemsTab.SetActive(true);
                break;
            case StartMenuTabs.HighScores:
                this.HighScoresTab.SetActive(true);
                break;
            case StartMenuTabs.Sound:            
                this.SoundTab.SetActive(true);
                break;
            case StartMenuTabs.Leaderboard:
                this.LeaderboardTab.SetActive(true);
                this.LoadLeaderboard();
                break;
            case StartMenuTabs.Close:
            default:                                
                break;
        }
    }

    public void SetPlayerName(string playerName)
    {
        PlayerManager.Instance.PlayerName = playerName;
        this.SetNameButton.interactable = !string.IsNullOrEmpty(playerName);
    }

    public void SetNameConfirm()
    {
        SerializationManager.Instance.Save();
        this.NameMenu.SetActive(false);
    }
}

public enum StartMenuTabs
{
    Sound = 0,
    Achievments = 1,
    Items = 2,
    HighScores = 3,
    Close = 4,
    Leaderboard = 5,
}
