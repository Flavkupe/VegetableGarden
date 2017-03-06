using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialManager : MonoBehaviour {

    public Tutorial[] Pages;    

    public GameObject TutorialMenu;

    public SetabbleText Textbox;
    public RectTransform Frame;

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
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
