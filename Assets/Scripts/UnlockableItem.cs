using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Display in menus for items which might or might not be unlocked
/// </summary>
public class UnlockableItem : MonoBehaviour {

    private Item BackingItem;

    public Image Background;
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
        int diff = Mathf.Max(0, item.PointsToUnlock - PlayerManager.Instance.UniversalScore);
        this.Text.text = unlocked ? item.Name : string.Format("{0} points to unlock!", diff);
        if (unlocked)
        {
            Vector2 delta = this.Background.rectTransform.sizeDelta;
            this.Background.rectTransform.sizeDelta = new Vector2(800, delta.y);
        }
    }
}
