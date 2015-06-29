using UnityEngine;
using System.Collections;

public class TouchTester : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.touchCount > 0)
			Debug.Log ("we're touching");
		else {
			Debug.Log("we're not touching");
		}
	}
}
