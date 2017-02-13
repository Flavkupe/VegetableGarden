using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;

public class ShopManager : MonoBehaviour {

    public ShopItem[] ShopItems;

    public Tooltip Tooltip;

    public SetabbleText MoneyText;

    public ItemPane SellPane;
    public GameObject SellSign;

    public ShopItem ShopItemTemplate;

    public FloatyText FloatyText;

    private static ShopManager instance;

    public Loading Loading; 

    public static ShopManager Instance
    {
        get { return instance; }
    }

    // Use this for initialization
    void Awake ()
    {
        if (PlayerManager.Instance == null)
        {
            SceneManager.LoadScene("StartMenu");
            return;
        }

        instance = this;
        LoadItemsFromResources();
        LoadPanel();
        this.RefreshMoneySign();
    }

    private void RefreshMoneySign()
    {
        if (this.MoneyText != null)
        {
            this.MoneyText.SetText("$" + PlayerManager.Instance.Cash);
        }
    }
	
    void Start()
    {        
        SoundManager.Instance.PlayMusic(MusicChoice.Shop);
    }

	// Update is called once per frame
	void Update () {
    }

    public void ContinueGame()
    {
        SerializationManager.Instance.Save();
        SceneManager.LoadSceneAsync("Main", LoadSceneMode.Single);
    }    

    private void LoadItemsFromResources()
    {
        List<GameObject> objects = PlayerManager.Instance.GetAllAvailableShopItems();
        foreach (ShopItem shopItem in this.ShopItems)
        {
            if (objects.Count == 0)
            {
                shopItem.gameObject.SetActive(false);

            }
            else
            {
                shopItem.gameObject.SetActive(true);
                GameObject instance = objects.GetRandom();
                objects.Remove(instance);
                shopItem.InitializeItem(instance.GetComponent<Item>(), false);
            }
        }
    }

    private void LoadPanel()
    {
        if (this.SellPane != null)
        {
            if (PlayerManager.Instance.Inventory.Count == 0)
            {
                this.SellSign.SetActive(false);
                this.SellPane.gameObject.SetActive(false);
                return;
            }

            foreach (Item item in PlayerManager.Instance.Inventory)
            {
                ShopItem shopItem = Instantiate(this.ShopItemTemplate);
                shopItem.gameObject.SetActive(true);
                shopItem.transform.localScale = shopItem.transform.localScale *= 0.8f;
                shopItem.InitializeItem(item, true);
                this.SellPane.AddItem(shopItem.gameObject);
            }
        }
    }    

    public void BuyOrSellItem(ShopItem item)
    {
        if (item == null || item.BackingItem == null)
        {
            return;
        }

        bool transacted = false;
        if (item.Owned)
        {
            // SELL
            SoundManager.Instance.PlaySound(SoundEffects.Kachink);
            PlayerManager.Instance.SellItem(item.BackingItem);
            item.gameObject.SetActive(false);
            transacted = true;
        }
        else
        {
            // BUY
            int cost = PlayerManager.Instance.GetTrueItemCost(item.BackingItem);
            if (PlayerManager.Instance.Cash >= cost)
            {
                SoundManager.Instance.PlaySound(SoundEffects.Kachink);
                PlayerManager.Instance.PurchaseItem(item.BackingItem);
                item.gameObject.SetActive(false);
                transacted = true;
            }
            else
            {
                SoundManager.Instance.PlaySound(SoundEffects.Error);
                transacted = false;
            }
        }

        if (transacted)
        {
            string transactionStr = (item.Owned ? "+" : "-") + "$" + item.GetTransactionPrice();
            FloatyText text = GameUtils.GenerateFloatyTextAt(transactionStr, item.transform.position.x, item.transform.position.y,
                this.FloatyText, null, Color.yellow);
            this.Tooltip.gameObject.SetActive(false);
        }

        this.RefreshMoneySign();
    }
}
