using UnityEngine;
using System.Collections;

public class MenuClickScript : MonoBehaviour, EventHandler {
	public GameObject CamLock1;
	public GameObject CamLock2;
	public GameObject CamLock3;
	public GameObject parent;
	public int menuPosition = 0;
	// Use this for initialization
	void Start () {
		EventManager em = EventManager.Instance ();
		em.RegisterForEventType ("mouse_release", this);
		em.RegisterForEventType ("mouse_click", this);
	}
	public void HandleEvent(GameEvent ge){
		if (ge.type.Equals ("mouse_release")) {
			RaycastHit targetFind;
			Ray targetSeek = Camera.main.ScreenPointToRay (InputWatcher.GetTouchPosition());
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
	}
	// Update is called once per frame
	void Update () {

	}
	void ButtonClick(){
		//Location of the menu dial determines the result
		//Commenting out LoadLevel calls until we have levels to load
		switch (menuPosition) {
		case 0:
			//Loads the scene used for playing the game
			Application.LoadLevel ("WorldSelect");
			break;
		case 1:
			//This and 3 load more levels
			//Application.LoadLevel ("AltSceneTest");
			//Simple menu like this might as well exist within the scene to save on load times
			break;
		case 2:
			//Same as the settings menu
			Camera.main.transform.position = CamLock3.transform.position;
			transform.localEulerAngles = new Vector3(0,0,0);
			parent.GetComponent<MenuSpinScript>().spinLock = true;
			Debug.Log ("Limiters");
			break;
		case 3:
			//Application.LoadLevel ("AltSceneTest");
			break;
		case 4:
			Camera.main.transform.position = CamLock2.transform.position;
			transform.localEulerAngles = new Vector3(0,0,0);
			parent.GetComponent<MenuSpinScript>().spinLock = true;
			Debug.Log ("Settings");
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
		parent.GetComponent<MenuSpinScript>().spinLock = false;
		menuPosition = 0;
		Camera.main.transform.position = CamLock1.transform.position;
	}
}
