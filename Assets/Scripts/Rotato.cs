using UnityEngine;
using System.Collections;

public class Rotato : MonoBehaviour {

    public float MaxAngle = 10.0f;
    public float RotatoSpeed = 2.0f;
    private float time = 0.0f;    

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () 
    {
        time += Time.deltaTime;
        float phase = Mathf.Sin(time / RotatoSpeed);
        transform.localRotation = Quaternion.Euler(new Vector3(0, 0, phase * MaxAngle));        
	}
}
