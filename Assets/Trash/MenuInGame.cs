﻿using UnityEngine;
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
public class MenuInGame : MonoBehaviour, EventHandler {

	public int menuPosition = 0;
	public GameObject textMesh;
	public GameObject startButton;
	public GameObject menuButton;
	public GameObject returnButton;
	public GameObject cameraLock2;
	string levelName = "";
	int lastPosition = 1;
	public string test;
	public string[] descHolder;
	public string[] levelHolder;
	// Use this for initialization
	void Start () {
		EventManager em = EventManager.Instance ();
		em.RegisterForEventType ("mouse_release", this);
		em.RegisterForEventType ("mouse_click", this);
	}
	public void HandleEvent(GameEvent ge){
		if (ge.type.Equals ("mouse_release")) {
			RaycastHit targetFind;

			Ray targetSeek = Camera.main.ScreenPointToRay (InputWatcher.GetTouchPosition ());
			if (Physics.Raycast (targetSeek, out targetFind)) {
				Debug.Log (targetFind.collider.name);
				//sees if ray collided with the start button
				if (targetFind.collider.gameObject == startButton) {
					//Debug.Log ("try and load level select");
					if(levelHolder[menuPosition] == "Return"){
						Camera.main.transform.position = cameraLock2.transform.position;		
					}else{
						WorldData.lastScene = Application.loadedLevelName;
						levelName = levelHolder[menuPosition];
						GamePause.paused = false;
						Application.LoadLevel(levelName);
					}

					/*if(menuPosition == 3 || menuPosition == 1){
						worldHolder.GetComponent<WorldData>().lastScene = Application.loadedLevelName;
						Application.LoadLevel(levelName);
					}else if(menuPosition == 2){
						Camera.main.transform.position = cameraLock2.transform.position;	
					}*/
				}
			}	
		}
	}
	// Update is called once per frame
	void Update () {
		//Stops entire statement from running every frame to save overhead
		if (menuPosition != lastPosition) {
			/*switch(menuPosition){
				//Sets values for WorldData, the on screen text, and the level that will be loaded
			case 0:
				textMesh.GetComponent<TextMesh>().text = "Settings";
				Debug.Log("Settings not done, add in later");
				break;
			case 1:
				textMesh.GetComponent<TextMesh>().text = "Tower Editor";
				levelName = "TowerSelect";

				break;
			case 2:
				textMesh.GetComponent<TextMesh>().text = "Back";
				//levelName = worldHolder.GetComponent<WorldData>().lastScene;
				break;
			case 3:
				textMesh.GetComponent<TextMesh>().text = "Main Menu";
				levelName = "MenuTest";
				break;
			default:
				break;
			}*/
			textMesh.GetComponent<TextMesh>().text = descHolder[menuPosition];
			lastPosition = menuPosition;
		}
	}
}