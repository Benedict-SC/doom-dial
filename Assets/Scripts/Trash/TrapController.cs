using UnityEngine;
using System.Collections;
using System;

public class TrapController : MonoBehaviour {

	float TRACK_LENGTH = 3.1f; //hard coded to avoid querying track size all the time

	//***Skill values begin here***

	//IMPLEMENTED
	public float dmg; //damage dealt out (direct value)
	public float range; //range -- expressed in percent of the length of the lane
	public float knockback; //knockback
	public float poison; //poison damage on enemy
	public float poisonDur; //how long poison lasts, in seconds
	public float stun; //amount (time?) of enemy stun
	public float lifeDrain; //lifedrain on enemy
	public float slowdown; //enemy slowdown -- scale of 1 to 10, can't go over 8
	public float slowDur; //how long slowdown lasts
	public float splash; //percent of effects to transfer to enemy affected
	public float splashRad; //radius of splash dmg

	//NOT YET IMPLEMENTED

	public float penetration; //ignores this amount of enemy shield
	public float shieldShred; //lowers enemy shield's max value by this
	//***Skill values end here***

	private bool isActive; //whether it's armed
	public float maxArmingTime; //max time needed to arm, in seconds
	private float armTime; //current countdown time for arming

	public float spawnx;
	public float spawny;

	public GameObject enemyHit; //for use by AoE

	private float age; //age of the trap -- used to determine which of two overlaid traps to destroy
	
	// Use this for initialization
	void Start () {
		age = 0.0f;
		dmg = 34; //test value
		armTime = maxArmingTime;
		isActive = false;
		SpriteRenderer sr = transform.gameObject.GetComponent<SpriteRenderer> ();
		float radius = sr.bounds.size.x / 2;
		CircleCollider2D collider = transform.gameObject.GetComponent<CircleCollider2D> ();
		collider.radius = radius;
		//Debug.Log ("bullet radius is: " + radius);
		//set its position
		this.transform.position = new Vector3 (spawnx, spawny, this.transform.position.z);
	}
	
	// Update is called once per frame
	void Update () {
		age += Time.deltaTime; //update the age

		if (armTime <=0.0f && !isActive)
		{
			//Debug.Log ("armTime is 0 or less! nuuuu");
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
		if (isActive)
		{
			//check for enemy collisions
		}
	}

	void OnTriggerEnter2D(Collider2D coll) { 
		// The following destroys This trap if a newer one is laid on top
		//Debug.Log ("entered trigger - TRAP");
		if (coll.gameObject.tag == "Trap") //if a trap is laid over this one
		{
			//Debug.Log ("this trigger is a trap!");
			TrapController tc = coll.gameObject.GetComponent <TrapController>();
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
		if (splash != 0)
		{
			if (poison != 0)
			{
				//POISON CLOUD HERE
			}
			else {
				//AoE DAMAGE HERE
				//Debug.Log ("got to splash");
				knockback = 0f; //to avoid stacking knockback
				stun = 0f; //to avoid stacking stun effect
				penetration = 0f;
				GameObject splashCircle = Instantiate (Resources.Load ("Prefabs/SplashCircle")) as GameObject;
				splashCircle.transform.position = this.transform.position;
				AoEController ac = splashCircle.GetComponent<AoEController>();
				ac.scale = splashRad;
				ac.parent = "Trap";
				ac.aoeTrapCon = this;
				ac.ScaleProps (splash);
			}
		}
		Destroy (this.gameObject);
	}

	public float GetAge(){
		return age;
	}

	public bool CheckActive(){
		return isActive;
	}
}
