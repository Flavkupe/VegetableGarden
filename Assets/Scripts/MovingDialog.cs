using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingDialog : MonoBehaviour {

    public void CloseComplete()
    {
        if (this.transform.parent != null)
        {
            this.transform.parent.gameObject.SetActive(false);
        }
        else
        {
            this.gameObject.SetActive(false);
        }
    }

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
