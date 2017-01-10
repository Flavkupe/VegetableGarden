using UnityEngine;
using System.Collections;
using System;

public class LevelGoalSign : MonoBehaviour 
{

    public TextOverSprite GoalText;
    public TextOverSprite TimeText;

    public float MovementSpeed = 5.0f;    
    public float LingerTime = 1.0f;

    private float currentLingerTime = 1.0f;    
    private float currentMovementSpeed = 5.0f;

    public float CurrentLingerTime
    {
        get { return currentLingerTime; }
        set { currentLingerTime = value; }
    }

    public float CurrentMovementSpeed
    {
        get { return currentMovementSpeed; }
        set { currentMovementSpeed = value; }
    }

    private bool animating = false;

    public bool Animating
    {
        get { return animating; }
    }

    public event EventHandler DoneAnimating;

	// Use this for initialization
	void Start () 
    {
	
	}
	
	// Update is called once per frame
	void Update () 
    {	
	}

    public IEnumerator Animate()
    {
        this.currentMovementSpeed = this.MovementSpeed;
        this.currentLingerTime = this.LingerTime;

        this.animating = true;
        Vector3 top = new Vector3(0.0f, 7.0f, 0.0f);
        Vector3 center = new Vector3(0.0f, 0.0f, 0.0f);        

        this.transform.localPosition = top;

        while (Vector3.Distance(this.transform.localPosition, center) > 0.1f)
        {
            this.transform.localPosition = Vector3.MoveTowards(this.transform.localPosition, center, Time.deltaTime * this.currentMovementSpeed);
            yield return null;
        }

        while (this.currentLingerTime > 0.0f)
        {
            this.currentLingerTime -= Time.deltaTime;
            yield return null;
        }

        while (Vector3.Distance(this.transform.localPosition, top) > 0.1f)
        {
            this.transform.localPosition = Vector3.MoveTowards(this.transform.localPosition, top, Time.deltaTime * this.currentMovementSpeed);
            yield return null;
        }

        this.animating = false;

        if (DoneAnimating != null)
        {
            DoneAnimating(this, new EventArgs());
        }
    }

    public void SetGoal(LevelGoal goal)
    {
        this.GoalText.SetText("Get " + goal.ScoreGoal + " points!");
        this.TimeText.SetText("Time: " + goal.Time + " seconds!");
    }

    public void SetSuccess()
    {
        this.GoalText.SetText("Success!");
        this.TimeText.SetText(string.Empty);
    }

    public void SetLoss(int score)
    {
        this.GoalText.SetText("Oh no! Not this time.");
        this.TimeText.SetText("Total score: " + score);
    }
}
