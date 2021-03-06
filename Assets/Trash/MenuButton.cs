﻿using UnityEngine;
using System.Collections;

public class MenuButton : MonoBehaviour, EventHandler {
	// Use this for initialization
	void Start () {
		EventManager em = EventManager.Instance ();
		em.RegisterForEventType ("mouse_release", this);
		em.RegisterForEventType ("mouse_click", this);
		//Need to find old object by hand as it isn't on the scene to start.
	}
	public void HandleEvent(GameEvent ge){
		if (ge.type.Equals ("mouse_release")) {
			RaycastHit targetFind;
			
			Ray targetSeek = Camera.main.ScreenPointToRay (InputWatcher.GetTouchPosition ());
			if (Physics.Raycast (targetSeek, out targetFind)) {
				//sees if ray collided with the start button
				if (targetFind.collider.gameObject == this.gameObject) {
					WorldData.lastScene = Application.loadedLevelName;
					Application.LoadLevel("Menu");
				}
			}
			
		}
	}
	// Update is called once per frame
	void Update () {
	
	}
}
