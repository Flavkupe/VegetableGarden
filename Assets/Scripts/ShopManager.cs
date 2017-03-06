using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;
using UnityEngine.UI;

public class ShopManager : MonoBehaviour {

    public int ItemLimit = 10;

    public GameObject SellPane;
    public GameObject BuyPane;

    public ShopItem[] ShopItems;

    public Tooltip TooltipR;
    public Tooltip TooltipL;

    public SetabbleText BuySellText;

    public SetabbleText MoneyText;

    public ItemPane ItemPane;    

    public LockedShopPanel EssentialsPanel;

    public ShopItem ShopItemTemplate;

    public FloatyText FloatyText;

    private static ShopManager instance;

    public Loading Loading;

    private bool sellMode = false;

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
        PopulateShelves();
        LoadOrRefreshItemPanel();
        this.RefreshMoneySign();
    }

    public void ToggleSellMode()
    {
        this.sellMode = !this.sellMode;
        if (this.sellMode)
        {
            this.BuyPane.gameObject.SetActive(false);
            this.SellPane.gameObject.SetActive(true);
            this.BuySellText.SetText("Buy");
        }
        else
        {
            this.BuyPane.gameObject.SetActive(true);
            this.SellPane.gameObject.SetActive(false);
            this.BuySellText.SetText("Sell");
        }
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

    private void PopulateShelves()
    {
        List<Item> items = PlayerManager.Instance.GetAllAvailableShopItems();

        this.EssentialsPanel.TogglePane(!PlayerManager.Instance.Achievments.Boutique);

        if (this.EssentialsPanel.UnlockedPanel.activeSelf && PlayerManager.Instance.Achievments.Boutique)
        {
            foreach (ShopItem shopItem in this.EssentialsPanel.ShopItems)
            {
                Queue<Item> essentialItems = new Queue<Item>(items.Select(a => a.GetComponent<Item>()).Where(b => b.IsEssentialItem).ToList());
                if (essentialItems.Count == 0)
                {
                    shopItem.gameObject.SetActive(false);
                }
                else
                {
                    Item item = essentialItems.Dequeue();
                    items.Remove(item);                    
                    shopItem.InitializeItem(item, false);
                }
            }
        }

        foreach (ShopItem shopItem in this.ShopItems)
        {
            if (items.Count == 0)
            {
                shopItem.gameObject.SetActive(false);

            }
            else
            {
                shopItem.gameObject.SetActive(true);
                Item item = items.GetRandom();
                items.Remove(item);                
                shopItem.InitializeItem(item, false);
            }
        }
    }

    private void LoadOrRefreshItemPanel()
    {
        if (this.SellPane != null)
        {
            if (PlayerManager.Instance.Inventory.Count == 0)
            {                
                return;
            }

            this.ItemPane.ClearList();

            foreach (Item item in PlayerManager.Instance.Inventory)
            {
                ShopItem shopItem = Instantiate(this.ShopItemTemplate);
                shopItem.gameObject.SetActive(true);
                shopItem.Tooltip = this.TooltipR;
                shopItem.transform.localScale = shopItem.transform.localScale *= 0.8f;
                shopItem.InitializeItem(item, true);
                this.ItemPane.AddItem(shopItem.gameObject);
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

            if (!item.BackingItem.IsInstantUse && PlayerManager.Instance.Inventory.Count >= this.ItemLimit)
            {
                FloatyText text = GameUtils.GenerateFloatyTextAt("Cannot carry more items!\nSell some items!",
                    item.transform.position.x, item.transform.position.y, this.FloatyText, null, Color.red, TextAnchor.UpperLeft, 12);
                SoundManager.Instance.PlaySound(SoundEffects.Error);                
                transacted = false;
            }
            else if (PlayerManager.Instance.Cash >= cost)
            {
                SoundManager.Instance.PlaySound(SoundEffects.Kachink);
                PlayerManager.Instance.PurchaseItem(item.BackingItem);
                item.gameObject.SetActive(false);
                this.LoadOrRefreshItemPanel();
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
            this.TooltipR.gameObject.SetActive(false);
            this.TooltipL.gameObject.SetActive(false);            
        }

        this.RefreshMoneySign();
    }
}
