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
public class MenuClickScript : MonoBehaviour, EventHandler {
	public GameObject parent;
	public int menuPosition = 0;
	public string[] levelList;
	// Use this for initialization
	void Start () {
		EventManager em = EventManager.Instance ();
		em.RegisterForEventType ("mouse_release", this);
		em.RegisterForEventType ("mouse_click", this);
		GameObject[] temp = new GameObject[2];
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
						WorldData.lastScene = Application.loadedLevelName;
						Application.LoadLevel (levelList[menuPosition]);
					}
				}
			}
			
		}
	}
	// Update is called once per frame
	void Update () {

	}
}
