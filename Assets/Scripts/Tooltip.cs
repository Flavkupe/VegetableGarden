using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class Tooltip : MonoBehaviour 
{
    private Text desc;
    private Text cooldown;
    private SetabbleText cost;

	// Use this for initialization
	void Awake() 
    {
        this.desc = this.transform.FindChild("Desc").GetComponent<Text>();
        this.cooldown = this.transform.FindChild("Cooldown").GetComponent<Text>();
        this.cost = this.transform.FindChild("CostShadow").GetComponent<SetabbleText>();
        SetVisible(false);
    }
	
	// Update is called once per frame
	void Update () 
    {
	}

    public void SetVisible(bool visible)
    {
        this.gameObject.SetActive(visible);
    }

    public void SetStats(string cost, string desc, string cooldown)
    {
        this.cost.SetText("$" + cost);
        this.cooldown.text = "Cooldown: " +  cooldown;
        this.desc.text = desc;
    }
}
