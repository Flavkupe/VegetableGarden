using System;
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
        if (!PlayerManager.Instance.HasAchievment(this.Type))
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
                amountReq = pm.AchievmentGoals.Punkin - pm.Achievments.PunkinProgress;
                break;
            case AchievmentType.Mato:
                amountReq = pm.AchievmentGoals.Mato - pm.Achievments.MatoProgress;
                break;
            case AchievmentType.Coffers:
                amountReq = pm.AchievmentGoals.Coffers - pm.Achievments.CoffersProgress;
                break;
            case AchievmentType.TimeToWaste:
                amountReq = pm.AchievmentGoals.TimeToWaste - pm.Achievments.TimeToWasteProgress;
                break;
            case AchievmentType.IrrigationStation:
                amountReq = pm.AchievmentGoals.IrrigationStation - pm.Achievments.IrrigationStationProgress;
                break;
            case AchievmentType.FlipFloppin:
                amountReq = pm.AchievmentGoals.FlipFloppin - pm.Achievments.FlipFloppinProgress;
                break;
            case AchievmentType.TiredOfWaiting:
                amountReq = (int)(pm.AchievmentGoals.TiredOfWaiting - pm.Achievments.TiredOfWaitingProgress);
                break;
            case AchievmentType.CashMoney:
                amountReq = PlayerManager.Instance.AchievmentGoals.CashMoney;
                break;
            case AchievmentType.BigPockets:
                amountReq = PlayerManager.Instance.AchievmentGoals.BigPockets;
                break;
            case AchievmentType.RockyBalboa:
                amountReq = pm.AchievmentGoals.RockyBalboa - PlayerManager.Instance.Achievments.RockyBalboaProgress;
                break;
            default:
                break;
        }

        return string.Format(LockedText, Math.Max(0, amountReq));
    }

    public void OnMouseExit()
    {
        if (this.MessageBox != null)
        {
            this.MessageBox.SetText("Earn Achievments! Get permanent bonuses!", Color.red);
        }
    }

    public void OnMouseEnter()
    {
        if (this.MessageBox != null)
        {
            if (PlayerManager.Instance.HasAchievment(this.Type))
            {
                MessageBox.SetText(this.UnlockedText, Color.green);
            }
            else
            {
                MessageBox.SetText(this.ToUnlock(), Color.black);
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
    IrrigationStation,
    FlipFloppin,
    TiredOfWaiting,
    Boutique,
    RockyBalboa
}

[Serializable]
public class AchievmentGoals
{
    public int Punkin = 700;
    public int Mato = 700;
    public int Coffers = 8000;
    public int TimeToWaste = 100;
    public int IrrigationStation = 25000;
    public int FlipFloppin = 100;
    public float TiredOfWaiting = 200000.0f;
    public int CashMoney = 2000;
    public int BigPockets = 10;
    public int RockyBalboa = 2000;
}