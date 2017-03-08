using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class TutorialManager : Singleton<TutorialManager>
{
    public Tutorial[] Pages;

    public LevelTutorial[] LevelTutorials;

    public GameObject TutorialMenu;

    public Image TutorialImage;

    public SetabbleText Textbox;
    public RectTransform Frame;

    public Animator TutorialAnimator;

    private int currentPage = 0;

    public void GoToPage(int page)
    {
        if (!this.TutorialMenu.gameObject.activeSelf)
        {
            this.TutorialMenu.gameObject.SetActive(true);
        }        

        foreach (Tutorial tut in Pages)
        {
            tut.gameObject.SetActive(false);
        }

        if (Pages.Length > page)
        {            
            Tutorial tutorial = Pages[page];
            tutorial.gameObject.SetActive(true);
            currentPage = page;
            tutorial.InitializeTutorial();
            Textbox.SetText(tutorial.Text);
            Frame.localPosition = new Vector3(Frame.localPosition.x, tutorial.BorderPosY);
            Frame.sizeDelta = new Vector3(Frame.sizeDelta.x, tutorial.BorderHeight);
        }
    }

    public void NextPage()
    {
        currentPage++;
        if (currentPage == Pages.Length)
        {
            currentPage = 0;
        }

        GoToPage(currentPage);
    }

    public void PrevPage()
    {
        currentPage--;
        if (currentPage < 0)
        {
            currentPage = Pages.Length - 1;
        }

        GoToPage(currentPage);
    }

        // Use this for initialization
    void Start () {
        instance = this;
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void ShowCurrentLevelTutorial()
    {
        int currentLevel = PlayerManager.Instance.CurrentLevel;
        if (currentLevel >= PlayerManager.Instance.MaxLevel)
        {
            LevelTutorial tutorial = this.LevelTutorials.ToList().FirstOrDefault(a => a.LevelOfTutorial == PlayerManager.Instance.CurrentLevel);
            if (tutorial != null)
            {
                this.TutorialMenu.SetActive(true);
                this.TutorialAnimator.Play("Open");
                this.Textbox.SetText(tutorial.TitleText);
                this.TutorialImage.sprite = tutorial.TutorialPicture;
            }
        }
    }

    public void CloseTutorialWindow()
    {
        this.TutorialAnimator.Play("Close");
    }    
}

[Serializable]
public class LevelTutorial
{
    public Sprite TutorialPicture;

    public string TitleText;

    // Which level the tutorial appears in
    public int LevelOfTutorial = 0;
}