using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;

public class StepperButtons : MonoBehaviour {

	public float step = 0.1f;
    public GameObject stepperView;
    public GameObject panel;

    public float value = 0;
    
    void Start()
    {
        updateValue();
    }

	public void Increase(){
        value += step;
        if (value > 1)
            value = 1;
        updateValue();
	}


    public void Decrease(){
        value -= step;
        if (value<0)
            value = 0;
        updateValue();
	}
    public void updateValue()
    {
        if (!panel || !stepperView)
            return;
        float panelWidth = panel.transform.parent.GetComponent<RectTransform>().rect.width;
        float stepperWidth = stepperView.GetComponent<RectTransform>().rect.width;
        int amount = (int)(panelWidth / stepperWidth);
        float padding = (panelWidth - stepperWidth * amount)/2+stepperWidth/2;
        foreach (Transform child in panel.transform)
        {
            Destroy(child.gameObject);
        }
        int needAmount = (int)(amount * value);
        for (int i = 0; i < needAmount; i++)
        {
            GameObject step = GameObject.Instantiate(stepperView);
            step.SetActive(true);
            step.transform.SetParent(panel.transform, false);
            step.transform.localPosition = new Vector3(padding + i * stepperWidth, 0);
        }

    }
}
