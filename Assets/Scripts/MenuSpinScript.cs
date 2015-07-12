using UnityEngine;
using System.Collections;

public class MenuSpinScript : MonoBehaviour {
	//Increases spin speed
	public float multiplier = 10f;
	public GameObject CamLock1;
	public GameObject CamLock2;
	public GameObject CamLock3;
	//Can only spin if this is true;
	bool spinner = false;
	bool spinLock = false;
	//Implemented early to allow the player to stop over a button and not accidentally trigger it
	float clickTime = 0;
	//Centralized variable for how long the player can hold before it becomes a drag instead of a press
	float clickDelay = 0.1f;
	float rotX = 0.0f;
	float rotY = 0.0f;
	int menuPosition = 0;
	// Use this for initialization
	void Start () {
		
	}

	// Update is called once per frame
	void Update () {
		if(Input.GetMouseButtonDown(0)){
			//Allows the dial to start spinning
			spinner = true;
		}
		if(Input.GetMouseButtonUp(0)){
			//Stops the dial from spinning more
			spinner = false;
			//Only tries to lock if the spinner has a chance of moving
			if(clickTime > clickDelay){
				//Locks position to nearest interval of 60
				float rotation = transform.eulerAngles.z;
				float lockRot = Mathf.Round(rotation /72)*72;

				menuPosition = (int) lockRot/72;
				transform.rotation = Quaternion.Euler(0, 0, lockRot);

			}else{
				RaycastHit targetFind;
				Ray targetSeek = Camera.main.ScreenPointToRay (Input.mousePosition);
					if (Physics.Raycast (targetSeek, out targetFind)) {
							//gets stats of clicked building, triggers the GUI popups
							if (targetFind.collider.gameObject.tag == "Button") {
						//what triggers changes based on what menu the camera is focused on.
						if(targetFind.transform.position.x == 0.0f){
							ButtonClick();
						}else if(targetFind.transform.position.x < 0.0f){
							ReturnMain(0);
						}else{
							ReturnMain(1);
						}
							}
						}
					}
			//resets time
			clickTime = 0;
		}
		if(Input.GetMouseButton(0)){
			if(!spinLock){
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
	}
	void ButtonClick(){
		//Location of the menu dial determines the result
		//Commenting out LoadLevel calls until we have levels to load
		switch (menuPosition) {
		case 0:
			//Loads the scene used for playing the game
			//Application.LoadLevel ("TestScene");
			break;
		case 1:
			//Simple menu like this might as well exist within the scene to save on load times
			Camera.main.transform.position = CamLock2.transform.position;
			transform.localEulerAngles = new Vector3(0,0,0);
			spinLock = true;
			Debug.Log ("Settings");
			break;
		case 2:
			//Same as the settings menu
			Camera.main.transform.position = CamLock3.transform.position;
			transform.localEulerAngles = new Vector3(0,0,0);
			spinLock = true;
			Debug.Log ("Limiters");
			break;
		case 3:
			//This and 4 load more levels
			//Application.LoadLevel ("AltSceneTest");
			break;
		case 4:
			//Application.LoadLevel ("AltSceneTest");
			break;
		case 5:
			//The way menuPosition is assigned results in it being 5 if the dial is released at a certain point
			//Application.LoadLevel ("TestScene");
			break;
		default:
			break;
		}
	}

	void ReturnMain(int currentMenu){
		//Later on currentMenu will be used to only save stuff that might have been changed in that menu
		spinLock = false;
		Camera.main.transform.position = CamLock1.transform.position;
	}
}