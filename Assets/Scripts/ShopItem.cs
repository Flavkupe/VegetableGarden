using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShopItem : MonoBehaviour
{
    public Item BackingItem;

    public Button Button;

    public bool Owned = false;

    // Use this for initialization
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
    }

    public int GetTransactionPrice()
    {
        if (this.BackingItem == null)
        {
            return 0;
        }

        if (!this.Owned)
        {
            // Buy price
            return this.BackingItem.Cost;
        }
        else
        {
            // Sell price
            return (int)((float)this.BackingItem.Cost * PlayerManager.Instance.SellValueRatio);
        }
    }

    public void InitializeItem(Item item, bool owned)
    {
        this.Owned = owned;
        this.BackingItem = item;
        this.Button.image.sprite = item.GetComponent<SpriteRenderer>().sprite;
    }

    public void OnHoverIn()
    {
        if (this.BackingItem != null)
        {
            string cost = this.GetTransactionPrice().ToString();
            ShopManager.Instance.Tooltip.SetStats(cost, this.BackingItem.Description, 
                ((int)this.BackingItem.Cooldown).ToString());
            ShopManager.Instance.Tooltip.SetVisible(true);
        }
    }

    public void OnHoverOut()
    {
        if (this.BackingItem != null)
        {
            ShopManager.Instance.Tooltip.SetVisible(false);
        }
    }
}