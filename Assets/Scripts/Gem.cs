using UnityEngine;
using System.Collections;
using System;

public enum GemType
{
    White,
    Red, 
    Orange,
    Yellow,
    Green,
    Blue,
    Purple,

    Tomato,
    Pumpkin,
    Carrot,
    Onion,
    Eggplant,
    Potato,
    Beet,
    Broccoli,
    GreenPepper,
    Cucumber,
}

public enum GemColor
{
    Green, // Brolli, Cucumber, GreenPepper
    Orange, // Punkin and Carrot
    Brown, // Onion, Potato
    Red, // Beet, Tomato
    Purple // Eggplant
}

public class Gem : MonoBehaviour 
{
    public GemType GemType = GemType.White;
    public GemColor GemColor = GemColor.Brown;

    public GameObject Sparkles;

    public GemGrid Grid;

    public int GridX;
    public int GridY;    

    public Transform Hover;
    public Transform Frame;

    private float glowing = 0.0f;

    public bool InTransition = false;

    private GameObject sparkles = null;

    public bool IsGlowing {  get { return this.glowing > 0.0f; } }

	// Use this for initialization
	void Start () 
    {
        this.sparkles = Instantiate(GameManager.Instance.Sparkles);
        this.sparkles.transform.position = new Vector3(this.transform.position.x, this.transform.position.y, -1);
        this.sparkles.transform.parent = this.transform;
        this.sparkles.SetActive(false);
	}
	
	// Update is called once per frame
	void Update () 
    {
        if (glowing > 0.0f)
        {
            glowing -= Time.deltaTime;
            if (glowing <= 0.0f)
            {
                this.sparkles.SetActive(false);
            }
        }
	}

    public void SetSelected(bool selected)
    {
        if (selected)
        {
            this.Grid.Selected = this;
        }

        this.Grid.Selected.Frame.gameObject.SetActive(selected);
    }

    public void SetGridLoc(int x, int y)
    {
        this.GridX = x;
        this.GridY = y;
    }

    public bool IsMatch(Gem other)
    {
        if (other == null || other.GemType != this.GemType)
        {
            return false;
        }

        if (other == this)
        {
            return false;
        }

        return true;
    }

    public bool IsColorMatch(Gem other)
    {
        if (other == null || other.GemColor != this.GemColor)
        {
            return false;
        }

        if (other == this)
        {
            return false;
        }

        return true;
    }

    void OnMouseDown() 
    {
        if (!this.Grid.CanMakeMove())
        {
            this.glowing = GameManager.Instance.GlowDuration;
            this.sparkles.SetActive(true); 
            return;
        }

        if (!this.Grid.TrySwapWith(this.Grid.Selected, this))
        {
            if (this.Grid.Selected != null)
            {
                this.Grid.Selected.Frame.gameObject.SetActive(false);
            }

            this.SetSelected(true);
        }
    }

    void OnMouseEnter()
    {
        this.Hover.gameObject.SetActive(true);
    }

    void OnMouseExit()
    {
        this.Hover.gameObject.SetActive(false);
    }

    public IEnumerator Vanish()
    {
        while (true)
        {            
            if (this.gameObject != null)
            {
                Vector3 t = this.transform.localScale;
                this.transform.localScale = new Vector3(t.x - 0.1f, t.y - 0.1f, 1);
                this.transform.Rotate(0.0f, 0.0f, 36.0f);

                if (this.transform.localScale.x <= 0.0f)
                {
                    Destroy(this.gameObject);
                    break;
                }
            }

            yield return null;
        }

        yield return null;
        
    }
}
