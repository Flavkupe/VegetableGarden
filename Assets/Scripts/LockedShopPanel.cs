using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LockedShopPanel : MonoBehaviour {

    public GameObject LockedPanel;
    public GameObject UnlockedPanel;

    public ShopItem[] ShopItems;

    public Text Title;

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void TogglePane(bool locked)
    {
        if (locked)
        {
            this.LockedPanel.gameObject.SetActive(true);
            this.UnlockedPanel.gameObject.SetActive(false);
            this.Title.text = "Locked!";
        }
        else
        {
            this.LockedPanel.gameObject.SetActive(false);
            this.UnlockedPanel.gameObject.SetActive(true);
            this.Title.text = "Essentials";
        }
    }
}
