using UnityEngine;
using System.Collections;

public class Filler : MonoBehaviour {

	public float size = 0f;
	public float minWidth = 0f;
	public float maxWidth = 0f;

	public float[]  starPositions = {0.3f, 0.7f, 1f};

	public GameObject fillerImage;

	private float _currentSize = 0;
	private RectTransform _fillerRect;
	private SwitchBlock[] _stars;

	// Use this for initialization
	void Start () {
		_stars = gameObject.GetComponentsInChildren<SwitchBlock> ();
		for (int i = 0; i < 3; i++) {
			RectTransform rectTransform = _stars[i].GetComponent<RectTransform> ();
			rectTransform.localPosition = new Vector3 (
				minWidth + (maxWidth - minWidth) * starPositions [i],
				rectTransform.localPosition.y,
				0
			);
			_stars[i].enabled = false;
		}
		_fillerRect = fillerImage.GetComponent<RectTransform> ();
		_fillerRect.sizeDelta = new Vector2(minWidth, _fillerRect.sizeDelta.y);
	}
	
	// Update is called once per frame
	void Update () {
		if (size > _currentSize) {
			_currentSize += Time.deltaTime*0.6f;
			_fillerRect.sizeDelta = new Vector2(minWidth+(maxWidth - minWidth)*_currentSize, _fillerRect.sizeDelta.y);
			for (int i = 0; i < 3; i++) {
				if (starPositions [i] < _currentSize)
					_stars [i].enabled = true;
			}
		}
	}
}
