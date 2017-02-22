using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScoreList : MonoBehaviour {

    public int Rows = 5;

    public int StartingNum = 1;

    public Text Textbox;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void SetScores(List<string> values)
    {        
        int current = 0;
        if (this.Textbox != null)
        {
            this.Textbox.text = string.Empty;
            foreach (string value in values)
            {
                this.Textbox.text += (current + this.StartingNum) + ") " + value + "\n";
                current++;
                if (current > this.Rows)
                {
                    return;
                }
            }
        }
    }
}
