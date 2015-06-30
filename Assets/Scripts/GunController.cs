using UnityEngine;
using System.Collections;
using System;

public class GunController : MonoBehaviour, EventHandler {

	float cooldown = 0.0f;
	float maxcool = 2.0f;

	public int buttonID; //assign in the Unity Editor to match the corresponding button
	//in the future, we'll assign this value in scripts to deal with changing gun placements

	float defaultBulletSpeed = 0.2f;
	float defaultBulletRange = 3.0f;

	// Use this for initialization
	void Start () {
		EventManager.Instance ().RegisterForEventType ("shot_fired", this);
		GameObject overlayObject = transform.Find("CooldownLayer").gameObject;
		overlayObject.transform.localScale = new Vector3 (0, 0, 1);
	}
	
	// Update is called once per frame
	void Update () {
		if (cooldown > 0) {
			cooldown -= 0.05f;
			//SpriteRenderer overlay = this.gameObject.GetComponentInChildren<SpriteRenderer> ();
			GameObject overlayObject = transform.Find("CooldownLayer").gameObject;
			overlayObject.transform.localScale = new Vector3 (cooldown / maxcool, cooldown / maxcool, 1);
		}
	}

	public void HandleEvent(GameEvent ge){

		//various conditions under which bullet shouldn't fire
		if ((int)ge.args [0] != buttonID)
			return;
		if (cooldown > 0) {
			return;
		}
		if (GameObject.Find ("Dial").GetComponent<SpinScript> ().IsSpinning ()) {
			return;
		}

		cooldown = maxcool; //start cooldown
		GameObject bullet = Instantiate (Resources.Load ("Prefabs/Bullet")) as GameObject; //make a bullet
		BulletController bc = bullet.GetComponent<BulletController>();
		//make it the type of bullet this thing fires
		bc.speed = defaultBulletSpeed;
		bc.range = defaultBulletRange;
		//find your angle
		float ownangle = this.transform.eulerAngles.z;
		float angle = (ownangle +  90) % 360 ;
		angle *= (float)Math.PI / 180;
		//find where to spawn the bullet
		float gunDistFromCenter = (float)Math.Sqrt (transform.position.x*transform.position.x + transform.position.y*transform.position.y);
		gunDistFromCenter += 0.5f;
		bc.spawnx = gunDistFromCenter * (float)Math.Cos (angle);
		bc.spawny = gunDistFromCenter * (float)Math.Sin (angle);
		bc.vx = bc.speed * (float)Math.Cos(angle);
		bc.vy = bc.speed * (float)Math.Sin(angle);
	}
	public float GetCooldownRatio(){
		return cooldown / maxcool;
	}
	public float GetCooldown(){
		return cooldown;
	}
}
