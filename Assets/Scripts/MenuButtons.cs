using UnityEngine;
using System.Collections;

public class MenuButtons : MonoBehaviour {

    public MenuButtonChoices Choice;

	// Use this for initialization
	void Start () {	    
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    void OnMouseDown()
    {
        if (StartMenuManager.Instance.MenuOpened)
        {
            return;
        }

        if (this.Choice == MenuButtonChoices.Start)
        {
            StartMenuManager.Instance.OnStartNormalGamePressed();
        }
        else if (this.Choice == MenuButtonChoices.Quit)
        {
            Application.Quit();
        }
    }
}

public enum MenuButtonChoices
{
    Start,
    Quit
}
