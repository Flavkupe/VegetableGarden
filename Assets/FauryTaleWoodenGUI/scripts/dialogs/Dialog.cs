using UnityEngine;
using System.Collections;

public class Dialog : MonoBehaviour {

    public GameObject dialogRoot;

    protected bool _isOpened = false;

    public void Close() {
        if(!_isOpened)
            return;
        Animator animator = gameObject.GetComponent<Animator>();
        animator.enabled = true;
        animator.CrossFade("dialog_hide", 0);
        _isOpened = false;
    }
        
    public void CloseComplete() {
        if(dialogRoot)
            dialogRoot.SetActive(false);
        else
            gameObject.SetActive(false);
        Animator animator = gameObject.GetComponent<Animator>();
        animator.enabled = false;
        animator.Stop();
    }
	virtual public void Open() {
        if(_isOpened)
            return;
        if(dialogRoot)
            dialogRoot.SetActive(true);
        else
            gameObject.SetActive(true);
        Animator animator = gameObject.GetComponent<Animator>();
        animator.enabled = true;
        animator.CrossFade("dialog_open", 0);
        _isOpened = true;
    }

    virtual public void OpenComplete() {
        Animator animator = gameObject.GetComponent<Animator>();
        animator.Stop();
        animator.enabled = false;
    }
}
