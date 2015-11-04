using UnityEngine;
using System.Collections;
/*
 * To add new scene options to a menu:
 * 1) Set lockThreshold in MenuSpinScript to a value that divides nicely into 360 that equals the number of options you want 
 * (Menu, WorldSelect, and levelSelect all need a third of that number so you can fit in the duplicate images so 
 * the fading works, lockThreshold on MenuTest only need to be set to 60 for a sixth option, adding a fifth option
 * to World/LevelSelect or Menu needs (5x3) = 15 lock positions, so lockThreshold needs to be 24)
 * 2) Go to whatever script handle menu options for that scene (MenuClickScript/MenuSelect/MenuInGame)
 * in the editor and add a new entry to DescHolder and LevelHolder, DescHolder holds the string shown in game,
 * LevelHolder holds the name of the scene. World and Level Select don't need any further work, as they are set to work off
 * of the numbers being passed in.
 * */
public class MenuSpinScript : MonoBehaviour, EventHandler {
	//Increases spin speed
	float multiplier = 1.0f;
	public GameObject Child;
	public GameObject spinPivot;
	//Can only spin if this is true;
	bool spinner = false;
	public bool spinLock = false;
	//Implemented early to allow the player to stop over a button and not accidentally trigger it
	float clickTime = 0;
	//Centralized variable for how long the player can hold before it becomes a drag instead of a press
	float clickDelay = 0.1f;
	public int lockThreshold = 72;
	public int menuPosition = 0;
	Vector3 centerPoint;
	bool touchDown = false;
	float originalRot = 0.0f; //the angle of the mouse when you start the spin
	float origz = 0.0f; //the angle of the dial when you start the spin
	float rotScale = 1.0f; //speeds up or slows down the rotation. should probably stay at 1.0, unless playtesting discovers otherwise
	int menuMax;
	// Use this for initialization
	void Start () {
		centerPoint = Camera.main.WorldToScreenPoint (this.transform.position);
		EventManager em = EventManager.Instance ();
		em.RegisterForEventType ("mouse_release", this);
		em.RegisterForEventType ("mouse_click", this);
		menuMax = 360 / lockThreshold;
	}
	public void HandleEvent(GameEvent ge){
		Vector3 mousepos = InputWatcher.GetInputPosition ();
		mousepos = new Vector3(mousepos.x - spinPivot.transform.position.x,mousepos.y-spinPivot.transform.position.y,mousepos.z);
		if (ge.type.Equals ("mouse_release")) {
			//Stops the dial from spinning more
			spinner = false;
			//Only tries to lock if the spinner has a chance of moving
			if(clickTime > clickDelay){
				//Locks position to nearest interval of 60
				float rotation = transform.eulerAngles.z;
				float lockRot = Mathf.Round(rotation /lockThreshold)*lockThreshold;
				transform.rotation = Quaternion.Euler(0, 0, lockRot);
				menuPosition = (int) lockRot/lockThreshold;
				if(Child.GetComponent<MenuClickScript>() != null){
					Child.GetComponent<MenuClickScript>().menuPosition = menuPosition % menuMax;
				}
				if(Child.GetComponent<WorldSelect>() != null){
					Child.GetComponent<WorldSelect>().menuPosition = menuPosition % menuMax;
				}
				if(Child.GetComponent<LevelSelect>() != null){
					Child.GetComponent<LevelSelect>().menuPosition = menuPosition % menuMax;
				}
				if(Child.GetComponent<MenuSelect>() != null){
					Child.GetComponent<MenuSelect>().menuPosition = menuPosition % menuMax;
				}
				if(Child.GetComponent<MenuInGame>() != null){
					Child.GetComponent<MenuInGame>().menuPosition = menuPosition % menuMax;
				}
			}
			//resets time
			clickTime = 0;
			touchDown = false;
		}else if(ge.type.Equals("mouse_click")){
			//Allows the dial to start spinning
			if(spinner == false){
				originalRot = Mathf.Atan2(mousepos.y,mousepos.x);
				origz = transform.eulerAngles.z;
				//Debug.Log ("new original degrees: " + originalRot);
			}
			spinner = true;
			touchDown = true;
		}
	}
	// Update is called once per frame
	void Update () {
		Vector3 mousepos = InputWatcher.GetInputPosition ();
		mousepos = new Vector3(mousepos.x - spinPivot.transform.position.x,mousepos.y-spinPivot.transform.position.y,mousepos.z);
		//Debug.Log (touchDown);
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

			}
		}
	}
}