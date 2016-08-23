/*Some Thom, some Duncan*/

using UnityEngine;
using System.Collections;
using System;
using UnityEngine.UI;

public class Bullet : MonoBehaviour {
	
	float TRACK_LENGTH = 110.8f + 5; //hard coded to avoid querying track size all the time
	// ^^^ RELATIVE TO WHERE BULLET STARTS, NOT CENTER
	
	//For bullet types (as opposed to traps and shield types)
	
	/* The following are stats/skills for bullets
	 * They are passed to the bullet object from the customized tower
	 */
	
	//IMPLEMENTED
	public float dmg; //damage dealt out
	public float range; //range -- expressed in percent of the length of the lane
	public float speed; //speed of the bullet -- (also) used for split bullets
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
	
	public bool multiSplits = false;
	float alignmentSpacing = 35f;//Dial.middle_radius-Dial.inner_radius;
	public Timer alignTimer = null;
	float ALIGNMENT_TIME = 1.1f;
	public float alignmentProgress = 0f;
	public int alignCode = 0; //0 is middle, 1 is outer, -1 is inner
	public float initialAlignRadius = 0f;
	
	public bool isSplitBullet; //whether it's the result of a split
	public Bullet splitParent; //source of the split bullet
	public Vector3 splitParentPos; //position where the parent hit
	public GameObject splitPivot; //parent object acting as the pivot
	
	public bool timerElapsed;
	public Timer splitTimer;
	
	public int spreadCode = 0;
	
	public GameObject homingTarget;
	float homingStrengthConstant = 6f;
	
	Image bulletImg;
	/*GameObject gameManager;
	WaveManager waveMan;*/ //why were these here? bullets don't need to know about wave timing
	
	//BEING WORKED ON
	
	public float splitCount; //number of pieces it splits into
	public float penetration; //ignores this amount of enemy shield
	public float shieldShred; //lowers enemy shield's max value by this
	public float homingStrength; //strength of homing effect
	
	public bool chainsPoison = false;
	public float slowsShields = 0f;
	public bool leeches = false;
	
	public bool pierces = false;
	public int piercesLeft = 0;
	
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
		bulletImg = GetComponent<Image>();
		
		float radius = sr.rect.size.x / 2;
		CircleCollider2D collider = transform.gameObject.GetComponent<CircleCollider2D> ();
		collider.radius = radius;
		
