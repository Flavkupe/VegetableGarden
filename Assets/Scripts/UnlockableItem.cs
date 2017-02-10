using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Display in menus for items which might or might not be unlocked
/// </summary>
public class UnlockableItem : MonoBehaviour {

    private Item BackingItem;

    public Text Text;

	// Use this for initialization
	void Start () {
    }
	
	// Update is called once per frame
	void Update () {		
	}

    public void SetItem(Item item, bool unlocked)
    {
        this.BackingItem = item;
        int diff = item.PointsToUnlock - PlayerManager.Instance.UniversalScore;
        this.Text.text = unlocked ? item.Name : string.Format("{0} points to unlock!", diff);
    }
}
