/*Devon*/

using UnityEngine;
using System.Collections;

public class MenuSelectScaleEffect : MonoBehaviour {
	public GameObject parent;
	public float rotOffset = 0.0f;
	float scaleFactor = 0.0f;
	public float opFactor = 0.0f;
	public float secondaryOp = 0.0f;
	public float rotationSet;
	public float opAdjust = 2.0f;
	Color transparency = new Color(1,1,1,1);
	float temp = 360.0f;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {

		//rotOffset keeps the scale accurate for all icons past the first
		rotationSet = (parent.transform.eulerAngles.z + rotOffset) % 360;
		if(rotationSet <= 60.0f){
			temp = 120.0f;
			opAdjust = 2.0f;
			//Shrinks object while it moves up and to the left
			scaleFactor = 1.0f - ((rotationSet)/ temp);
			opFactor = (1.0f - ((rotationSet)/ temp));
			secondaryOp = (rotationSet) / temp;
			scaleFactor *= scaleFactor;
			
		}else if(rotationSet >= 270.0f){
			temp = 360.0f;
			opAdjust = 3.0f;
			//Grows object when it hits the halfway point when going left
			scaleFactor = (rotationSet) / temp;
			opFactor = (rotationSet) / temp;
			secondaryOp = 1.0f - ((rotationSet)/ temp);

			scaleFactor *= scaleFactor;
		}
		

		transform.localScale = new Vector3 (scaleFactor, scaleFactor, 0);
		//Keeps all icons facing down
		transform.localEulerAngles = new Vector3(0,0,(360 - parent.transform.eulerAngles.z));
		transparency = GetComponent<SpriteRenderer> ().material.color;
		
		transparency.a = opFactor - (secondaryOp * opAdjust);
		GetComponent<SpriteRenderer> ().material.color = transparency;
	}
}

