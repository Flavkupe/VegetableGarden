using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour {

    private List<Item> inventory = new List<Item>();
    
    public int Cash = 0;

    public float SellValueRatio = 0.25f;

    public int CurrentLevel = 0;

    public float MusicVol = 1.0f;
    public float SfxVol = 1.0f;

    public List<Item> Inventory { get { return this.inventory; } }

    // Use this for initialization
    void Awake() {
        DontDestroyOnLoad(this.transform.gameObject);
        instance = this;
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    private static PlayerManager instance = null;

    public static PlayerManager Instance
    {
        get { return instance; }
    }

    public void SellItem(Item item)
    {
        this.Cash += (int)((float)item.Cost * SellValueRatio);        
        this.inventory.Remove(item);        
    }

    public void PurchaseItem(Item item)
    {        
        this.Cash -= item.Cost;
        if (item.IsInstantUse)
        {
            this.ApplyInstantItem(item);
        }
        else
        {
            this.AddItem(item);
        }
    }

    public int GoldGainBonus = 0;
    public float FastDropMultiplierBonus = 1.0f;
    public float IrrigationDurationBonus = 0.0f;
    public float SlowTimeMultiplierBonus = 1.0f;
    public int IrrigationPointsBonus = 0;

    public int PurpleGemBonus = 0;

    private void ApplyInstantItem(Item item)
    {
        Item_PermanentEffect useItem = item as Item_PermanentEffect;
        if (useItem != null)
        {
            switch (useItem.Effect)
            {
                case EffectType.ExtraGold:
                    this.GoldGainBonus++;
                    break;
                case EffectType.FastDrop:
                    this.FastDropMultiplierBonus++;
                    break;
                case EffectType.IrrigationDuration:
                    this.IrrigationDurationBonus += 20.0f;
                    break;
                case EffectType.IrrigationPoints:
                    this.IrrigationPointsBonus++;
                    break;
                case EffectType.SlowTime:
                    this.SlowTimeMultiplierBonus *= 0.8f;
                    break;
                case EffectType.PurpleGemColorBonus:
                    this.PurpleGemBonus++;
                    break;
            }
        }
    }

    private void AddItem(Item item)
    {
        this.inventory.Add(item);
    }
}
