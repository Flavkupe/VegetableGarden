﻿using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class SetabbleText : MonoBehaviour 
{
    private Text textObject;
    private Text shadowTextObject;

    private void InitializeTextObjects()
    {
        textObject = this.GetComponent<Text>();
        shadowTextObject = this.gameObject.transform.GetChild(0).GetComponent<Text>();
    }

	void Start () 
    {
        this.InitializeTextObjects();
	}
	
	void Update () 
    {
	
	}

    public void SetText(string score)
    {
        if (textObject != null)
        {
            textObject.text = score;
        }

        if (shadowTextObject != null)
        {
            shadowTextObject.text = score;
        }
    }
}
