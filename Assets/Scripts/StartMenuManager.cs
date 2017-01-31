using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Linq;
using System;

public class StartMenuManager : MonoBehaviour 
{
    public GameObject HighScoreMenu;

    public GameObject GameModeMenu;

    public bool MenuOpened { get { return this.HighScoreMenu.activeSelf; } }

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
	
	// Update is called once per frame
	void Update ()
    {
	}

    public void OnHighScoresButtonPressed(bool open)
    {
        List<int> scores = PlayerManager.Instance.HighScores;

        this.HighScoreMenu.SetActive(open);
        if (open)
        {            
            int leftRange = Mathf.Min(scores.Count, this.ScoresLeft.Rows);
            this.ScoresLeft.SetScores(scores.GetRange(0, leftRange));
            if (scores.Count > this.ScoresLeft.Rows)
            {
                int rightRange = Mathf.Min(this.ScoresRight.Rows, scores.Count - this.ScoresLeft.Rows);
                this.ScoresRight.SetScores(scores.GetRange(this.ScoresLeft.Rows, rightRange));
            }
        }
    }

    private void StartGame(GameMode mode)
    {
        PlayerManager.Instance.GameMode = mode;
        SceneManager.LoadScene("Main", LoadSceneMode.Single);
    }

    public void OnStartNormalGamePressed()
    {
        this.StartGame(GameMode.Normal);
    }

    public void OnStartCasualGamePressed()
    {
        this.StartGame(GameMode.Casual);
    }
}
