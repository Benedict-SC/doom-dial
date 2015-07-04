﻿using UnityEngine;
using System.Collections;
using System;

public class GunController : MonoBehaviour, EventHandler {

	//Tower type: "Bullet", "Trap", or "Shield"
	String towerType;

	//***Skill values begin here***
	public float dmg; //damage dealt out (direct value)
	public float speed; //speed of the bullet
	public float range; //range -- expressed in percent of the length of the lane
	public float knockback; //knockback
	public float lifeDrain; //lifedrain on enemy
	public float poison; //poison damage on enemy
	public float splash; //radius of splash damage
	public float stun; //amount (time?) of enemy stun
	public float slowdown; //enemy slowdown -- scale of 1 to 10, can't go over 8
	public float penetration; //ignores this amount of enemy shield
	public float shieldShred; //lowers enemy shield's max value by this

	public int spread; //number of shots fired at once, default should be 1.

	public bool doesSplit; //whether it splits in 2 at the end of its path/collision
	public bool isHoming; //whether it homes in on nearest enemy
	public bool doesArc; //whether it arcs (travels over enemies until it hits the ground at max range)
	//***Skill values end here***

	/* Another tower attribute
	 * But not passed to bullets
	 */
	float cooldownFactor = 1.0f; //percentage of max cooldown time.  By default 1.0

	float cooldown = 0.0f;
	float maxcool;

	public int buttonID; //assign in the Unity Editor to match the corresponding button
	//in the future, we'll assign this value in scripts to deal with changing gun placements

	float defaultBulletSpeed = 0.2f;
	float defaultBulletRange = 3.0f;

	// Use this for initialization
	void Start () {
		EventManager.Instance ().RegisterForEventType ("shot_fired", this);
		GameObject overlayObject = transform.Find("CooldownLayer").gameObject;
		overlayObject.transform.localScale = new Vector3 (0, 0, 1);

		maxcool = 2.0f * cooldownFactor;


		//defaults
		towerType = "Bullet";
		dmg = 10;
		speed = defaultBulletSpeed;
		range = defaultBulletRange;


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

		//Decide what to do based on tower type ("Bullet", "Trap", or "Shield")
		switch (towerType)
		{
		case "Bullet":
			cooldown = maxcool; //start cooldown
			GameObject bullet = Instantiate (Resources.Load ("Prefabs/Bullet")) as GameObject; //make a bullet
			BulletController bc = bullet.GetComponent<BulletController>();
			//make it the type of bullet this thing fires
			ConfigureBullet (bc);
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
			break;
		case "Trap":
			print ("Traps not implemented yet");
			break;
		case "Shield":
			print ("Shields not implemented yet");
			break;
		default:
			print ("Uh oh, I didn't receive a valid towerType string value!");
			Debug.Log("value is: " + towerType);
			break;
		}

	}
	public float GetCooldownRatio(){
		return cooldown / maxcool;
	}
	public float GetCooldown(){
		return cooldown;
	}

	//Assigns skill values to bullets
	private void ConfigureBullet(BulletController bc)
	{
		if (speed == 0 || range == 0 || dmg == 0)
			print ("Check your speed, range, and/or dmg!  One might be 0!");
		bc.dmg = dmg;
		bc.speed = speed;
		bc.range = range;
		bc.knockback = knockback;
		bc.lifeDrain = lifeDrain;
		bc.poison = poison;
		bc.splash = splash;
		bc.stun = stun;
		bc.slowdown = slowdown;
		bc.spread = spread;
		bc.penetration = penetration;
		bc.shieldShred = shieldShred;
		bc.doesSplit = doesSplit;
		bc.isHoming = isHoming;
		bc.doesArc = doesArc;
	}
}
