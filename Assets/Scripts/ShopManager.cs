using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;

public class ShopManager : MonoBehaviour {

    public ShopItem[] ShopItems;

    public Tooltip Tooltip;

    public SetabbleText MoneyText;

    private static ShopManager instance;
    
    public static ShopManager Instance
    {
        get { return instance; }
    }

    // Use this for initialization
    void Awake () {
        if (PlayerManager.Instance == null)
        {
            SceneManager.LoadScene("StartMenu");
            return;
        }

        instance = this;
        LoadItemsFromResources();
        if (this.MoneyText != null)
        {
            this.MoneyText.SetText("$" + PlayerManager.Instance.Cash);
        }
    }
	
	// Update is called once per frame
	void Update () {
        //ShopManager.Instance.Tooltip.SetVisible(false);
    }

    public void ContinueGame()
    {
        SceneManager.LoadScene("Main", LoadSceneMode.Single);
    }

    private void LoadItemsFromResources()
    {
        List<GameObject> objects = Resources.LoadAll<GameObject>("Prefabs/Items").ToList();
        objects = objects.Where(a => !PlayerManager.Instance.Inventory.Any(b => b.name == a.name)).ToList();
        foreach (ShopItem shopItem in this.ShopItems)
        {
            GameObject instance = objects.GetRandom();
            objects.Remove(instance);
            shopItem.InitializeItem(instance.GetComponent<Item>());
            if (objects.Count == 0)
            {
                break;
            }
        }
    }

    public void BuyItem(ShopItem item)
    {
        if (item != null && item.BackingItem != null && 
            PlayerManager.Instance.Cash >= item.BackingItem.Cost)
        {
            SoundManager.Instance.PlaySound(SoundEffects.Kachink);
            PlayerManager.Instance.PurchaseItem(item.BackingItem);
            item.gameObject.SetActive(false);
            if (this.MoneyText != null)
            {
                this.MoneyText.SetText("$" + PlayerManager.Instance.Cash);
            }
        }
        else
        {
            SoundManager.Instance.PlaySound(SoundEffects.Error);
        }
    }
}
