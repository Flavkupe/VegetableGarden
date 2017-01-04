﻿using UnityEngine;
using System.Collections;

public class FloatyText : MonoBehaviour
{
    private TextMesh textObject;
    private TextMesh shadowTextObject;
    public float MoveSpeed = 1;

    void InitComponents()
    {
    }

	void Start () 
    {
        textObject = this.GetComponent<TextMesh>();
        shadowTextObject = this.gameObject.transform.GetChild(0).GetComponent<TextMesh>();
        GameObject.Destroy(this.gameObject, 1.0f);        
	}
		
	void Update () 
    {
        float newY = this.transform.localPosition.y + (Time.deltaTime * MoveSpeed);
        this.transform.localPosition = new Vector3(this.transform.localPosition.x, newY, this.transform.localPosition.z);
	}

    public void SetText(string score, Color? color = null)
    {
        textObject = this.GetComponent<TextMesh>();
        shadowTextObject = this.gameObject.transform.GetChild(0).GetComponent<TextMesh>();

        if (textObject != null)
        {
            textObject.text = score;
            if (color != null)
            {
                textObject.color = color.Value;
            }
        }

        if (shadowTextObject != null)
        {
            shadowTextObject.text = score;
        }       
    }
}