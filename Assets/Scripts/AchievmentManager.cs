using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AchievmentManager : MonoBehaviour
{
    public AchievmentNotification Notification;

    public AchievmentIcon PunkinIcon;
    public AchievmentIcon MatoIcon;
    public AchievmentIcon CashMoneyIcon;
    public AchievmentIcon BigScoreIcon;
    public AchievmentIcon BiggerScoreIcon;
    public AchievmentIcon BiggestScoreIcon;
    public AchievmentIcon BigPocketsIcon;
    public AchievmentIcon CoffersIcon;
    public AchievmentIcon TimeToWasteIcon;
    public AchievmentIcon IrrigationStationIcon;
    public AchievmentIcon FlipFloppinIcon;
    public AchievmentIcon TiredOfWaitingIcon;
    public AchievmentIcon Boutique;
    public AchievmentIcon RockyBalboaIcon;

    private static AchievmentManager instance = null;
    
    public static AchievmentManager Instance
    {
        get { return instance; }
    }

    // Use this for initialization
    void Start () {
        instance = this;        
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void AnnounceAchievment(AchievmentIcon icon)
    {
        this.Notification.gameObject.SetActive(true);
        this.Notification.Initialize(icon);
    }
}
