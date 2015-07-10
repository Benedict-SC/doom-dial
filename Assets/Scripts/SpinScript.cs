﻿using UnityEngine;
using System.Collections;
//Attach this script to the dial
public class SpinScript : MonoBehaviour {
	//Increases spin speed
	//public float multiplier = 10f;
	//Can only spin if this is true;
	bool spinner = false;
	//Implemented early to allow the player to stop over a button and not accidentally trigger it
	float clickTime = 0;
	//Centralized variable for how long the player can hold before it becomes a drag instead of a press
	float clickDelay = 0.1f;

	float originalRot = 0.0f;
	float origz = 0.0f;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		Vector3 rawmousepos = Input.mousePosition;
		Vector3 mousepos = Camera.main.ScreenToWorldPoint (rawmousepos);

		if(Input.GetMouseButtonDown(0)){
			//Allows the dial to start spinning
			if(spinner == false){
				originalRot = Mathf.Atan2(mousepos.y,mousepos.x);
				origz = transform.eulerAngles.z;
				Debug.Log ("new original degrees: " + originalRot);
			}
			spinner = true;

		}
		if(Input.GetMouseButtonUp(0)){
			//Stops the dial from spinning more
			spinner = false;
			//Only tries to lock if the spinner has a chance of moving
			if(clickTime > clickDelay){
			//Locks position to nearest interval of 60
			float rotation = transform.eulerAngles.z;
			float lockRot = Mathf.Round(rotation /60)*60;
			transform.rotation = Quaternion.Euler(0, 0, lockRot);
			}
			//resets time
			clickTime = 0;
		}
		if(Input.GetMouseButton(0)){
			clickTime += Time.deltaTime;
			//Only allows the dial to spin if the player has been pressing for over a certain amount of time
			if(spinner && clickTime > clickDelay){
				//Probably not the best for dealing with movement on both axis, 
				//also will change code to touch controls once we start testing the game on mobile
				float angle = Mathf.Atan2(mousepos.y,mousepos.x);// (mousepos.y,mousepos.x);
				float degrees = Mathf.Rad2Deg * angle;
				float origDegrees = Mathf.Rad2Deg * originalRot;
				transform.rotation = Quaternion.Euler(0,0,(origz + degrees - origDegrees)%360);
				//transform.Rotate(0, 0, Input.GetAxis("Mouse Y") + Input.GetAxis("Mouse X")* multiplier, Space.World);
			}
		}
	}
	public bool IsSpinning(){
		return clickTime > clickDelay;
	}
}
