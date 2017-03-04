using UnityEngine;
using System.Collections;

public class DialogCall : MonoBehaviour {

    public void Open() {
        gameObject.GetComponentInChildren<Dialog>().Open();
    }
}
