﻿/*Thom*/

using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;
using UnityEngine.UI;

public class Trap : Weapon {
	
	float TRACK_LENGTH = 3.1f; //hard coded to avoid querying track size all the time
	public float baseDamage = 10f;
	private bool isActive; //whether it's armed
	public float maxArmingTime; //max time needed to arm, in seconds
	private float armTime; //current countdown time for arming
	public int usesLeft; //remaining uses

	public int duplicatedTimes = 0;
	Timer duplicateTimer;
	float duplicateDelay = 0.3f;
	bool firingDupes = false;
	
	public float spawnx;
	public float spawny;
	
	public GameObject enemyHit; //for use by AoE
	
	private float age; //age of the trap -- used to determine which of two overlaid traps to destroy

	private RectTransform rt;
	
	// Use this for initialization
	void Start () {
		age = 0.0f;
        Debug.Log("x transform is " + transform.position.x + " y transform is " + transform.position.y + " dmg is " + dmg);
		armTime = maxArmingTime;
		isActive = false;
		rt = GetComponent<RectTransform>();
		float radius = rt.rect.width / 2;
		CircleCollider2D collider = transform.gameObject.GetComponent<CircleCollider2D> ();
		collider.radius = radius;
		//Debug.Log ("bullet radius is: " + radius);
		//set its position
		//this.transform.position = new Vector3 (spawnx, spawny, this.transform.position.z);
	}
	
	// Update is called once per frame
	void Update () {
		age += Time.deltaTime; //update the age
		
		if (armTime <=0.0f && !isActive)
		{
			Debug.Log ("armTime is 0 or less!");
			isActive = true;
			Debug.Log ("trap armed!");
		}
		//traps don't move so no changes in its position after placement
		//they need to count down to become active
		if (armTime > 0.0f) //if it's still counting down
		{
			armTime -= Time.deltaTime; //update arming countdown time
			Debug.Log ("armTime = " + armTime);
			if (armTime <= 0.0f) //if it's done counting down, activate the trap
			{
				isActive = true;
				Debug.Log ("trap armed!");
			}
		}
		if(firingDupes){
			if(duplicateTimer.TimeElapsedSecs() > duplicateDelay){
				if(duplicatedTimes < duplicate){
					FireEffect();
					duplicateTimer.Restart();
					duplicatedTimes++;
					if(duplicatedTimes >= duplicate){
						usesLeft--;
						if(usesLeft <= 0){
							Debug.Log("dupe trap expended");
							Destroy (this.gameObject);
						}
						firingDupes = false;
						duplicatedTimes = 0;
					}
				}else{
					Debug.Log("trap duplicate power shouldn't reach here");
				}
			}
		}
		if(attraction != 0.0f){
			List<Enemy> inRange = Dial.GetAllEnemiesInZone(zone);
			foreach(Enemy e in inRange){
				Vector2 attractionDirection = rt.anchoredPosition - e.GetComponent<RectTransform>().anchoredPosition;
				float distFromTrap = attractionDirection.magnitude;
				Vector2 adjusted = attractionDirection/(distFromTrap*distFromTrap +100f);
				e.GetComponent<Steering>().ExternalForceUpdate(this,adjusted);
			}
		}
	}
	
	void OnTriggerEnter2D(Collider2D coll) { 
		// The following destroys This trap if a newer one is laid on top
		Debug.Log ("entered trigger - TRAP");
		if (coll.gameObject.tag == "Trap") //if a trap is laid over this one
		{
			//Debug.Log ("this trigger is a trap!");
			Trap tc = coll.gameObject.GetComponent <Trap>();
			if (age >= tc.GetAge ()) //if this is the trap laid down first
			{
				Debug.Log ("destroyed old trap!");
				Collide ();
			}
		}
	}
	
	//called when the bullet hits something, from the OnCollisionEnter in EnemyController
	public void Collide(){
		//Add other destruction stuff here
		//gameObject.SetActive (false);
		if(firingDupes){
			//ignore further collisions
			return;
		}
		if(duplicate > 0){
			firingDupes = true;
			duplicateTimer = new Timer();
			FireEffect();
			duplicatedTimes = 0;
		}else{
           	FireEffect();
			usesLeft--;
			if(usesLeft <= 0){
				Debug.Log("successfully destroyed trap");
				Destroy (this.gameObject);
			}
		}
		
		
	}
	void FireEffect(){
		if(aoe > 0f){
            GameObject splashCircle = Instantiate(Resources.Load("Prefabs/MainCanvas/SplashCircle")) as GameObject;
            splashCircle.GetComponent<RectTransform>().anchoredPosition = rt.anchoredPosition;
            splashCircle.transform.SetParent(Dial.spawnLayer.transform, false);
            AoE ac = splashCircle.GetComponent<AoE>();
            ac.scale = aoe;
			ac.aoeDamage = dmg;
            ac.parent = "Trap";
		}
		if(field > 0f){
            GameObject damageField = Instantiate(Resources.Load("Prefabs/MainCanvas/DamageField")) as GameObject;
            damageField.GetComponent<RectTransform>().anchoredPosition = rt.anchoredPosition;
            damageField.transform.SetParent(Dial.spawnLayer.transform, false);
			DamageField df = damageField.GetComponent<DamageField>();
			if(aoe > 0f)
				df.aoeSize = aoe;
			else
				df.aoeSize = 1f;
			df.damagePerTick = dmg * 0.1f;
			df.maxTime = field;
		}
	}
	
	public float GetAge(){
		return age;
	}
	
	public bool CheckActive(){
		return isActive;
	}
}
