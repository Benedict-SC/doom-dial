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
	public Vector3 splitParentPos; //position where the parent hit
	public GameObject splitPivot; //parent object acting as the pivot
	public int dirScalar; //1 or -1 to determine direction
	public float splitRadius; //distance from the center to the split bullet
	public float angleLimit; //max range of a split bullet, in radians
	public Vector3 angleLimitVect; //^^^ as a quaternion
	private Vector3 zeroRotate = new Vector3 (0f,0f,0f);
	private float lerpTime;
	public bool timerElapsed;
	private float currentLerpTime = 0f;
	public float LERP_TIME_CONSTANT;
	public Timer splitTimer;
	bool originalAngleSet = false;
	float originalAngle = 0f;

	public GameObject homingTarget;
	GameObject gameManager;
	WaveManager waveMan;

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

	//var to stop the bullet during pause
	bool isPaused = false;

	// Use this for initialization
	void Start () {
		SpriteRenderer sr = transform.gameObject.GetComponent<SpriteRenderer> ();
		float radius = sr.bounds.size.x / 2;
		CircleCollider2D collider = transform.gameObject.GetComponent<CircleCollider2D> ();
		collider.radius = radius;
		if (isSplitBullet)
		{
			collider.radius *= 2f;
		}
		collide2D = collider;
		//Debug.Log ("bullet radius is: " + radius);
		if (arcDmg > 0)
		{
			isActive = false;
			collider.enabled = false;
		}
		if (isSplitBullet)
		{
			splitPivot = Instantiate (Resources.Load ("Prefabs/SplitPivot")) as GameObject;
			splitPivot.transform.position = new Vector3(0f,0f,0f);
			splitPivot.transform.rotation = new Quaternion (0f,0f,0f,0f);
			transform.SetParent (splitPivot.transform, true);
			LERP_TIME_CONSTANT = .34f; //CONSTANT - time to travel across one lane
			lerpTime = splitCount * LERP_TIME_CONSTANT;
			Debug.Log ("lerpTime is " + lerpTime);
			//StartCoroutine("splitMovement");
			splitTimer = new Timer();
			timerElapsed = false;
			splitTimer.Restart ();
		}
		gameManager = GameObject.Find ("GameManager");
		if (gameManager == null)
		{
			Debug.Log ("zoneconecontroller couldn't find GameManager!");
		}
		waveMan = gameManager.GetComponent<WaveManager>();
	}
	
	// Update is called once per frame
	void Update () {
		isPaused = GamePause.paused;
		if (!isSplitBullet)
		{
			//Debug.Log ("is not a split bullet!");
			//Debug.Log ("bullet at " + transform.position.x + ", " + transform.position.y);
			//Debug.Log ("velocity: " + vx + ", " + vy);
			//Debug.Log ("homing?" + isHoming);
			if (homingStrength == 0)
			{
				//position doesn't let you modify individual fields so this is gonna be wordy
				if(!isPaused){
					//Stops bullet during pause
				this.transform.position = new Vector3(this.transform.position.x + vx, this.transform.position.y + vy, this.transform.position.z);
				}
				//if bullet exceeds its range, disappear
				//Debug.Log ("x is " + transform.position.x + " and spawnx is " + spawnx);
			}
			else if (homingStrength != 0)
			{
				if (!isPaused)
				{
					//edit this somehow
					this.transform.position = new Vector3(this.transform.position.x + vx, this.transform.position.y + vy, this.transform.position.z);
				}
				if (homingTarget == null)
				{
					SetHomingTarget();
				}
				else if (homingTarget != null)
				{
					Vector3 homeDir = Vector3.Lerp (this.transform.position, homingTarget.transform.position, homingStrength);
					Debug.Log ("homeDir: " + homeDir.ToString());
					//homeDir *= homingStrength;
					Debug.Log ("vx: " + vx);
					Debug.Log ("vy: " + vy);
					this.transform.position += homeDir - this.transform.position;
					Debug.Log ("new vx: " + vx);
					Debug.Log ("new vy: " + vy);
				}
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
				Collide();
				return;
			}
			
			//after moving, check collision with enemies
		}

		else if (isSplitBullet)
		{
			//stops bullet during pause
			if(!isPaused)
			{
				transform.position = new Vector3(transform.position.x, transform.position.y, transform.position.z);
			}
			if (!timerElapsed && splitTimer.TimeElapsedMillis () >= 200) //wait until the two splits have separated
			{
				timerElapsed = true;
			}
			
			if (!originalAngleSet)
			{
				originalAngle = splitPivot.transform.eulerAngles.z;
				originalAngleSet = true;
			}
			if (splitPivot.transform.eulerAngles.z + 2.0f * dirScalar < splitPivot.transform.eulerAngles.z + dirScalar * angleLimitVect.z)
			{
				//hopefully rotating the splitPivot works?
				splitPivot.transform.eulerAngles = Vector3.Lerp (zeroRotate, angleLimitVect, (currentLerpTime + Time.deltaTime)/(lerpTime));
				//Debug.Log ("splitPivot rotation is " + splitPivot.transform.rotation.ToString());
				currentLerpTime += Time.deltaTime;
			}
			if (dirScalar < 0)
			{
				//Debug.Log ("angle: " + (splitPivot.transform.eulerAngles.z - 2.0f));
				//Debug.Log ("limit angle: " + (Mathf.Abs ((originalAngle + dirScalar * angleLimitVect.z) - 360f)));
				float addValue = 0.0f;
				if (splitCount == 1 || splitCount == 2)
				{
					addValue = 2.0f;
				}
				
				if (splitPivot.transform.eulerAngles.z - addValue <= Mathf.Abs ((originalAngle + dirScalar * angleLimitVect.z) - 360f))
				{
					Debug.Log (" negative splits destroying themselves due to range");
					Collide ();
				}
			}
			else if (dirScalar > 0)
			{
				float addValue = 0.0f;
				if (splitCount == 1 || splitCount == 2)
				{
					addValue = 2.0f;
				}
				if (splitPivot.transform.eulerAngles.z + addValue >= originalAngle + dirScalar * angleLimitVect.z)
				{
					Debug.Log ("splits destroying themselves due to range");
					Collide ();
				}
			}
		}

	}

	void OnTriggerEnter2D(Collider2D coll)
	{
		//Debug.Log ("called triggerenter");
		if (coll.gameObject.tag == "Bullet")
		{
			BulletController bc = coll.gameObject.GetComponent<BulletController>();
			if (bc.isSplitBullet)
			{
				if (isSplitBullet)
				{
					if (bc.splitParent == this.splitParent)
					{
						Debug.Log ("the two splits collided!");
						//Determine lane ID and spawn aoe in appropriate lane
						if (timerElapsed) //just to make sure they don't destroy each other on spawn
						{
							GameObject zoneCone = null;

							Debug.Log ("current track is " + GetCurrentTrackID());

							switch (GetCurrentTrackID())
							{
							case 1:
								zoneCone = GameObject.Find ("ZoneCone1");
								break;
							case 2:
								zoneCone = GameObject.Find ("ZoneCone2");
								break;
							case 3:
								zoneCone = GameObject.Find ("ZoneCone3");
								break;
							case 4:
								zoneCone = GameObject.Find ("ZoneCone4");
								break;
							case 5:
								zoneCone = GameObject.Find ("ZoneCone5");
								break;
							case 6:
								zoneCone = GameObject.Find ("ZoneCone6");
								break;
							default:
								Debug.Log("Couldn't find ZoneCone of this ID...not 1-6?");
								break;
							}

							if (zoneCone != null)
							{
								ZoneConeController zcc = zoneCone.GetComponent<ZoneConeController>();
								zcc.StartCoroutine("Detonate");
							}
							else{
								Debug.Log ("zoneCone is null for some reason?");
							}

							bc.Collide ();
							Collide ();
						}
					}
				}
				else if (!isSplitBullet) //your own bullets can stop split-offs!
				{
					bc.Collide ();
					Collide ();
				}
			}
		}
	}

	//returns the nearest enemy in this bullet's zone
	public GameObject FindNearestEnemy()
	{
		float minDist = 9999f;
		GameObject minEnemy = null;
		Debug.Log("HEY I HAD TO BREAK THIS METHOD BECAUSE WAVEMANAGER DOESN'T KNOW ABOUT ALL ENEMIES ANYMORE");
		/*foreach (GameObject enemy in waveMan.enemiesOnscreen)
		{
			if (enemy != null)
			{
				EnemyController ec = enemy.GetComponent<EnemyController>();
				if (ec.GetTrackID() == GetCurrentTrackID()) //if this enemy is in this bullet's zone
				{
					Debug.Log ("FindNearestEnemy found a candidate!");
					float dist = Vector3.Distance(this.transform.position, enemy.transform.position);
					if (dist <= minDist)
					{
						Debug.Log ("found a new minEnemy!");
						minDist = dist;
						minEnemy = enemy;
					}
				}
			}
		}*/
		return minEnemy;
	}

	public void SetHomingTarget()
	{
		homingTarget = FindNearestEnemy();
	}

	//returns the current zone ID of This bullet
	public int GetCurrentTrackID(){
		float degrees = ((360-Mathf.Atan2(transform.position.y,transform.position.x) * Mathf.Rad2Deg)+90 + 360)%360;
		Debug.Log(degrees);
		if(degrees >= 30.0 && degrees < 90.0){
			return 2;
		}else if(degrees >= 90.0 && degrees < 150.0){
			return 3;
		}else if(degrees >= 150.0 && degrees < 210.0){
			return 4;
		}else if(degrees >= 210.0 && degrees < 270.0){
			return 5;
		}else if(degrees >= 270.0 && degrees < 330.0){
			return 6;
		}else if(degrees >= 330.0 || degrees < 30.0){
			return 1;
		}else{
			//what the heck, this shouldn't happen
			Debug.Log ("What the heck, this shouldn't happen");
			return 0;
		}
	}

	//called when the bullet hits something, from the OnCollisionEnter in EnemyController
	public void Collide()
	{
		Debug.Log ("collided (called method Collide())");
		if (splash != 0)
		{
			if (poison != 0)
			{
				//POISON CLOUD HERE
			}
			else 
			{
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
		//Debug.Log ("splitCount is " + splitCount);
		if (splitCount > 0)
		{
			if (!isSplitBullet) //only pre-split bullets
			{
				//Debug.Log ("spawned 2 split bullets");
				ScaleProps(0.5f);

				GameObject split1 = Instantiate (Resources.Load ("Prefabs/Bullet")) as GameObject;
				BulletController splitbc = split1.GetComponent<BulletController>();
				splitbc.dirScalar = -1;
				splitbc.splitParent = this;
				//splitbc.arcDmg = 5f;
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
		if (gameObject.transform.parent != null)
		{
			Destroy (gameObject.transform.parent.gameObject);
		}
		else
		{
			Destroy(gameObject);
		}

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
		//Debug.Log ("splitRadius is " + splitRadius);
		angleLimit = splitCount * dirScalar * ((Mathf.PI)/3f);
		float superAddValue = 0f;
		/*if (splitCount == 3)
		{
			superAddValue = 6f;
		}*/
		angleLimitVect = new Vector3 (0f, 0f, ((angleLimit * 57.296f) + superAddValue)); //rad to deg constant
		splitParentPos = bc.transform.position;
		//Debug.Log ("angleLimit is " + angleLimit);
		//Debug.Log ("angleLimit as degrees is " + angleLimit * 57.296f);
		//Debug.Log ("angleLimitQuat is " + angleLimitVect.ToString ());
	}

	public void SetSplitDist()
	{
		//splitCenterDist = [calculate distance from center]
	}
}
