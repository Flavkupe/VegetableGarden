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
    Purple, // Eggplant

    Half // Any gems (half of board)
}

public class Gem : MonoBehaviour 
{
    public GemType GemType = GemType.White;
    public GemColor GemColor = GemColor.Brown;

    public GameObject Sparkles;

    public GemGrid Grid;

    public int GemId;

    public int GridX;
    public int GridY;    

    public Transform Hover;
    public Transform Frame;

    public bool InTransition = false;

    private GameObject sparkles = null;

    private CooldownTimer GlowTimer;

    public bool IsGlowing {  get { return !this.GlowTimer.IsExpired; } }

    private bool mouseReleased = false;

    private Vector3? destination = null;

	// Use this for initialization
	void Start () 
    {
        this.GlowTimer = new CooldownTimer(GameManager.Instance.GetTotalGlowDuration(), false);
        this.sparkles = Instantiate(GameManager.Instance.Sparkles);
        this.sparkles.transform.position = new Vector3(this.transform.position.x, this.transform.position.y, -1);
        this.sparkles.transform.parent = this.transform;
        this.sparkles.SetActive(false);
	}
	
	// Update is called once per frame
	void Update () 
    {
        mouseReleased = Input.GetMouseButtonUp(0);

        if (this.GlowTimer.Tick(Time.deltaTime).IsExpired)
        {
            // If glow expires
            this.sparkles.SetActive(false);
        }        

        if (this.InTransition)
        {
            Debug.Assert(destination != null, "destination cannot be null if in transition");
            if (!this.transform.localPosition.IsNear(destination.Value))
            {
                this.transform.localPosition = Vector3.MoveTowards(this.transform.localPosition, destination.Value, 
                    Time.deltaTime * GameManager.Instance.Grid.GetTotalSlideSpeed());                
            }
            else
            {            
                // snap into place once near enough
                this.transform.localPosition = destination.Value;
                this.InTransition = false;
                destination = null;
            }
        }
    }

    public void SlideGemTo(Vector3 destination)
    {
        this.destination = destination;
        this.InTransition = true;
        
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
        if (GameManager.Instance.IsPaused)
        {
            return;
        }

        if (!this.Grid.CanMakeMove() || this.Grid.SoonAfterMatch())
        {
            // Irrigate on either condition, but do not continue if move is disallowed
            this.GlowTimer.Reset();            
            this.sparkles.SetActive(true);

            if (!this.Grid.CanMakeMove())
            {
                return;
            }
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

    void OnMouseOver()
    {
        if (GameManager.Instance.IsPaused)
        {
            return;
        }

        if (mouseReleased)
        {
            this.Grid.TrySwapWith(this.Grid.Selected, this);            
        }
    }

    void OnMouseEnter()
    {
        if (GameManager.Instance.IsPaused)
        {
            return;
        }

        this.Hover.gameObject.SetActive(true);
    }

    void OnMouseExit()
    {
        if (GameManager.Instance.IsPaused)
        {
            return;
        }

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
