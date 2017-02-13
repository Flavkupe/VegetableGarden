using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AchievmentIcon : MonoBehaviour {

    public string Name;

    public AchievmentType Type;

    public SetabbleText MessageBox;

    public string UnlockedText;

    public string LockedText;

    // Use this for initialization
    void Start() {
        if (!this.IsUnlocked())
        {
            this.GetComponent<Image>().color = Color.black;
        }
    }

    // Update is called once per frame
    void Update() {

    }

    private string ToUnlock()
    {
        PlayerManager pm = PlayerManager.Instance;
        int amountReq = 0;
        switch (Type)
        {
            case AchievmentType.Punkin:
                amountReq = pm.Achievments.PunkinProgress;
                break;
            case AchievmentType.Mato:
                amountReq = pm.Achievments.MatoProgress;
                break;
            case AchievmentType.Coffers:
                amountReq = pm.Achievments.CoffersProgress;
                break;
            case AchievmentType.TimeToWaste:
                amountReq = pm.Achievments.TimeToWasteProgress;
                break;
            case AchievmentType.IrrigationStation:
                amountReq = pm.Achievments.IrrigationStationProgress;
                break;
            default:
                break;
        }

        return string.Format(LockedText, amountReq);
    }

    private bool IsUnlocked()
    {
        PlayerManager pm = PlayerManager.Instance;
        switch (Type)
        {
            case AchievmentType.Punkin:
                return pm.Achievments.PunkinProgress == 0; ;
            case AchievmentType.Mato:
                return pm.Achievments.MatoProgress == 0; ;
            case AchievmentType.CashMoney:
                return pm.Achievments.CashMoney;
            case AchievmentType.BigScore:
                return pm.Achievments.BigScore;
            case AchievmentType.BiggerScore:
                return pm.Achievments.BiggerScore;
            case AchievmentType.BiggestScore:
                return pm.Achievments.BiggestScore;
            case AchievmentType.BigPockets:
                return pm.Achievments.BigPockets;
            case AchievmentType.Coffers:
                return pm.Achievments.CoffersProgress == 0; ;
            case AchievmentType.TimeToWaste:
                return pm.Achievments.TimeToWasteProgress == 0;
            case AchievmentType.IrrigationStation:
                return pm.Achievments.IrrigationStationProgress == 0; ;
            default:
                return false;
        }
    }

    public void OnMouseExit()
    {
        if (this.MessageBox != null)
        {
            this.MessageBox.SetText("Earn Achievments! Get permanent bonuses!");
        }
    }

    public void OnMouseEnter()
    {
        if (this.MessageBox != null)
        {
            if (this.IsUnlocked())
            {
                MessageBox.SetText(this.UnlockedText);
            }
            else
            {
                MessageBox.SetText(this.ToUnlock());
            }
        }
    }
}

public enum AchievmentType
{
    Punkin,
    Mato,
    CashMoney,
    BigScore,
    BiggerScore,
    BiggestScore,
    BigPockets,
    Coffers,
    TimeToWaste,
    IrrigationStation
}
