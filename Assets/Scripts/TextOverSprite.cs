using UnityEngine;
using System.Collections;

public class TextOverSprite : MonoBehaviour 
{
    private TextOverSprite shadow;
    private TextMesh textMesh;
    private TextMesh shadowTextMesh;

    public bool HasShadow = true;

	// Use this for initialization
	void Awake () 
    {
        this.textMesh = this.GetComponent<TextMesh>();        
        if (this.HasShadow)
        {
            // toggle shadow on and off to say shadow doesn't actually have a... shadow
            this.HasShadow = false;
            shadow = Instantiate(this);
            this.HasShadow = true;

            shadow.HasShadow = false;
            shadow.transform.SetParent(this.transform);
            shadow.transform.localScale = new Vector3(1, 1, 1);
            shadow.transform.localPosition = new Vector3(-0.02f, -0.02f, 0.01f);
            this.shadowTextMesh = shadow.GetComponent<TextMesh>();
            this.shadowTextMesh.color = Color.black;
        }
	}

    void Start()
    {
        MeshRenderer meshRenderer = this.GetComponent<MeshRenderer>();
        Renderer parentRenderer = this.transform.parent.GetComponent<Renderer>();
        meshRenderer.sortingLayerID = parentRenderer.sortingLayerID;
    }
	
	// Update is called once per frame
	void Update () 
    {	
	}

    public void SetText(string text)
    {        
        this.textMesh.text = text;

        if (this.shadowTextMesh != null)
        {
            this.shadowTextMesh.text = text;
        }
    }
}
