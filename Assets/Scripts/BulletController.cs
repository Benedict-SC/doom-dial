using UnityEngine;
using System.Collections;
using System;

public class BulletController : MonoBehaviour {

	float TRACK_LENGTH = 3.02f; //hard coded to avoid querying track size all the time
	// ^^^ RELATIVE TO WHERE BULLET STARTS, NOT CENTER

	//For bullet types (as opposed to traps and shield types)

	/* The following are stats/skills for bullets
	 * They are passed to the bullet object from the customized tower
	 */

	//IMPLEMENTED
	public float dmg; //damage dealt out
	public float range; //range -- expressed in percent of the length of the lane
	public float speed; //possibly not necessary, as the speed is passed from the GunController
	public float poison; //poison damage on enemy
	public float poisonDur; //how long poison lasts, in seconds
	public float knockback; //knockback -- positive value for distance knocked back
	public float stun; //amount (time?) of enemy stun
	public float lifeDrain; //lifedrain on enemy
	public float slowdown; //enemy slowdown
	public float slowDur; //how long slowdown lasts
	public float splash; //percentage of fx to transfer to hits
	public float splashRad; //radius of splash damage

	//BEING WORKED ON

	public float splitCount; //number of pieces it splits into
	public float penetration; //ignores this amount of enemy shield
	public float shieldShred; //lowers enemy shield's max value by this
	public float homingStrength; //strength of homing effect
	public float arcDmg; //dmg bonus from arcing - if above 0 it arcs
	/*
	 * End of attributes passed from tower
	 */

	private bool isActive; //for use by arcing projectiles

	public float vx;
	public float vy;
	public float spawnx;
	public float spawny;

	public GameObject enemyHit; //for use by AoE

	// Use this for initialization
	void Start () {
		SpriteRenderer sr = transform.gameObject.GetComponent<SpriteRenderer> ();
		float radius = sr.bounds.size.x / 2;
		CircleCollider2D collider = transform.gameObject.GetComponent<CircleCollider2D> ();
		collider.radius = radius;
		//Debug.Log ("bullet radius is: " + radius);
	}
	
	// Update is called once per frame
	void Update () {
		//Debug.Log ("bullet at " + transform.position.x + ", " + transform.position.y);
		//Debug.Log ("velocity: " + vx + ", " + vy);
		//Debug.Log ("homing?" + isHoming);
		if (homingStrength == 0)
		{
			//position doesn't let you modify individual fields so this is gonna be wordy
			this.transform.position = new Vector3(this.transform.position.x + vx, this.transform.position.y + vy, this.transform.position.z);
			//if bullet exceeds its range, disappear
			//Debug.Log ("x is " + transform.position.x + " and spawnx is " + spawnx);
		}
		
		float distance = (float)Math.Sqrt ((this.transform.position.x - spawnx) * (this.transform.position.x - spawnx)
						+ (this.transform.position.y - spawny) * (this.transform.position.y - spawny));
		//Debug.Log ("distance is " + distance + " and range is " + (range*TRACK_LENGTH) );
		if(distance > range * TRACK_LENGTH){
			Debug.Log ("we somehow destroyed ourselves / at (" + transform.position.x + "," + transform.position.y + ")");
			Collide (); //destroys itself and begins any post-death status effects
			return;
		}

		//after moving, check collision with enemies

	}
	//called when the bullet hits something, from the OnCollisionEnter in EnemyController
	public void Collide(){
		Debug.Log ("collided");
		if (splash != 0)
		{
			if (poison != 0)
			{
				//POISON CLOUD HERE
			}
			else {
				//AoE DAMAGE HERE
				if (arcDmg > 0) //circle splash
				{
					//Debug.Log ("got to splash");
					knockback = 0f;
					stun = 0f;
					penetration = 0f;
					GameObject splashCircle = Instantiate (Resources.Load ("Prefabs/SplashCircle")) as GameObject;
					splashCircle.transform.position = this.transform.position;
					AoEController ac = splashCircle.GetComponent<AoEController>();
					ac.scale = splashRad;
					//Debug.Log ("splashRad is " + splashRad);
					ac.parent = "Bullet";
					dmg += arcDmg;
					ac.aoeBulletCon = this;
					ac.ScaleProps(splash);
				}
				else //normal cone splash
				{
					knockback = 0f;
					stun = 0f;
					penetration = 0f;
					GameObject splashCone = Instantiate (Resources.Load ("Prefabs/SplashCone")) as GameObject;
					splashCone.transform.position = this.transform.position;
					splashCone.transform.rotation = this.transform.rotation;
					/*splashCone.transform.rotation = new Quaternion(gameObject.transform.rotation.x,
					                                               gameObject.transform.rotation.y,
					                                               gameObject.transform.rotation.z,
					                                               gameObject.transform.rotation.w);*/
					AoEController ac = splashCone.GetComponent<AoEController>();
					ac.scale = splashRad;
					Debug.Log ("old splash damage: " + dmg);
					Debug.Log ("old splash lifedrain: " + lifeDrain);
					//Debug.Log ("splashRad is " + splashRad);
					ac.parent = "Bullet";
					ac.aoeBulletCon = this;
					ac.ScaleProps(splash);
					Debug.Log ("new splash damage: " + ac.aoeBulletCon.dmg);
					Debug.Log ("new splash life: " + ac.aoeBulletCon.lifeDrain);
				}
			}
		}
		//gameObject.SetActive (false);
		Destroy(gameObject);
	}

	public bool CheckActive(){
		return true;
	}

}
