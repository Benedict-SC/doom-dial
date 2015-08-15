﻿using UnityEngine;
using System.Collections;

public class LevelSelect : MonoBehaviour, EventHandler {
	public int menuPosition = 0;
	public GameObject textMesh;
	public GameObject startButton;
	public WorldData WorldData;
	public GameObject menuButton;
	string levelName = "LevelHolder";
	int lastPosition = 1;
	// Use this for initialization
	void Start () {
		EventManager em = EventManager.Instance ();
		em.RegisterForEventType ("mouse_release", this);
		em.RegisterForEventType ("mouse_click", this);
		//Need to find old object by hand as it isn't on the scene to start.
		WorldData = GameObject.FindWithTag ("DataHolder").GetComponent<WorldData> ();
	}
	public void HandleEvent(GameEvent ge){
		if (ge.type.Equals ("mouse_release")) {
			RaycastHit targetFind;
			
			Ray targetSeek = Camera.main.ScreenPointToRay (InputWatcher.GetTouchPosition ());
			if (Physics.Raycast (targetSeek, out targetFind)) {
				//sees if ray collided with the start button
				if (targetFind.collider.gameObject == startButton) {
					
					//Application.LoadLevel(levelName);
				}
				if (targetFind.collider.gameObject == menuButton) {
					Debug.Log("Test");
					WorldData.lastScene = "LevelSelect";
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
				textMesh.GetComponent<TextMesh>().text = WorldData.worldSelected + "-2";
				WorldData.levelSelected = "2";
				levelName = "TestScene";
				break;
			case 1:
				textMesh.GetComponent<TextMesh>().text = WorldData.worldSelected + "-3";
				WorldData.levelSelected = "3";
				levelName = "TestScene";
				break;
			case 2:
				textMesh.GetComponent<TextMesh>().text = WorldData.worldSelected + "-4";
				WorldData.levelSelected = "4";
				levelName = "TestScene";
				break;
			case 3:
				textMesh.GetComponent<TextMesh>().text = WorldData.worldSelected + "-1";
				WorldData.levelSelected = "1";
				levelName = "TestScene";
				break;
			default:
				break;
			}
			lastPosition = menuPosition;
		}
	}
}