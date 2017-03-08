using UnityEngine;
using System.Collections;

public class FloatyText : MonoBehaviour
{
    private TextMesh textObject;
    private TextMesh shadowTextObject;
    public float MoveSpeed = 1;

    public float Lifetime = 1.0f;

    public bool IsSpecialText = false;

    void InitComponents()
    {
    }

	void Start () 
    {
        GameObject.Destroy(this.gameObject, Lifetime);

        if (!IsSpecialText)
        {
            textObject = this.GetComponent<TextMesh>();

            if (textObject != null)
            {                
                textObject.GetComponent<Renderer>().sortingLayerName = "TopText";
            }

            if (this.gameObject.transform.childCount > 0)
            {
                shadowTextObject = this.gameObject.transform.GetChild(0).GetComponent<TextMesh>();
                if (shadowTextObject != null)
                {
                    shadowTextObject.GetComponent<Renderer>().sortingLayerName = "TopTextShadow";
                }
            }
        }
    }
		
	void Update () 
    {
        float newY = this.transform.localPosition.y + (Time.deltaTime * MoveSpeed);
        this.transform.localPosition = new Vector3(this.transform.localPosition.x, newY, this.transform.localPosition.z);
	}

    public void SetText(string score, Color? color = null, TextAnchor? anchor = null, int? fontSize = null)
    {
        if (IsSpecialText)
        {
            return;
        }

        textObject = this.GetComponent<TextMesh>();

        if (this.gameObject.transform.childCount > 0)
        {
            shadowTextObject = this.gameObject.transform.GetChild(0).GetComponent<TextMesh>();
        }         

        if (textObject != null)
        {
            textObject.text = score;
            if (color != null)
            {
                textObject.color = color.Value;
            }

            if (anchor != null)
            {
                this.textObject.anchor = anchor.Value;                
            }

            if (fontSize != null)
            {
                this.textObject.fontSize = fontSize.Value;
            }
        }

        if (shadowTextObject != null)
        {
            shadowTextObject.text = score;

            if (anchor != null)
            {
                this.shadowTextObject.anchor = anchor.Value;
            }

            if (fontSize != null)
            {
                this.shadowTextObject.fontSize = fontSize.Value;
            }
        }
    }
}
