/*Thom*/

using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;
using UnityEngine.UI;

public class Trap : Weapon {
	
	float TEMP_MAX_ATTRACTION = 10f;

	float TRACK_LENGTH = 3.1f; //hard coded to avoid querying track size all the time
	public float baseDamage = 8f;
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

	protected RectTransform rt;
	
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
				Vector2 attractionVector = rt.anchoredPosition - e.GetComponent<RectTransform>().anchoredPosition;
				Vector2 direction = attractionVector.normalized;
				float distFromTrap = attractionVector.magnitude;
				Steering s = e.GetComponent<Steering>();
				float strength = s.maxAccel * 0.99f; //baseline maximum
				float percentOfMax = attraction/TEMP_MAX_ATTRACTION; //use attraction number to get how close to the max attraction you're at
				strength *=  percentOfMax; //multiply to get acceleration
				Vector2 adjusted = direction * strength; //then apply that to the direction vector
				s.ExternalForceUpdate(this,adjusted); //and tell the AI to be pulled that way
			}
		}
	}
	
	void OnTriggerEnter2D(Collider2D coll) { 
		// The following destroys This trap if a newer one is laid on top
		Debug.Log ("entered trigger - TRAP");
		if (coll.gameObject.tag == "Trap" || coll.gameObject.tag == "ProjectileTrap") //if a trap is laid over this one
		{
			//Debug.Log ("this trigger is a trap!");
			Trap tc = coll.gameObject.GetComponent <Trap>();
			if (age <= tc.GetAge ()) //if this is the trap laid down second - prioritize the original
			{
                Destroy(this.gameObject);
			}
		}
        else if (coll.gameObject.tag == "Bullet")
        {
            Bullet bc = coll.gameObject.GetComponent<Bullet>();
            if (bc.comboKey > 0f || comboKey > 0f)
            {
                //TODO - chance/odds of the following happening?
                //Bullet hitting Trap combo effect
                BulletTrapComboEffects(bc);
            }
        }
	}

    //Bullet/Trap combo effects
    void BulletTrapComboEffects(Bullet bc)
    {
        //(From the spreadsheet)
        //Trap explodes with the properties of the bullet

        //Damage
        dmg += bc.dmg;
        //Charge (size of aoe)
        aoe += bc.charge;
        //Split (multiple explosions)
        //TODO
        //Penetration (it's an aoe, ignores shields)
        //TODO
        //Continuous (leaves a damage-over-time field)
        field += bc.continuousStrength;
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
	protected virtual void FireEffect(){
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
