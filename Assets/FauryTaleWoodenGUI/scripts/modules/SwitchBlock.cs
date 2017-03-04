using UnityEngine;
using System.Collections;

public class SwitchBlock : MonoBehaviour {

    public GameObject stateOn;
    public GameObject stateOff;
	public ParticleSystem particle;

    public bool enable {
        get {
            return enable;
        }
        set {
			stateOn.SetActive(value);
            stateOff.SetActive(!value);
			if(value)
				particle.Play();
        }
    }
}