		/*if (isSplitBullet) //what the heck???
		{
			collider.radius *= 2f;
		}*/
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
		/*gameManager = GameObject.Find ("GameManager");
		if (gameManager == null)
		{
			Debug.Log ("zonecone couldn't find GameManager!");
		}
		waveMan = gameManager.GetComponent<WaveManager>();*/
	}
	
	// Update is called once per frame
	void Update () {
		if(Pause.paused)
			return;
		if(arcFalling){
			ArcBulletFallingUpdate();
			return;
		}
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
			else if (homingStrength != 0 && arcDmg == 0)
			{
				rt.anchoredPosition = new Vector2(rt.anchoredPosition.x + vx, rt.anchoredPosition.y + vy);
				if (homingTarget == null)
				{
					SetHomingTarget();
				}
				else if (homingTarget != null)
				{
					float speedMult = speed/PieceParser.SPEED_CONSTANT;
					Vector2 targPos = homingTarget.GetComponent<RectTransform>().anchoredPosition;
					Vector2 homeDir = targPos - rt.anchoredPosition; //direction to home in
					homeDir = homeDir.normalized * homingStrengthConstant * speedMult;
					Debug.Log ("homeDir: " + homeDir.ToString());
					homeDir *= homingStrength;
					//Debug.Log ("vx: " + vx);
					//Debug.Log ("vy: " + vy);
					vx += homeDir.x;
					vy += homeDir.y;
					//Debug.Log ("new vx: " + vx);
					//Debug.Log ("new vy: " + vy);
				}
			}
			
			float distance = (float)Math.Sqrt ((rt.anchoredPosition.x - spawnx) * (rt.anchoredPosition.x - spawnx)
			                                   + (rt.anchoredPosition.y - spawny) * (rt.anchoredPosition.y - spawny));
			if(arcDmg > 0 && homingStrength == 0){
				float prog = distance / (range * TRACK_LENGTH + (Dial.DIAL_RADIUS-spawnDistFromCenter));
				float midFarness = Math.Abs(0.5f-prog);
				midFarness *= 2;
				float height = 1f - midFarness;//float 0 to 1 representing the percent of the bullet's "max height"
				transform.localScale = new Vector3(1f+height,1f+height,1);
				bulletImg.color = new Color(bulletImg.color.r,bulletImg.color.g,bulletImg.color.b,0.5f+(midFarness*0.5f));
			}
			//Debug.Log ("distance is " + distance + " and range is " + (range*TRACK_LENGTH) );
			if(distance > range * TRACK_LENGTH + (Dial.DIAL_RADIUS-spawnDistFromCenter)){
				if(arcDmg > 0){
					isActive = true;
					collide2D.enabled = true;
					arcFallTimer = new Timer();
					arcFalling = true;
				}else{
					//Debug.Log ("we somehow destroyed ourselves / at (" + rt.anchoredPosition.x + "," + rt.anchoredPosition.y + ")");
					piercesLeft = 0;
					Collide();
					return;
				}
			}
			
			if(arcDmg > 0 && homingStrength > 0){//arc/homing combo
				float maxrange = range * TRACK_LENGTH + (Dial.DIAL_RADIUS-spawnDistFromCenter);
				float modifiedHomingStrength = (.25f/.015f)*homingStrength; 
				float maxDeviation = modifiedHomingStrength*(Mathf.PI/3f);
				Debug.Log ("homing strength was " + homingStrength);
				Debug.Log ("max deviation was " + maxDeviation + " radians");
				SetHomingTarget();
				Vector2 targetPos;
				if(homingTarget == null){
					targetPos = new Vector2(spawnx,spawny);
				}else{
					targetPos = homingTarget.GetComponent<RectTransform>().anchoredPosition;
				}
				
				float angle = Mathf.Atan2(spawny,spawnx);
				float targetAngle = Mathf.Atan2(targetPos.y,targetPos.x);
				float difference = targetAngle - angle;
				if(Mathf.Abs(difference) <= maxDeviation){
					angle += difference;
				}else{
					if(difference > 0){
						angle += maxDeviation;
					}else{
						angle -= maxDeviation;
					}
				}
				rt.anchoredPosition = new Vector2(Mathf.Cos(angle)*maxrange,Mathf.Sin (angle)*maxrange);
				isActive = true;
				collide2D.enabled = true;
				arcFallTimer = new Timer();
				arcFalling = true;
			}
			
			//after moving, check collision with enemies
		}
		
		else if (isSplitBullet)
		{
			RectTransform rt = (RectTransform)transform;
			
			if (!timerElapsed && splitTimer.TimeElapsedMillis () >= 100) //wait until the two splits have separated
			{
				timerElapsed = true;
			}
			
			if(alignCode != 0){
				alignmentProgress = alignTimer.TimeElapsedSecs()/ALIGNMENT_TIME;
				if(alignmentProgress < 1){
					float displacement = Mathf.Sin((Mathf.PI / 2)*alignmentProgress) * alignmentSpacing*alignCode; //curved motion away from middle
					splitDistance = initialAlignRadius + displacement;
				}else{
					alignCode = 0;//this might be an issue if we need to know whether it's an inner/middle/outer bullet later
					//but for now it should save on divides
				}
			}
				
			splitTravelDist += splitSpeed; //increase angle relative to start
			float currentSplitAngle = splitStartingAngle + splitTravelDist; //calculate a fixed angle
			float radians = currentSplitAngle * Mathf.Deg2Rad; //convert to radians
			//change the bullet's rotation to point in the direction it's moving
			transform.eulerAngles = new Vector3(transform.eulerAngles.x,transform.eulerAngles.y,currentSplitAngle + 90f*splitDirection);
			//put the bullet at the location corresponding to its current angle
			rt.anchoredPosition = new Vector2(Mathf.Cos(radians)*splitDistance,Mathf.Sin (radians)*splitDistance);
			
			if(Mathf.Abs(splitTravelDist) > splitCount*60f){ //if you've gone past your max range
				piercesLeft = 0;
				Collide ();
			}
		}
		
	}
	Timer arcFallTimer;
	bool arcFalling = false;
	float arcFallDuration = .3f; //long enough for at least one frame to pass and do collision checking with enemies
	void ArcBulletFallingUpdate(){
		if(arcFallTimer.TimeElapsedSecs() > arcFallDuration){
			piercesLeft = 0;
			Collide ();
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
					if (bc.splitParent == this.splitParent && splitDirection != bc.splitDirection)
					{
						Debug.Log ("the two splits collided!");
						//Determine lane ID and spawn aoe in appropriate lane
						if (timerElapsed) //just to make sure they don't destroy each other on spawn
						{
                            //spawn a FullZoneBlast to destroy all enemies in This zone
                            Debug.Log("bullet-collision splash dmg started");
                            GameObject zoneBlast = Instantiate(Resources.Load("Prefabs/MainCanvas/FullZoneBlast")) as GameObject;
                            int currentLaneID = GetCurrentLaneID();
                            FullZoneBlast fzb = zoneBlast.GetComponent<FullZoneBlast>();
                            fzb.SetZoneID(currentLaneID);
                            fzb.SetDamage(dmg * 2); //dmg value for now, idk

                            //destroy selves
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
        else if (coll.gameObject.tag == "Shield")
        {
            Shield sc = coll.gameObject.GetComponent<Shield>();
            float sSpeedBoost = sc.speedBoost;
            float sRangeBoost = sc.rangeBoost;
            float sSpread = sc.spread;
            float sSpreadRadius = sc.spreadRadius;

            //modify This bullet's values based on shield boosts
            //modify speed
            float ownangle = this.transform.eulerAngles.z;
            float angle = (ownangle + 90) % 360;
            angle *= (float)Math.PI / 180;
            //do we want this line?.. vvv
            //angle = (angle - (float)Math.PI / 6f) + ((((float)Math.PI / 3f) / (2))); //handles spread effect
            vx = (speed + sSpeedBoost) * (float)Math.Cos(angle);
            vy = (speed + sSpeedBoost) * (float)Math.Sin(angle);
            range += sRangeBoost;
            if (range > 1f)
            {
                range = 1f;
            }
            //spread does the lightning arc thing -- do this later
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
				if (ec.GetCurrentTrackID() == GetCurrentTrackID()) //if this enemy is in this bullet's zone
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
		//Debug.Log(degrees);
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
		if(pierces){
			//Debug.Log ("pierce bullet called collide with " + piercesLeft + " pierces left");
			if(piercesLeft > 0){
				piercesLeft--;
				//Debug.Log ("pierces left: " + (piercesLeft));
				return;
			}
		}
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
					GameObject splashCircle = Instantiate (Resources.Load ("Prefabs/MainCanvas/SplashCircle")) as GameObject;
					splashCircle.transform.SetParent(Dial.spawnLayer,false);
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
					AoE ac = splashCone.GetComponent<AoE>();
					ac.scale = splashRad;
					ac.parent = "Bullet";
					ac.aoeBulletCon = this;
					ac.ScaleProps(splash);
				}
			}
		}
		//Debug.Log ("splitCount is " + splitCount);
		if (splitCount > 0)
		{
			if (!isSplitBullet) //only pre-split bullets
			{
				RectTransform rt = GetComponent<RectTransform>();
				//Debug.Log ("spawned 2 split bullets");
				ScaleProps(0.5f);
				
				GameObject split1 = Instantiate (Resources.Load ("Prefabs/MainCanvas/Bullet")) as GameObject;
				split1.transform.SetParent(Dial.spawnLayer,false);
				Bullet splitbc = split1.GetComponent<Bullet>();
				splitbc.splitDirection = -1;
				splitbc.splitParent = this;
				//splitbc.arcDmg = 5f;
				split1.transform.position = this.transform.position;
				splitbc.SetSplitPropsNew(this);
				
				GameObject split2 = Instantiate (Resources.Load ("Prefabs/MainCanvas/Bullet")) as GameObject;
				split2.transform.SetParent(Dial.spawnLayer,false);
				Bullet splitbc2 = split2.GetComponent<Bullet>();
				splitbc2.splitDirection = 1;
				splitbc2.splitParent = this;
				split2.transform.position = this.transform.position;
				splitbc2.SetSplitPropsNew(this);
				if(multiSplits){
					float currentRadius = Mathf.Sqrt(rt.anchoredPosition.x*rt.anchoredPosition.x + rt.anchoredPosition.y*rt.anchoredPosition.y);
					
					GameObject splitLO = Instantiate (Resources.Load ("Prefabs/MainCanvas/Bullet")) as GameObject;
					GameObject splitRO = Instantiate (Resources.Load ("Prefabs/MainCanvas/Bullet")) as GameObject;
					splitLO.transform.SetParent(Dial.spawnLayer,false);
					splitRO.transform.SetParent(Dial.spawnLayer,false);
					Bullet bLO = splitLO.GetComponent<Bullet>();
					bLO.splitDirection = 1;
					bLO.splitParent = this;
					bLO.transform.position = this.transform.position;
					bLO.SetSplitPropsNew(this);
					bLO.alignTimer = new Timer();
					bLO.alignCode = 1;
					bLO.initialAlignRadius = currentRadius;
					
					Bullet bRO = splitRO.GetComponent<Bullet>();
					bRO.splitDirection = -1;
					bRO.splitParent = this;
					bRO.transform.position = this.transform.position;
					bRO.SetSplitPropsNew(this);
					bRO.alignTimer = new Timer();
					bRO.alignCode = 1;
					bRO.initialAlignRadius = currentRadius;
					
					if(currentRadius - alignmentSpacing > Dial.inner_radius){ //don't spawn bullets that would hit the dial
						GameObject splitLI = Instantiate (Resources.Load ("Prefabs/MainCanvas/Bullet")) as GameObject;
						GameObject splitRI = Instantiate (Resources.Load ("Prefabs/MainCanvas/Bullet")) as GameObject;
						splitLI.transform.SetParent(Dial.spawnLayer,false);
						splitRI.transform.SetParent(Dial.spawnLayer,false);
						
						Bullet bLI = splitLI.GetComponent<Bullet>();
						bLI.splitDirection = 1;
						bLI.splitParent = this;
						bLI.transform.position = this.transform.position;
						bLI.SetSplitPropsNew(this);
						bLI.alignTimer = new Timer();
						bLI.alignCode = -1;
						bLI.initialAlignRadius = currentRadius;
						
						Bullet bRI = splitRI.GetComponent<Bullet>();
						bRI.splitDirection = -1;
						bRI.splitParent = this;
						bRI.transform.position = this.transform.position;
						bRI.SetSplitPropsNew(this);
						bRI.alignTimer = new Timer();
						bRI.alignCode = -1;
						bRI.initialAlignRadius = currentRadius;
						
						if(spreadCode == -1){ //all these spreadcode checks prevent extra bullets from spawning
							Destroy (splitLI);
						}else if(spreadCode == 1){
							Destroy (splitRI);
						}
					}
					if(spreadCode == -1){
						Destroy (splitLO);
					}else if(spreadCode == 1){
						Destroy (splitRO);
					}
				}
				if(spreadCode == -1){
					Destroy (split2);
				}else if(spreadCode == 1){
					Destroy (split1);
				}
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

    int GetCurrentLaneID()
    {
        float angle = transform.eulerAngles.z;
        if (angle > -2.0 && angle < 2.0)
            return 1;
        else if (angle > 58.0 && angle < 62.0)
            return 6;
        else if (angle > 118.0 && angle < 122.0)
            return 5;
        else if (angle > 178.0 && angle < 182.0)
            return 4;
        else if (angle > 238.0 && angle < 242.0)
            return 3;
        else if (angle > 298.0 && angle < 302.0)
            return 2;
        else {
            Debug.Log("somehow a gun has a very very wrong angle");
            return -1;
        }
    }
}
