﻿using UnityEngine;
//using UnityEngine.UI;
using System.Collections;

public class ButtonController : MonoBehaviour, EventHandler {

	public int buttonID;
	bool isPaused = false;
	GunController gc = null;

	// Use this for initialization
	void Start () {
		EventManager em = EventManager.Instance ();
		em.RegisterForEventType ("mouse_release", this);
		GameObject overlayObject = transform.Find ("CooldownLayer").gameObject;
		overlayObject.transform.localScale = new Vector3 (0, 0, 1);
		GameObject gun = GameObject.Find ("Gun" + buttonID);
		if (gun != null){
			gc = gun.GetComponent<GunController> ();
			SetDecalFromTower (gc);
		}else {
			SpriteRenderer sr = transform.Find("Label").gameObject.GetComponent<SpriteRenderer>();
			sr.sprite = null;
		}
	}
	public void SetDecalFromTower(GunController gc){
		Sprite s = gc.transform.Find("Label").gameObject.GetComponent<SpriteRenderer>().sprite;
		SpriteRenderer sr = transform.Find("Label").gameObject.GetComponent<SpriteRenderer>();
		sr.sprite = s;

	}
	
	// Update is called once per frame
	void Update () {
		isPaused = GamePause.paused;
		if (gc == null)
			return;
		if (gc.GetCooldown () > 0) {
			float ratio = gc.GetCooldownRatio();
			GameObject overlayObject = transform.Find("CooldownLayer").gameObject;
			//Image img = overlayObject.GetComponent<SpriteRenderer>().sprite.
			overlayObject.transform.localScale = new Vector3 (ratio,ratio, 1);
		}
	}

	public void HandleEvent(GameEvent ge){ //REVISE FOR TOUCH LATER
		if (!isPaused) {
			Vector3 pos = this.transform.position;
			Vector3 mousepos = (Vector3)ge.args [0];
			Vector3 newmousepos = mousepos;//Camera.main.ScreenToWorldPoint (mousepos); //handled in inputwatcher now
			newmousepos.z = 0;
			float distance = (newmousepos - pos).magnitude;
			//calculate radius of buttons
			SpriteRenderer s = this.GetComponent<SpriteRenderer> ();
			float radius = s.bounds.size.x / 2;


			if (distance < radius && gc != null) {
				//Debug.Log ("button released on button " + buttonID);
				GameEvent nge = new GameEvent ("shot_fired");
				nge.addArgument (buttonID);
				EventManager.Instance ().RaiseEvent (nge);
			}
		}
	}
}
