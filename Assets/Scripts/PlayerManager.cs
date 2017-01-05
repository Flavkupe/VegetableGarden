using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour {

    private List<Item> inventory = new List<Item>();

    private int cash = 0;
    private int currentLevel = 0;

    public int Cash;

    public int CurrentLevel;

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

    public void AddItem(Item item)
    {
        this.inventory.Add(item);
    }

    public void PurchaseItem(Item item)
    {
        //GameManager.Instance.PlaySound(SoundEffects.Kachink);
        this.Cash -= item.Cost;
        this.AddItem(item);
    }
}
