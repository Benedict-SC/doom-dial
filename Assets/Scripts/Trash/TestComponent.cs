using UnityEngine;
using UnityEngine.UI;

public class TestComponent : MonoBehaviour{
	RectTransform rt;
	void Start(){
		rt = GetComponent<RectTransform>();
	}
	void Update(){
		Debug.Log ("a: " + rt.sizeDelta.ToString());
		Debug.Log ("b: " + rt.rect.width);
	}
}
