using UnityEngine;
using System.Collections;

public class MenuSpinScript : MonoBehaviour {
	//Increases spin speed
	public float multiplier = 10f;
	public GameObject Child;
	//Can only spin if this is true;
	bool spinner = false;
	public bool spinLock = false;
	//Implemented early to allow the player to stop over a button and not accidentally trigger it
	float clickTime = 0;
	//Centralized variable for how long the player can hold before it becomes a drag instead of a press
	float clickDelay = 0.1f;
	float rotX = 0.0f;
	float rotY = 0.0f;
	public int lockThreshold = 72;
	public int menuPosition = 0;
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
				float lockRot = Mathf.Round(rotation /lockThreshold)*lockThreshold;

				menuPosition = (int) lockRot/lockThreshold;
				if(Child){
				Child.GetComponent<MenuClickScript>().menuPosition = menuPosition;
				}
				transform.rotation = Quaternion.Euler(0, 0, lockRot);

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

}