using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ItemPane : MonoBehaviour {

    private List<Item> inventory = new List<Item>();    

	// Use this for initialization
	void Start () 
    {	
	}
	
	// Update is called once per frame
	void Update () 
    {	
	}

    public void AddItem(Item item)
    {
        item.transform.SetParent(this.transform);
        this.inventory.Add(item);        
        this.RefreshLayout();
    }

    public void RemoveItem(Item item)
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
        float xOffset = -1.65f;
        float padding = 0.3f;
        foreach (Item item in this.inventory)
        {
            item.transform.localPosition = new Vector3(xOffset, 0.0f, 0.0f);
            xOffset += padding;
            xOffset += (item.GetComponent<BoxCollider2D>().size.x / 2.0f);
        }
    }
}
