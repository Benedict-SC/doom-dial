
using UnityEngine;
using System.Collections;
//Attach this script to the dial
public class SpinScript : MonoBehaviour, EventHandler {
	//Increases spin speed
	//public float multiplier = 10f;
	//Can only spin if this is true;
	bool spinner = false;
	//Implemented early to allow the player to stop over a button and not accidentally trigger it
	float clickTime = 0;
	bool touchDown = false;
	//Centralized variable for how long the player can hold before it becomes a drag instead of a press
	float clickDelay = 0.1f;

	float rotScale = 1.0f; //speeds up or slows down the rotation. should probably stay at 1.0, unless playtesting discovers otherwise
	
	float originalRot = 0.0f; //the angle of the mouse when you start the spin
	float origz = 0.0f; //the angle of the dial when you start the spin
	
	// Use this for initialization
	void Start () {
		EventManager em = EventManager.Instance ();
		em.RegisterForEventType ("mouse_release", this);
		em.RegisterForEventType ("mouse_click", this);
	}
	
	public void HandleEvent(GameEvent ge){
		Vector3 mousepos = InputWatcher.GetInputPosition ();
		if (ge.type.Equals ("mouse_release")) {
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
			touchDown = false;
		}else if(ge.type.Equals("mouse_click")){
			//Allows the dial to start spinning
			if(spinner == false){
				originalRot = Mathf.Atan2(mousepos.y,mousepos.x);
				origz = transform.eulerAngles.z;
				Debug.Log ("new original degrees: " + originalRot);
			}
			spinner = true;
			touchDown = true;
		}
	}
	
	// Update is called once per frame
	void Update () {
		Vector3 mousepos = InputWatcher.GetInputPosition ();
		Debug.Log (touchDown);
		if(touchDown){
			//Debug.Log ("mouse down");
			clickTime += Time.deltaTime;
			//Only allows the dial to spin if the player has been pressing for over a certain amount of time
			if(spinner && clickTime > clickDelay){
				//Probably not the best for dealing with movement on both axis, 
				//also will change code to touch controls once we start testing the game on mobile
				float angle = Mathf.Atan2(mousepos.y,mousepos.x);// (mousepos.y,mousepos.x);
				float degrees = (Mathf.Rad2Deg * angle);
				float origDegrees = Mathf.Rad2Deg * originalRot;
				transform.rotation = Quaternion.Euler(0,0,(origz + (degrees - origDegrees)*rotScale)%360);
				//transform.Rotate(0, 0, Input.GetAxis("Mouse Y") + Input.GetAxis("Mouse X")* multiplier, Space.World);
			}
		}
	}
	public bool IsSpinning(){
		return clickTime > clickDelay;
	}
}

//spinscript was already fixed and set up for event handling- I've left this code here in case we need to reference it,
// but it's been rolled back to the previous fixes
/*
using UnityEngine;
using System.Collections;
//Attach this script to the dial
public class SpinScript : MonoBehaviour, EventHandler {
	//Increases spin speed
	//public float multiplier = 10f;
	//Can only spin if this is true;
	bool spinner = false;
	//Implemented early to allow the player to stop over a button and not accidentally trigger it
	float clickTime = 0;
	//Centralized variable for how long the player can hold before it becomes a drag instead of a press
	float clickDelay = 0.1f;
	float rotX = 0.0f;
	float rotY = 0.0f;
	//Allows easy adjustment of spin speed
	public float multiplier = 5.0f;
	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
		Vector3 rawmousepos = Input.mousePosition;
		Vector3 mousepos = Camera.main.ScreenToWorldPoint (rawmousepos);

		if(Input.GetMouseButtonDown(0)){
			Debug.Log ("GetMouseButtonDown triggered");
			//Allows the dial to start spinning
			spinner = true;

		}
		if(Input.GetMouseButtonUp(0)){
			Debug.Log ("GetMouseButtonUp triggered");
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
			Debug.Log ("GetMouseButton triggered");
			clickTime += Time.deltaTime;
			//Only allows the dial to spin if the player has been pressing for over a certain amount of time
			if(spinner && clickTime > clickDelay){
				//Change code to touch control later
				//Changes direction of spin on each axis based on the location of the input
				if(Input.mousePosition.x >= Screen.width/2){
					rotY = Input.GetAxis("Mouse Y");
				}else{
					rotY = Input.GetAxis("Mouse Y")* -1;
				}
				if(Input.mousePosition.y >= Screen.height/2){
					rotX = Input.GetAxis("Mouse X")*-1;
				}else{
					rotX = Input.GetAxis("Mouse X");
				}
				
				transform.Rotate(0, 0, ((rotX + rotY)* multiplier), Space.World);
			}
		}
	}
	public bool IsSpinning(){
		return clickTime > clickDelay;
	}
}
*/
