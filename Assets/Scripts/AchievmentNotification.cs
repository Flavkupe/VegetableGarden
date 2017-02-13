using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AchievmentNotification : MonoBehaviour {

    private AchievmentIcon data;

    public SetabbleText Text;

    public Image Icon;

    private CooldownTimer timer = new CooldownTimer(4.0f, false);

    // Use this for initialization
    void Start () {
        
	}

    public void Initialize(AchievmentIcon data)
    {
        this.data = data;
        this.timer.Reset();
        this.Text.SetText(string.Format("{0} unlocked!", data.Name));
        this.Icon.sprite = data.GetComponent<Image>().sprite;
        this.gameObject.SetActive(true);
    }

    // Update is called once per frame
    void Update () {
        if (timer.Tick(Time.deltaTime).IsExpired)
        {
            this.gameObject.SetActive(false);
        }
    }
}
