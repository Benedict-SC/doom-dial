using UnityEngine;
using System.Collections;
using System;

public class TrapController : MonoBehaviour {

	//***Skill values begin here***
	public float dmg; //damage dealt out (direct value)
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
	//***Skill values end here***

	private bool isActive; //whether it's armed
	public float maxArmingTime; //max time needed to arm, in seconds
	private float armTime; //current countdown time for arming
	
	public float vx;
	public float vy;
	public float spawnx;
	public float spawny;
	
	// Use this for initialization
	void Start () {
		dmg = 34; //test value
		armTime = maxArmingTime;
		isActive = false;
		SpriteRenderer sr = transform.gameObject.GetComponent<SpriteRenderer> ();
		float radius = sr.bounds.size.x / 2;
		CircleCollider2D collider = transform.gameObject.GetComponent<CircleCollider2D> ();
		collider.radius = radius;
		//Debug.Log ("bullet radius is: " + radius);
	}
	
	// Update is called once per frame
	void Update () {
		//traps don't move so no changes in its position after placement
		//they need to count down to become active
		if (armTime > 0.0f) //if it's still counting down
		{
			armTime -= Time.deltaTime; //update arming countdown time
			if (armTime <= 0.0f) //if it's done counting down, activate the trap
			{
				isActive = true;
			}
		}

		if (isActive)
		{
			//check for enemy collisions
		}



	}
	
	//called when the bullet hits something, from the OnCollisionEnter in EnemyController
	public void Collide(){
		if (isActive)
		{
			Destroy (this.gameObject);
		}

	}
}
