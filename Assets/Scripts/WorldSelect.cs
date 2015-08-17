﻿using UnityEngine;
using System.Collections;

public class WorldSelect : MonoBehaviour, EventHandler {
	public int menuPosition = 0;
	public GameObject textMesh;
	public GameObject startButton;
	public GameObject menuButton;
	public GameObject worldHolder;
	string levelName = "WorldSelect";
	int lastPosition = 1;
	// Use this for initialization
	void Start () {
		EventManager em = EventManager.Instance ();
		em.RegisterForEventType ("mouse_release", this);
		em.RegisterForEventType ("mouse_click", this);
		worldHolder = GameObject.FindWithTag ("DataHolder");
	}
	public void HandleEvent(GameEvent ge){
		if (ge.type.Equals ("mouse_release")) {
			RaycastHit targetFind;

			Ray targetSeek = Camera.main.ScreenPointToRay (InputWatcher.GetTouchPosition ());
			if (Physics.Raycast (targetSeek, out targetFind)) {
				//sees if ray collided with the start button
				if (targetFind.collider.gameObject == startButton) {
					//Debug.Log ("try and load level select");
					Application.LoadLevel(levelName);
				}
				if (targetFind.collider.gameObject == menuButton) {
					Debug.Log("Test");
					worldHolder.GetComponent<WorldData>().lastScene = "WorldSelect";
					Application.LoadLevel("Menu");
				}
			}

		}
	}
	// Update is called once per frame
	void Update () {
		//Stops entire statement from running every frame to save overhead
		if (menuPosition != lastPosition) {
			switch(menuPosition){
				//Sets values for WorldData, the on screen text, and the level that will be loaded
			case 0:
				worldHolder.GetComponent<WorldData>().worldSelected = "World2";
				textMesh.GetComponent<TextMesh>().text = "World 2";
				levelName = "LevelSelect";
				break;
			case 1:
				worldHolder.GetComponent<WorldData>().worldSelected = "World3";
				textMesh.GetComponent<TextMesh>().text = "World 3";
				levelName = "LevelSelect";
				break;
			case 2:
				worldHolder.GetComponent<WorldData>().worldSelected = "World4";
				textMesh.GetComponent<TextMesh>().text = "World 4";
				levelName = "LevelSelect";
				break;
			case 3:
				worldHolder.GetComponent<WorldData>().worldSelected = "World1";
				textMesh.GetComponent<TextMesh>().text = "World 1";
				levelName = "LevelSelect";
				break;
			default:
				break;
			}
			lastPosition = menuPosition;
		}
	}
}
