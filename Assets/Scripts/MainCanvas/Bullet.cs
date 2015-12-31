using UnityEngine;
using System.Collections;
using System;
using UnityEngine.UI;

public class Bullet : MonoBehaviour {
	
	float TRACK_LENGTH = 110.8f; //hard coded to avoid querying track size all the time
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
	
	
	public float splitDistance = 0f; //radius
	public float splitStartingAngle = 0f; //degrees of where the bullet initially split
	public float splitTravelDist = 0f; //degrees traveled since splitting
	public float splitSpeed = 0f; //degrees per frame
	public float SPLIT_SPEED_DEFAULT = 3f; //how fast to go without speed pieces
	public int splitDirection; //1 or -1 to determine direction
	
	public bool isSplitBullet; //whether it's the result of a split
	public Bullet splitParent; //source of the split bullet
	public Vector3 splitParentPos; //position where the parent hit
	public GameObject splitPivot; //parent object acting as the pivot
	
	public bool timerElapsed;
	public Timer splitTimer;
	
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
		RectTransform sr = (RectTransform)transform;
		float radius = sr.rect.size.x / 2;
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
			transform.SetParent (Dial.spawnLayer, false);
			splitTimer = new Timer();
			timerElapsed = false;
		}
		gameManager = GameObject.Find ("GameManager");
		if (gameManager == null)
		{
			Debug.Log ("zonecone couldn't find GameManager!");
		}
		waveMan = gameManager.GetComponent<WaveManager>();
	}
	
	// Update is called once per frame
	void Update () {
		if(Pause.paused)
			return;
		if (!isSplitBullet)
		{
			RectTransform rt = (RectTransform)transform;
			//Debug.Log ("is not a split bullet!");
			//Debug.Log ("bullet at " + transform.position.x + ", " + transform.position.y);
			//Debug.Log ("velocity: " + vx + ", " + vy);
			//Debug.Log ("homing?" + isHoming);
			if (homingStrength == 0)
			{
				//position doesn't let you modify individual fields so this is gonna be wordy
				rt.anchoredPosition = new Vector2(rt.anchoredPosition.x + vx, rt.anchoredPosition.y + vy);
				//if bullet exceeds its range, disappear
				//Debug.Log ("x is " + transform.position.x + " and spawnx is " + spawnx);
			}
			else if (homingStrength != 0)
			{
				rt.anchoredPosition = new Vector2(rt.anchoredPosition.x + vx, rt.anchoredPosition.y + vy);
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
			
			float distance = (float)Math.Sqrt ((rt.anchoredPosition.x - spawnx) * (rt.anchoredPosition.x - spawnx)
			                                   + (rt.anchoredPosition.y - spawny) * (rt.anchoredPosition.y - spawny));
			//Debug.Log ("distance is " + distance + " and range is " + (range*TRACK_LENGTH) );
			if (distance > (range * TRACK_LENGTH + (Dial.DIAL_RADIUS-spawnDistFromCenter)) - 0.5f) //give a bit of a window for an arcing projectile hitting
			{
				isActive = true;
				collide2D.enabled = true;
			}
			if(distance > range * TRACK_LENGTH + (Dial.DIAL_RADIUS-spawnDistFromCenter)){
				//Debug.Log ("we somehow destroyed ourselves / at (" + rt.anchoredPosition.x + "," + rt.anchoredPosition.y + ")");
				Collide();
				return;
			}
			
			//after moving, check collision with enemies
		}
		
		else if (isSplitBullet)
		{
			RectTransform rt = (RectTransform)transform;
			
			if (!timerElapsed && splitTimer.TimeElapsedMillis () >= 200) //wait until the two splits have separated
			{
				timerElapsed = true;
			}
			
			splitTravelDist += splitSpeed; //increase angle relative to start
			float currentSplitAngle = splitStartingAngle + splitTravelDist; //calculate a fixed angle
			float radians = currentSplitAngle * Mathf.Deg2Rad; //convert to radians
			//change the bullet's rotation to point in the direction it's moving
			transform.eulerAngles = new Vector3(transform.eulerAngles.x,transform.eulerAngles.y,currentSplitAngle + 90f*splitDirection);
			//put the bullet at the location corresponding to its current angle
			rt.anchoredPosition = new Vector2(Mathf.Cos(radians)*splitDistance,Mathf.Sin (radians)*splitDistance);
			
			if(Mathf.Abs(splitTravelDist) > splitCount*60f){ //if you've gone past your max range
				Collide ();
			}
		}
		
	}
	float spawnDistFromCenter = 0f;
	public void UpdateSpawnDist(){
		RectTransform rt = (RectTransform)transform;
		spawnDistFromCenter = Mathf.Sqrt ((spawnx*spawnx)+(spawny*spawny));
	}
	
	void OnTriggerEnter2D(Collider2D coll)
	{
		//Debug.Log ("called triggerenter");
		if (coll.gameObject.tag == "Bullet")
		{
			Bullet bc = coll.gameObject.GetComponent<Bullet>();
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
		GameObject[] enemiesOnScreen = GameObject.FindGameObjectsWithTag("Enemy");
		foreach (GameObject enemy in enemiesOnScreen)
		{
			if (enemy != null)
			{
				Enemy ec = enemy.GetComponent<Enemy>();
				if (ec.GetTrackID() == GetCurrentTrackID()) //if this enemy is in this bullet's zone
				{
					Debug.Log ("FindNearestEnemy found a candidate!");
                    RectTransform rt = (RectTransform)transform;
					float dist = Vector2.Distance(rt.anchoredPosition, ((RectTransform)(enemy.transform)).anchoredPosition);
					if (dist <= minDist)
					{
						Debug.Log ("found a new minEnemy!");
						minDist = dist;
						minEnemy = enemy;
					}
				}
			}
		}
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
		//Debug.Log ("collided (called method Collide())");
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
					AoE ac = splashCircle.GetComponent<AoE>();
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
					GameObject splashCone = Instantiate (Resources.Load ("Prefabs/MainCanvas/SplashCone")) as GameObject;
					splashCone.transform.SetParent(Dial.spawnLayer,false);
					splashCone.transform.position = this.transform.position;
					splashCone.transform.rotation = this.transform.rotation;
					/*splashCone.transform.rotation = new Quaternion(gameObject.transform.rotation.x,
					                                               gameObject.transform.rotation.y,
					                                               gameObject.transform.rotation.z,
					                                               gameObject.transform.rotation.w);*/
					AoE ac = splashCone.GetComponent<AoE>();
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
				
				GameObject split1 = Instantiate (Resources.Load ("Prefabs/MainCanvas/Bullet")) as GameObject;
				Bullet splitbc = split1.GetComponent<Bullet>();
				splitbc.splitDirection = -1;
				splitbc.splitParent = this;
				//splitbc.arcDmg = 5f;
				split1.transform.position = this.transform.position;
				splitbc.SetSplitPropsNew(this);
				
				GameObject split2 = Instantiate (Resources.Load ("Prefabs/MainCanvas/Bullet")) as GameObject;
				Bullet splitbc2 = split2.GetComponent<Bullet>();
				splitbc2.splitDirection = 1;
				splitbc2.splitParent = this;
				split2.transform.position = this.transform.position;
				splitbc2.SetSplitPropsNew(this);
			}
		}
		
		//gameObject.SetActive (false);
		Destroy(gameObject); //why was it destroying the parent???
		/*if (gameObject.transform.parent != null)
		{
			Destroy (gameObject.transform.parent.gameObject);
		}
		else
		{
			Destroy(gameObject);
		}*/
		
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
	
	public void SetSplitPropsNew(Bullet bc){
		RectTransform rt = (RectTransform)bc.transform;
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
		//get radius of where the bullet split
		splitDistance = Mathf.Sqrt(rt.anchoredPosition.x*rt.anchoredPosition.x + rt.anchoredPosition.y*rt.anchoredPosition.y);
		//get andle in degrees of where the bullet split
		splitStartingAngle = Mathf.Atan2(rt.anchoredPosition.y,rt.anchoredPosition.x) * Mathf.Rad2Deg;
		//decide how fast the bullet should move
		splitSpeed = SPLIT_SPEED_DEFAULT * (speed/PieceParser.SPEED_CONSTANT) * splitDirection;
		RectTransform ownRect = (RectTransform)transform;
		//place the new bullet in the right place
		ownRect.anchoredPosition = new Vector2(Mathf.Cos(splitStartingAngle*Mathf.Deg2Rad)*splitDistance,Mathf.Sin (splitStartingAngle*Mathf.Deg2Rad)*splitDistance);
	}
}
