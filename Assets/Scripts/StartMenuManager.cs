﻿using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine.UI;

public class StartMenuManager : MonoBehaviour 
{
    private bool loaded = false;
    public GameObject GameModeMenu;

    public GameObject Menu;
    public GameObject AchievmentsTab;
    public GameObject SoundTab;
    public GameObject HighScoresTab;
    public GameObject ItemsTab;

    public GameObject TutorialMenu;

    public UnlockableItem UnlockableItemTemplate;

    public ItemPane UnlockedItemDisplay;

    public bool MenuOpened { get { return this.Menu.activeSelf || this.TutorialMenu.activeSelf; } }

    public ScoreList ScoresLeft;

    public ScoreList ScoresRight;    

    static StartMenuManager instance;
    public static StartMenuManager Instance
    { 
        get { return instance; } 
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
        SoundManager.Instance.PlayMusic(MusicChoice.Menu);
        SerializationManager.Instance.Load();        
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
	}

    public void LoadScores()
    {
        List<int> scores = PlayerManager.Instance.HighScores;
        
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
        this.MenuInit();

        this.Menu.SetActive(true);
        this.AchievmentsTab.SetActive(false);
        this.SoundTab.SetActive(false);
        this.HighScoresTab.SetActive(false);
        this.ItemsTab.SetActive(false);
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
            default:
                this.SoundTab.SetActive(true);
                break;
        }
    }
}

public enum StartMenuTabs
{
    Sound = 0,
    Achievments = 1,
    Items = 2,
    HighScores = 3
}
