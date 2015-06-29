using UnityEngine;
using System.Collections;
using System;

public class GunController : MonoBehaviour, EventHandler {

	public float cooldown = 0.0f;
	public float maxcool = 1.0f;

	public int buttonID; //assign in the Unity Editor to match the corresponding button
	//in the future, we'll assign this value in scripts to deal with changing gun placements

	float defaultBulletSpeed = 0.2f;
	float defaultBulletRange = 3.0f;

	// Use this for initialization
	void Start () {
		EventManager.Instance ().RegisterForEventType ("shot_fired", this);
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void HandleEvent(GameEvent ge){
		if ((int)ge.args [0] != buttonID)
			return;

		cooldown = maxcool;
		GameObject bullet = Instantiate (Resources.Load ("Prefabs/Bullet")) as GameObject;
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
		bc.spawnx = gunDistFromCenter * (float)Math.Cos (angle);
		bc.spawny = gunDistFromCenter * (float)Math.Sin (angle);
		bc.vx = bc.speed * (float)Math.Cos(angle);
		bc.vy = bc.speed * (float)Math.Sin(angle);
	}
}
