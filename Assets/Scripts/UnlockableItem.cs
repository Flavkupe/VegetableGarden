using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Display in menus for items which might or might not be unlocked
/// </summary>
public class UnlockableItem : MonoBehaviour
{
    public Image Background;
    public string Text;

	// Use this for initialization
	void Start () {
    }
	
	// Update is called once per frame
	void Update () {		
	}

    public void SetItem(Item item, bool unlocked)
    {        
        int diff = Mathf.Max(0, item.PointsToUnlock - PlayerManager.Instance.UniversalScore);
        this.Text = unlocked ? item.Name : string.Format("{0} points to unlock!", diff);
        if (unlocked)
        {
            Vector2 delta = this.Background.rectTransform.sizeDelta;
            this.Background.rectTransform.sizeDelta = new Vector2(800, delta.y);
        }
    }

    public void HideTextFromUI()
    {
        if (StartMenuManager.Instance != null)
        {
            StartMenuManager.Instance.HideItemText();
        }
    }

    public void ShowTextInUI()
    {
        if (StartMenuManager.Instance != null)
        {
            StartMenuManager.Instance.ShowItemText(this.Text);
        }
    }
}
