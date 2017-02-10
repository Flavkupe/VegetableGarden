using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Loading : MonoBehaviour {

    public GameObject Spinner;
    public Image Tint;
    public float SpinSpeed = 10.0f;
    public float DimSpeed = 10.0f;
    public float MaxOpacity = 200;

	// Use this for initialization
	void Start ()
    {		
	}
	
	// Update is called once per frame
	void Update ()
    {
		
	}

    public void Tick()
    {
        if (Spinner != null)
        {
            Spinner.transform.Rotate(0, 0, Time.deltaTime * SpinSpeed);
        }

        if (Tint != null)
        {
            if (Tint.color.a < MaxOpacity)
            {
                Color c = Tint.color;
                Tint.color = new Color(c.r, c.g, c.b, c.a + Time.deltaTime * DimSpeed);
            }
        }
    }
}
