using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShopItem : MonoBehaviour
{

    public Item BackingItem;

    public Button Button;

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void InitializeItem(Item item)
    {
        this.BackingItem = item;
        this.Button.image.sprite = item.GetComponent<SpriteRenderer>().sprite;
    }

    public void OnHoverIn()
    {
        if (this.BackingItem != null)
        {
            ShopManager.Instance.Tooltip.SetStats(this.BackingItem.Cost.ToString(),
                this.BackingItem.Description, ((int)this.BackingItem.Cooldown).ToString());
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