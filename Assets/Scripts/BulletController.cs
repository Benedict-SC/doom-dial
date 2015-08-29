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
	public float speed; //speed of the bullet -- used for split bullets
	public float poison; //poison damage on enemy
	public float poisonDur; //how long poison lasts, in seconds
	public float knockback; //knockback -- positive value for distance knocked back
	public float stun; //amount (time?) of enemy stun
	public float lifeDrain; //lifedrain on enemy
	public float slowdown; //enemy slowdown
	public float slowDur; //how long slowdown lasts
	public float splash; //percentage of fx to transfer to hits
	public float splashRad; //radius of splash damage
	public float arcDmg; //dmg bonus from arcing - if above 0 it arcs

	public bool isSplitBullet; //whether it's the result of a split
	public BulletController splitParent; //source of the split bullet
	public int dirScalar; //1 or -1 to determine direction
	public float splitRadius; //distance from the center to the split bullet
	public float angleLimit; //max range of a split bullet, in radians

	//BEING WORKED ON

	public float splitCount; //number of pieces it splits into
	public float penetration; //ignores this amount of enemy shield
	public float shieldShred; //lowers enemy shield's max value by this
	public float homingStrength; //strength of homing effect

	/*
	 * End of attributes passed from tower
	 */

	private bool isActive = true; //for use by arcing projectiles

	public float vx;
	public float vy;
	public float spawnx;
	public float spawny;

	public GameObject enemyHit; //for use by AoE

	CircleCollider2D collide2D;

	// Use this for initialization
	void Start () {
		SpriteRenderer sr = transform.gameObject.GetComponent<SpriteRenderer> ();
		float radius = sr.bounds.size.x / 2;
		CircleCollider2D collider = transform.gameObject.GetComponent<CircleCollider2D> ();
		collider.radius = radius;
		collide2D = collider;
		//Debug.Log ("bullet radius is: " + radius);
		if (arcDmg > 0)
		{
			isActive = false;
			collider.enabled = false;
		}
		if (isSplitBullet)
		{
			//Debug.Log ("started split bullet");
		}
	}
	
	// Update is called once per frame
	void Update () {
		if (!isSplitBullet)
		{
			//Debug.Log ("is not a split bullet!");
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
			if (distance > (range * TRACK_LENGTH) - 0.5f) //give a bit of a window for an arcing projectile hitting
			{
				isActive = true;
				collide2D.enabled = true;
			}
			if(distance > range * TRACK_LENGTH){
				Debug.Log ("we somehow destroyed ourselves / at (" + transform.position.x + "," + transform.position.y + ")");
				Collide (); //destroys itself and begins any post-death status effects
				return;
			}
			
			//after moving, check collision with enemies
		}
		else if (isSplitBullet) //splitbullet movement behavior
		{
			Debug.Log ("split bullet behavior");
			float x;
			float y;
			float angle = Mathf.Atan2 (transform.position.y,transform.position.x);

			angle += speed * .3f * dirScalar; //constant scalar

			x = splitRadius * Mathf.Cos (angle);
			y = splitRadius * Mathf.Sin (angle);
			transform.position = new Vector3(x, y, transform.position.z);

			if (angle < 0f)
			{
				angle += 2*Mathf.PI;
			}

			if (dirScalar < 0) //clockwise -- only works for negative angles atm??
			{
				if (angle <= angleLimit)
				{
					Debug.Log ("angle:" + angle);
					Debug.Log ("angleLimit:" + angleLimit);
					Debug.Log ("split max range");
					Collide ();
				}
			}
			else if (dirScalar > 0) //counterclockwise -- only works for positive angles atm??
			{
				Debug.Log ("angle:" + angle);
				Debug.Log ("angleLimit:" + angleLimit);
				if (angle >= angleLimit) //max range
				{
					Collide ();
				}
			}

		}

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
					/*Debug.Log ("old splash damage: " + dmg);
					Debug.Log ("old splash lifedrain: " + lifeDrain);*/
					//Debug.Log ("splashRad is " + splashRad);
					ac.parent = "Bullet";
					ac.aoeBulletCon = this;
					ac.ScaleProps(splash);
					/*Debug.Log ("new splash damage: " + ac.aoeBulletCon.dmg);
					Debug.Log ("new splash life: " + ac.aoeBulletCon.lifeDrain);*/
				}
			}
		}
		Debug.Log ("splitCount is " + splitCount);
		if (splitCount > 0)
		{
			if (!isSplitBullet) //only pre-split bullets
			{
				Debug.Log ("spawned 2 split bullets");
				ScaleProps(0.5f);

				GameObject split1 = Instantiate (Resources.Load ("Prefabs/Bullet")) as GameObject;
				BulletController splitbc = split1.GetComponent<BulletController>();
				splitbc.dirScalar = -1;
				splitbc.splitParent = this;
				splitbc.arcDmg = 5f;
				split1.transform.position = this.transform.position;
				splitbc.SetSplitProps(this);

				GameObject split2 = Instantiate (Resources.Load ("Prefabs/Bullet")) as GameObject;
				BulletController splitbc2 = split2.GetComponent<BulletController>();
				splitbc2.dirScalar = 1;
				splitbc2.splitParent = this;
				split2.transform.position = this.transform.position;
				splitbc2.SetSplitProps(this);
			}
		}
		//gameObject.SetActive (false);
		Destroy(gameObject);
	}

	public bool CheckActive(){
		return isActive;
	}

	void ScaleProps(float pc)
	{
		dmg *= pc; //damage dealt out
		poison *= pc; //poison damage on enemy
		stun *= pc; //amount (time?) of enemy stun
		lifeDrain *= pc; //lifedrain on enemy
		slowdown *= pc; //enemy slowdown
		splash *= pc; //percentage of fx to transfer to hits
		splashRad *= pc; //radius of splash damage
		penetration *= pc; //ignores this amount of enemy shield
		shieldShred *= pc; //lowers enemy shield's max value by this
	}

	public void SetSplitProps(BulletController bc)
	{
		dmg = bc.dmg; //damage dealt out
		speed = bc.speed; //speed of the bullet
		poison = bc.poison; //poison damage on enemy
		poisonDur = bc.poisonDur; //how long poison lasts, in seconds
		knockback = 0f; //knockback -- positive value for distance knocked back
		stun = bc.stun; //amount (time?) of enemy stun
		lifeDrain = bc.lifeDrain; //lifedrain on enemy
		slowdown = bc.slowdown; //enemy slowdown
		slowDur = bc.slowDur; //how long slowdown lasts
		splash = bc.splash; //percentage of fx to transfer to hits
		splashRad = bc.splashRad; //radius of splash damage
		splitCount = bc.splitCount; //split-result bullets don't split themselves
		penetration = bc.penetration; //ignores this amount of enemy shield
		shieldShred = bc.shieldShred; //lowers enemy shield's max value by this
		homingStrength = 0f; //strength of homing effect
		arcDmg = 0f; //dmg bonus from arcing - if above 0 it arcs
		isSplitBullet = true; //is the result of a split
		splitRadius = Mathf.Abs(transform.position.x / Mathf.Cos(Mathf.Atan2 (transform.position.y,transform.position.x)));
		Debug.Log ("splitRadius is " + splitRadius);
		angleLimit = Mathf.Atan2 (transform.position.y,transform.position.x) + (splitCount * dirScalar * ((Mathf.PI)/3f));
		if (angleLimit < 0f)
		{
			angleLimit += 2*Mathf.PI;
		}
	}

	public void SetSplitDist()
	{
		//splitCenterDist = [calculate distance from center]
	}

}
