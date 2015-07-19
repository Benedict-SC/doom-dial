using UnityEngine;
using System.Collections;

public class MenuScaleEffect : MonoBehaviour {
	public GameObject parent;
	public float rotOffset = 0.0f;
	float scaleFactor = 0.0f;
	public float rotationSet;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		//rotOffset keeps the scale accurate for all icons past the first
		rotationSet = (parent.transform.eulerAngles.z + rotOffset) % 360;
		if(rotationSet >= 180.0f){
			//Shrinks object while it moves up and to the left
			scaleFactor = (rotationSet) / 360.0f;
		}else{
			//Grows object when it hits the halfway point when going left
			scaleFactor = 1.0f - ((rotationSet)/ 360.0f);
		}
		transform.localScale = new Vector3 (scaleFactor, scaleFactor, 0);
		//Keeps all icons facing down
		transform.localEulerAngles = new Vector3(0,0,(360 - parent.transform.eulerAngles.z));
	}
}
