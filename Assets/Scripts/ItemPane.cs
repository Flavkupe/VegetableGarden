using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ItemPane : MonoBehaviour
{
    public int NumCols = 6;
    public float Padding = 0.8f;

    private List<GameObject> inventory = new List<GameObject>();

    public float ItemSizeXDefault = 0.64f;
    public float ItemSizeYDefault = 0.64f;

    public float ItemScaleFactor = 1.0f;

    // Use this for initialization
    void Start () 
    {        
    }
	
	// Update is called once per frame
	void Update () 
    {	
	}

    public void AddItem(GameObject item)
    {
        BoxCollider2D box = item.GetComponent<BoxCollider2D>();
        if (box != null)
        {
            box.size = new Vector2(ItemSizeXDefault, ItemSizeYDefault);
        }

        item.transform.localScale *= this.ItemScaleFactor;
        for(int i = 0; i < item.transform.childCount; ++i)
        {
            item.transform.GetChild(i).localScale *= this.ItemScaleFactor;
        }

        item.transform.SetParent(this.transform);        
        this.inventory.Add(item);        
        this.RefreshLayout();
    }

    public void RemoveItem(GameObject item)
    {
        if (item.transform.parent == this.transform)
        {
            item.transform.parent = null;
        }

        this.inventory.Remove(item);
        this.RefreshLayout();
    }

    public void RefreshLayout()
    {        
        float xOffset = 0.0f;
        float yOffset = 0.0f;        
        int currCol = 1;
        foreach (GameObject item in this.inventory)
        {            
            float sizeX = ItemSizeXDefault;
            float sizeY = ItemSizeYDefault;
            BoxCollider2D box = item.GetComponent<BoxCollider2D>();
            if (box != null)
            {
                sizeX = box.size.x;
                sizeY = box.size.y;
            }            

            item.transform.localPosition = new Vector3(xOffset, yOffset, 0.0f);
            xOffset += this.Padding;
            xOffset += (sizeX / 2.0f);
            currCol++;
            if (currCol > NumCols)
            {
                currCol = 1;
                yOffset -= this.Padding / 2.0f;
                yOffset -= (sizeY / 2.0f);
                xOffset = 0.0f;
            }
        }
    }
}
