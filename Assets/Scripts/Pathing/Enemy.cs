using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using MiniJSON;

/*************
* Oh man look how clean this is suddenly!
* Why didn't anyone tell me about #region tags?!
* Now you can collapse and expand the different sections,
*  instead of scrolling up and down constantly!
*************/

public class Enemy : MonoBehaviour,EventHandler {
	
	#region GeneralData (all the data declarations, except for status stuff)
	public readonly float DIAL_RADIUS = 52.1f; //hard coded to avoid constantly querying dial
	//if dial size ever needs to change, replace references to this with calls to a getter
	public static readonly float NORMALNESS_RANGE = 2.0f;//constant for determining if an enemy is "slow" or "fast" - ***balance later
	public static readonly float NORMAL_SPEED = .2f;
	public static readonly float NORMAL_IMPACT_TIME = 25f;
	public static readonly float FRAMES_FROM_ZERO_TO_MAX = 3f;
	public static readonly float KNOCK_CONSTANT = 0.03f;// constant for knockback per update - ***balance this at some point!
	public static readonly float KNOCK_DURATION = 0.75f;// constant for knockback time
	public readonly float ENEMY_SCALE = 35f;
	
	public Dial dialCon;
	
	protected long spawntime = 0;
	protected bool warnedFor = false;
	public int trackID = 0;
	protected int trackLane = 0;
	
	protected float maxhp = 100.0f;
	protected float hp = 100.0f;
	protected string srcFileName;
	
	public bool spawnedByBoss = false;
	
	public bool moving = false;
	protected float moverLaneOverride = 0f;
	protected string moverType = "Linear";
	protected AIPath path;
	
	protected float timesShot = 0.0f;
	
	protected float impactTime; //"speed"
	protected float impactDamage;
	protected float radius;
	protected float maxShields;
	protected float shields;
	protected bool tripsTraps; //some enemies are immune to traps
	protected bool shieldPen; //and some go through player shields
	
	protected float highDropRate;
	protected float medDropRate;
	protected float lowDropRate;
	protected bool rarityUpWithHits;
	protected int rareDropThreshold;
	protected float rareChance;
	protected float normalChance;
	protected bool lastPause;
	
	protected Timer poisonTimer;
	protected Timer poisonTickTimer;
	protected Timer slowTimer;
	protected Timer stunTimer;
	protected Timer knockbackTimer;
	
	protected bool frozen = false;
	
	protected RectTransform rt;
	protected Steering steering;
	
	protected EnemyShield shield = null;
	protected bool beingShieldDrainedByBulk = false;
	protected bool dead = false; //WE NEED THIS APPARENTLY???
	
	protected float progress //being sneaky with C# properties to avoid breaking progress-dependent code in stuff
	{
		get { 	float dist = rt.anchoredPosition.magnitude;
				dist -= Dial.DIAL_RADIUS; //is now distance from dial, not center
				float prog = Dial.TRACK_LENGTH - dist;
				return prog/Dial.TRACK_LENGTH;
			}
		set { return; }//no setting progress directly
	}
	#endregion
	
	#region Setup (things enemies need to do before they get a move on)
	// Use this for initialization
	public virtual void Start () {
		rt = (RectTransform)transform;
		dialCon = GameObject.Find("Dial").GetComponent<Dial>();
		//EventManager.Instance ().RegisterForEventType ("shot_collided", this);
		//SpriteRenderer sr = transform.gameObject.GetComponent<SpriteRenderer> ();
		//float rad = sr.bounds.size.x / 2;
		//CircleCollider2D collider = transform.gameObject.GetComponent<CircleCollider2D> ();
		//collider.radius = rad;
		
		highDropRate = 100.0f;
		medDropRate = 33.3f;
		lowDropRate = 0.0f;
		lastPause = false;
		
		poisonTimer = new Timer();
		poisonTickTimer = new Timer();
		slowTimer = new Timer();
		stunTimer = new Timer();
		knockbackTimer = new Timer();
		//timer = new Timer ();
		//Debug.Log ("enemy radius is " + radius);
		
	}
	public void SetPositionBasedOnAngle(){
		float trackangle = GetStartingTrackAngle();
		trackangle -= trackLane*15f;
		trackangle += moverLaneOverride;
		trackangle += 90f; //correct for math
		trackangle *= Mathf.Deg2Rad;
		Vector2 pos = Dial.ENEMY_SPAWN_LENGTH * new Vector2(Mathf.Cos (trackangle),Mathf.Sin(trackangle));
		if(rt == null)
			rt = GetComponent<RectTransform>();
		rt.anchoredPosition = pos;
	}
	public void SetPositionBasedOnProgress(float fakeprog){
		float trackangle = GetStartingTrackAngle();
		trackangle -= trackLane*15f;
		trackangle += moverLaneOverride;
		trackangle += 90f; //correct for math
		trackangle *= Mathf.Deg2Rad;
		
		float fakeprogressdist = Dial.DIAL_RADIUS + (Dial.TRACK_LENGTH - (fakeprog*Dial.TRACK_LENGTH));
		
		Vector2 pos = fakeprogressdist * new Vector2(Mathf.Cos (trackangle),Mathf.Sin(trackangle));
		if(rt == null)
			rt = GetComponent<RectTransform>();
		rt.anchoredPosition = pos;
	}
	public void StartMoving(){
		//Debug.Log ("start moving called on " + this.ToString());
		ConfigureEnemy (); 
		steering = gameObject.AddComponent<Steering>() as Steering;
		steering.enemy = this;
		float speedMult = 1f/(impactTime/NORMAL_IMPACT_TIME);
		steering.maxSpeed = speedMult * NORMAL_SPEED;
		steering.referenceMaxSpeed = steering.maxSpeed;
		steering.maxAccel = steering.maxSpeed/FRAMES_FROM_ZERO_TO_MAX;
		steering.referenceMaxAccel = steering.maxAccel;
		path = AIPath.CreatePathFromJSONFilename(moverType);
		path.SetDialDimensions(Dial.spawnLayer.anchoredPosition,Dial.FULL_LENGTH);
		path.SetAngle(GetStartingTrackAngle() + (-trackLane*15f) + moverLaneOverride);
		steering.StartFollowingPath(path);
		/*List<Vector2> pathlist = path.GetPathAsListOfVectors();
		foreach(Vector2 node in pathlist){ //path visualization
			GameObject dot = new GameObject();
			Image dimg = dot.AddComponent<Image>() as Image;
			dot.transform.SetParent(Dial.unmaskedLayer,false);
			dot.GetComponent<RectTransform>().sizeDelta = new Vector2(5f,5f);
			dot.GetComponent<RectTransform>().anchoredPosition = node;
		}*/
		//some scaling- could maybe be done through transform.scale, but I don't trust Unity to handle the collider
		ScaleEnemy();
		
		moving = true;
		
		//float angle = Mathf.Atan2(rt.anchoredPosition.y , rt.anchoredPosition.x);
		//ySpeed = Mathf.Sin (angle) * speed;
		//xSpeed = Mathf.Cos (angle) * speed;
	}
	public void ScaleEnemy(){
		Image sr = transform.gameObject.GetComponent<Image> ();
		float scalefactor = (radius * 2) / ((RectTransform)(sr.gameObject.transform)).rect.size.x;
		transform.localScale = new Vector3 (scalefactor*ENEMY_SCALE, scalefactor*ENEMY_SCALE, 1);
	}
	public void OverrideMoverLane(float f){
		moverLaneOverride = f;
	}
	public void ConfigureEnemy(){
		FileLoader fl = new FileLoader ("JSONData" + Path.DirectorySeparatorChar + "Bestiary",srcFileName);
		string json = fl.Read ();
		//Debug.Log (json.Length);
		Dictionary<string,System.Object> data = (Dictionary<string,System.Object>)Json.Deserialize (json);
		maxhp = (float)(double)data ["maxHP"];
		hp = (float)(double)data ["HP"];
		impactDamage = (float)(double)data ["damage"];
		impactTime = (float)(double)data ["impactTime"];
		radius = (float)(double)data ["size"];
		maxShields = (float)(double)data ["maxShields"];
		shields = (float)(double)data ["shields"];
		tripsTraps = (bool)data ["tripsTraps"];
		shieldPen = (bool)data ["shieldPen"];
		
		rarityUpWithHits = (bool)data ["rarityUpWithHits"];
		rareDropThreshold = (int)(long)data ["rareDropThreshold"];
		rareChance = (float)(double)data ["rareChance"];
		normalChance = (float)(double)data ["normalChance"];
		
		//shield stuff
		if(data.ContainsKey("shielded")){
			bool shielded = (bool)data["shielded"];
			if(shielded){ //from here we assume all the shield properties are defined
				float shieldMax = (float)(double)data ["shieldMax"];
				float shieldHP = (float)(double)data ["shieldHP"];
				float shieldRegen = (float)(double)data ["shieldRegen"];
				float shieldSpeed = (float)(double)data["shieldSpeed"];
				List<System.Object> fragments = (List<System.Object>)data["fragments"];
				
				GameObject shieldObj = Instantiate (Resources.Load ("Prefabs/MainCanvas/EnemyShield")) as GameObject;
				shieldObj.transform.SetParent(transform,false);
				shield = shieldObj.GetComponent<EnemyShield>();
				shield.ConfigureShield(shieldMax,shieldHP,shieldRegen,shieldSpeed,fragments);
			}
		}
		
		//movement types
		moverType = (string)data["movementType"];
		
	}
	#endregion
	
	// Update is called once per frame
	public virtual void Update () {
		//handle whether or not to update, pause stuff
		moving = !Pause.paused;
		if(frozen){
			return;
		}
		if (!moving)
			return;
		
		CheckShieldCollisions();
		if(dead)
			return;
		
		//handle poison
		if(poisoned){
			//Debug.Log("i'm poisoned");
			if(poisonTimer.TimeElapsedSecs() >= poisonDuration){
				poisoned = false;
				lethargyPoisoned = false;
				chainPoisoned = false;
				EndChainPoisonRadius();
			}else if(poisonTickTimer.TimeElapsedSecs() > 0.5f){
				float poisonDamage = poisonPerTick*maxhp;
				hp -= poisonDamage;
				poisonTickTimer.Restart();
			}else{
				//do nothing
			}
			if(lethargyPoisoned){
				RefreshSlow();
			}	
		}
		//movement
		
		if(knockbackInProgress){
			//Debug.Log ("knocking back");
			//calculate knock direction
			Vector2 dirToDial = Vector2.zero - rt.anchoredPosition;
			float dotprod = Vector2.Dot(dirToDial,steering.vel);
			if(dotprod >= 0 || dirToDial.magnitude > Dial.FULL_LENGTH){//we're done knocking back, time to stop
				//Debug.Log("knockback over, (" +dotprod+", "+steering.vel.ToString()+")");
				knockbackInProgress = false;
				/*if(knockChained){
					DropEnemies();
				}*/
				knockChained = false;
				steering.clipVelocity = true;
				steering.maxAccel = steering.referenceMaxAccel;
				
				if(stunWaiting){
					stunInProgress = true;
					stunWaiting = false;
					stunTimer.Restart();
					steering.Stun ();
				}else if(slowWaiting){
					slowInProgress = true;
					slowWaiting = false;
					slowTimer.Restart();
					steering.Slow(slowedSpeed);
				}
			}
		}else if(stunInProgress){
			if(stunTimer.TimeElapsedSecs() >= stunDuration){//done being stunned
				stunInProgress = false;
				steering.stunned = false;
				if(slowWaiting){
					slowInProgress = true;
					slowWaiting = false;
					slowTimer.Restart();
					steering.Slow(slowedSpeed);
				}
			}
		}else if(slowInProgress){
			if(slowTimer.TimeElapsedSecs() >= slowDuration){
				slowInProgress = false;
				steering.RevertSpeed();
			}
		}
		
		
		GameObject healthCircle = transform.FindChild("Health").gameObject;
		healthCircle.transform.localScale = new Vector3 (hp / maxhp, hp / maxhp, 1);
		
		if (hp <= 0.0f)
		{
			Die ();
		}
		
		
	}
	
	
	#region AssaultAndBattery (things that happen if the enemy gets hit or dies)
	public void TakeDamage(float damage){
		if(damage >= hp){
			hp = 0;
			Die ();
		}else{
			hp -= damage;
		}
	}
	public void HandleEvent(GameEvent ge){}
	public void CheckShieldCollisions(){
		if(shield == null){
			//do nothing
		}else{
			if(shield.hitThisFrame){//we're fine, shield took it
				//do nothing
			}else if(collidedThisFrame){//shield was not hit last frame, but we were
				secondaryCollisionTicket = true; //get a new ticket to manually call collision
				Debug.Log ("calling on trigger enter");
				OnTriggerEnter2D(heldCollision);
				
			}
			//whatever else happens, we clear the shield's collision
			shield.hitThisFrame = false;	
		}
		collidedThisFrame = false; //and clear our own collision when we're done
		
	}
	public bool collidedThisFrame = false;
	public bool secondaryCollisionTicket = false;
	public Collider2D heldCollision = null;
	public virtual void OnTriggerEnter2D(Collider2D coll){
		if(frozen)
			return; //invincible while boss is moving it
		if(coll.gameObject.tag == "Enemy"){ //handled before anything that cares about shields
			if(knockChained){
				Debug.Log ("we hit something with knockchain");
				float timeEstimate = 1.3f; //how long stuff presumably gets knocked back for
				//it's an estimate since we can't see it directly (without writing a script to measure it)
				float duration = knockbackTimer.TimeElapsedSecs();
				float remainingTime = timeEstimate-duration;
				if(remainingTime > 0){
					float power = (remainingTime/timeEstimate) * knockbackPower;
					coll.GetComponent<Enemy>().SelfKnockback(power);
					coll.GetComponent<Enemy>().SelfStun(stunDuration);
					Debug.Log ("we're calling selfknockback on something");
				}
			}
		}
		if(shield != null){
			if(shield.hitThisFrame)//the shield handled collision for us this time
				return;
			else{//the shield was either missed or hasn't been handled by collision yet
				if(secondaryCollisionTicket){
					//let execution through to actually handle the collision- we're calling this function manually
					secondaryCollisionTicket = false; //punch the ticket
				}else{//skip colliding- wait until update to check if the shield got it for us
					collidedThisFrame = true;
					heldCollision = coll; //store the collision so we can handle it if/when we call this manually
					return;
				}
			}
		}
		if(coll == null){
			Debug.Log ("bullet's gone");
			return;
		}
		//Debug.Log ("!!! we made it through to our own collision");
		if (coll.gameObject.tag == "Bullet") //if it's a bullet
		{
			Debug.Log ("we got hit by a bullet");
			Bullet bc = coll.gameObject.GetComponent<Bullet> ();
			if (bc != null) {
				if (bc.CheckActive()) //if we get a Yes, this bullet/trap/shield is active
				{
					Debug.Log ("bullet was active");
					if (bc.isSplitBullet && bc.timerElapsed || !bc.isSplitBullet)
					{
						Debug.Log ("all the split stuff went through");
						bc.enemyHit = this.gameObject;
						GetStatused(bc);
						//StartCoroutine (StatusEffectsBullet (bc));
						hp -= bc.dmg + bc.arcDmg;
						timesShot++;
					}
					if (!bc.isSplitBullet)
					{
						bc.Collide();
					}
					else if (bc.isSplitBullet)
					{
						//if (bc.timerElapsed)
						//{
						bc.Collide ();
						//}
					}
					
					if(hp <= 0){
						hp = 0;
						Die ();
					}
				}
			}
		}
		else if (coll.gameObject.tag == "Trap") //if it's a trap
		{
			if (tripsTraps)
			{
				Trap tc = coll.gameObject.GetComponent<Trap>();
				if (tc != null)
				{
					if (tc.CheckActive()) //if we get a Yes, this bullet/trap/shield is active
					{
						tc.enemyHit = this.gameObject;
						//StartCoroutine (StatusEffectsTrap (tc));
						hp -= tc.dmg;
						tc.Collide();
						if (hp <= 0)
						{
							Die();
						}
					}
				}
			}
		}
		else if (coll.gameObject.tag == "Shield") //if it's a shield
		{
			//shield actions are handled in DialController
		}
		else if (coll.gameObject.tag == "AoE")
		{
			//Debug.Log ("enemy collided with AoE");
			GameObject obj = coll.gameObject;
			AoE ac = obj.GetComponent<AoE>();
			if (ac.parent == "Bullet")
			{
				if (ac.aoeBulletCon.enemyHit != this.gameObject) //if this isn't the enemy originally hit
				{
					//Debug.Log ("parent is bullet@");
					Bullet bc = ac.aoeBulletCon;
					GetStatused(bc);
					//StartCoroutine (StatusEffectsBullet (bc));
					hp -= bc.dmg;
					Debug.Log ("damage taken: " + bc.dmg);
					//timesShot++;
					if(hp <= 0){
						Die ();
					}
				}
			}
			else if (ac.parent == "Trap")
			{
				if (ac.aoeTrapCon.enemyHit != this.gameObject) //if this isn't the enemy originally hit
				{
					Trap tc = ac.aoeTrapCon;
					//StartCoroutine (StatusEffectsTrap (tc));
					hp -= tc.dmg;
					if(hp <= 0){
						Die ();
					}
				}
			}
			
		}
		//other types of collision?
	}
	public virtual void AddToBonus(List<System.Object> bonusList){
		if(spawnedByBoss)
			return;
		Dictionary<string,System.Object> enemyDict = new Dictionary<string,System.Object>();
		enemyDict.Add("enemyID",srcFileName);
		enemyDict.Add("trackID",(long)GetCurrentTrackID());
		bonusList.Add(enemyDict);
	}
	
	public virtual void Die(){
		dead = true;
		//put more dying functionality here
		System.Random r = new System.Random ();
		float rng = (float)r.NextDouble() * 100; //random float between 0 and 100
		
		
		if (hp <= 0.0f) {
			dialCon.IncreaseSuperPercent();
			if (this.impactTime < TrackController.NORMAL_SPEED + NORMALNESS_RANGE 
			    && this.impactTime > TrackController.NORMAL_SPEED - NORMALNESS_RANGE) { //is "normal speed"
				//Debug.Log ("normal speed enemy died");
				if (rng < medDropRate) {
					DropPiece ();
				}
			} else if (this.impactTime >= TrackController.NORMAL_SPEED + NORMALNESS_RANGE) { //is "slow"
				//Debug.Log("slow enemy died");
				float distanceFromCenter = Mathf.Sqrt ((rt.anchoredPosition.x) * (rt.anchoredPosition.x) + (rt.anchoredPosition.y) * (rt.anchoredPosition.y));
				if (distanceFromCenter > Dial.middle_radius) { //died in outer ring
					if (rng < highDropRate) {
						DropPiece ();
					}
				} else if (distanceFromCenter > Dial.inner_radius) { //died in middle ring
					if (rng < medDropRate) {
						DropPiece ();
					}
				} else { //died in inner ring
					if (rng < lowDropRate) {
						DropPiece ();
					}
				}
			} else { //is "fast"
				//Debug.Log("fast enemy died");
				float distanceFromCenter = Mathf.Sqrt ((rt.anchoredPosition.x) * (rt.anchoredPosition.x) + (rt.anchoredPosition.y) * (rt.anchoredPosition.y));
				if (distanceFromCenter > Dial.middle_radius) { //died in outer ring
					if (rng < lowDropRate) {
						DropPiece ();
					}
				} else if (distanceFromCenter > Dial.inner_radius) { //died in middle ring
					if (rng < medDropRate) {
						DropPiece ();
					}
				} else { //died in inner ring
					if (rng < highDropRate) {
						DropPiece ();
					}
				}
			}
			EnemyIndexManager.LogEnemyDeath(srcFileName);
		}else{
			EnemyIndexManager.LogHitByEnemy(srcFileName);
		}
		Destroy (this.gameObject);
	}
	public bool canDropPiece = true;
	public void DropPiece(){
		if(!canDropPiece){
			return;
		}
		GameObject piece = Instantiate (Resources.Load ("Prefabs/MainCanvas/DroppedPiece")) as GameObject;
		piece.transform.SetParent(Dial.underLayer,false);
		Vector2 position = new Vector2 (rt.anchoredPosition.x, rt.anchoredPosition.y);
		//angle it
		float radians = (-Mathf.PI/2)+Mathf.Atan2(position.y,position.x);
		piece.transform.eulerAngles = new Vector3(piece.transform.eulerAngles.x,piece.transform.eulerAngles.y,radians*Mathf.Rad2Deg);
		
		((RectTransform)piece.transform).anchoredPosition = position;
		Drop dc = piece.GetComponent<Drop> ();
		dc.SetTypes(srcFileName);
		
		System.Random r = new System.Random ();
		float rng = (float)r.NextDouble() * 100; //random float between 0 and 100
		//note: threshold values are exclusive, which means if timesShot is equal to it, it's considered outside the rarity threshold
		bool outsideThreshold = (rarityUpWithHits && timesShot <= rareDropThreshold) 
			|| (!rarityUpWithHits && timesShot >= rareDropThreshold);
		if (outsideThreshold) {
			if(rng < normalChance){
				dc.MakeRare();
			}
		} else {
			if(rng < rareChance){
				dc.MakeRare();
			}
		}
		
		//piece.GetComponent<Drop> ().MakeRare ();
		
		GameEvent ge = new GameEvent ("piece_dropped"); //in case other systems need to know about drop events
		//add relevant arguments
		ge.addArgument (position);
	}
	#endregion
	
	#region GettersAndSetters (accessors for various properties)
	public long GetSpawnTime(){
		return spawntime;
	}
	public void SetSpawnTime(long time){
		spawntime = time;
	}
	public void SetSrcFileName(string filename){
		srcFileName = filename;
	}
	public string GetSrcFileName(){
		return srcFileName;
	}
	public void SetTrackID(int id){
		trackID = id;
	}
	public int GetTrackID(){
		return trackID;
	}
	public int GetCurrentTrackID(){ //in case it's moved between lanes without having set the track ID on purpose
		RectTransform rt_a = (RectTransform)transform;
		float degrees = ((360-Mathf.Atan2(rt_a.anchoredPosition.y,rt_a.anchoredPosition.x) * Mathf.Rad2Deg)+90 + 360)%360;
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
	public float GetStartingTrackAngle(){
		if(trackID == 1){
			return 0f;
		}else if(trackID == 2){
			return 300f;
		}else if(trackID == 3){
			return 240f;
		}else if(trackID == 4){
			return 180f;
		}else if(trackID == 5){
			return 120f;
		}else if(trackID == 6){
			return 60f;
		}else
			return 0f;
	}
	public void SetTrackLane(int lane){
		trackLane = lane;
	}
	public int GetTrackLane(){
		return trackLane;
	}
	public float GetDamage(){
		return impactDamage;
	}
	public bool HasWarned(){
		return warnedFor;
	}
	public void Warn(){
		warnedFor = true;
	}
	public float GetImpactTime(){
		return impactTime;
 	}
 	public float GetProgress(){
 		return progress;
 	}
 	public void SetProgress(float prog){
 		Debug.Log("THIS CHUCKLEFUCK'S TRYING TO SET PROGRESS EVEN THOUGH THAT DOESN'T EXIST");
 		return;
 	}
	public void SetHP(float f){
		hp = f;
	}
	public void SetMaxHP(float f){
		maxhp = f;
	}
	public float GetHP(){
		return hp;
	}	
	public float GetMaxHP(){
		return maxhp;
	}
	#endregion
	
	#region Shields (shield stuff)
	public void GiveShield(float power,float sSpeed, float sRegen, List<System.Object> fragDicts){
		GameObject shieldObj = Instantiate (Resources.Load ("Prefabs/MainCanvas/EnemyShield")) as GameObject;
		shieldObj.transform.SetParent(transform,false);
		shield = shieldObj.GetComponent<EnemyShield>();
		shield.ConfigureShield(power,power,sRegen,sSpeed,fragDicts);
		shield.MakeStuffRealTinyInPreparationForGrowing();
		shield.GrowShields();
	}
	public void NullShield(){
		shield = null;
	}
	public EnemyShield GetShield(){
		return shield;
	}
	#endregion
	
	#region StatusEffects (status effects applied by bullets/traps/aoe)
	bool poisoned = false;
	float poisonPerTick = 0f;
	bool chainPoisoned = false;
	bool lethargyPoisoned = false;
	float poisonDuration = 0f;
	bool knockbackInProgress = false;
	float knockbackPower = 0f;
	bool knockChained = false;
	bool stunWaiting = false;
	float stunDuration = 0f;
	bool stunInProgress = false;
	bool slowWaiting = false;
	float slowDuration = 0f;
	float slowedSpeed = 1f;
	float savedSlowDuration = 0f;
	float savedSlowSpeed = 1f;
	protected bool slowInProgress = false;
	public void GetStatused(Bullet bc){
		//life drain
		if(bc.lifeDrain != 0){
			float healthDrained = bc.lifeDrain * bc.dmg; //adjust for enemy shields somehow later
			dialCon.health += healthDrained;
			if(dialCon.health > dialCon.maxHealth)
				dialCon.health = dialCon.maxHealth;
		}
		//poison
		if(bc.poison != 0){
			Debug.Log ("tried to poison");
			if(poisoned){
				if(poisonPerTick < bc.poison)
					poisonPerTick = bc.poison;
			}else{
				poisonPerTick = bc.poison;
			}
			poisonDuration = bc.poisonDur;
			if(bc.chainsPoison){
				AddChainPoisonRadius(bc.poison,bc.poisonDur);
			}
			poisonTimer.Restart();
			poisoned = true;
		}
		//leeching
		if(bc.leeches){
			if(shield != null){
				shield.leeched = true;
			}
		}
		//knockback
		if(bc.knockback != 0){
			knockbackInProgress = true;
			knockbackPower = bc.knockback;
			knockbackTimer.Restart();
			if(stunInProgress)
				stunWaiting = true;
			if(slowInProgress)
				slowWaiting = true;
			stunInProgress = false;
			slowInProgress = false;
			if(bc.stun != 0){
				knockChained = true;
			}
			steering.Knockback(bc.knockback);
		}
		//stun
		if(bc.stun != 0){
			if(knockbackInProgress){
				stunInProgress = false;
				stunWaiting = true;
			}else{
				stunInProgress = true;
				stunWaiting = false;
				stunTimer.Restart();
				if(slowInProgress)
					slowWaiting = true;
				steering.Stun ();
			}
			stunDuration = bc.stun;
		}
		//slow
		if(bc.slowdown != 0){
			if(knockbackInProgress || stunInProgress){
				slowWaiting = true;
				slowInProgress = false;
			}else{
				slowInProgress = true;
				slowWaiting = false;
				slowTimer.Restart();
				steering.Slow(bc.slowdown);
			}
			slowDuration = bc.slowDur;
			slowedSpeed = bc.slowdown;
			
			if(bc.poison != 0){
				lethargyPoisoned = true;
				savedSlowDuration = slowDuration;
				savedSlowSpeed = slowedSpeed;
			}
		}
	}
	bool chainPoisonSource = false;
	public void AddChainPoisonRadius(float strength,float duration){
		if(chainPoisonSource){
			return;
		}
		float CP_EXTRA_RADIUS = 20f;
		float cpradius = rt.sizeDelta.x/2f + CP_EXTRA_RADIUS/transform.localScale.x;
		GameObject cpcollider = Instantiate (Resources.Load ("Prefabs/MainCanvas/ChainPoisonRadius")) as GameObject;
		ChainPoisonRadius cpr = cpcollider.GetComponent<ChainPoisonRadius>();
		cpr.SetRadius(cpradius);
		cpr.SetStrengthAndDuration(strength,duration);
		cpcollider.transform.SetParent(transform,false);
		chainPoisonSource = true;
	}
	public void EndChainPoisonRadius(){
		if(chainPoisonSource){
			chainPoisonSource = false;
			Destroy(transform.FindChild("ChainPoisonRadius").gameObject);
		}		
	}
	#endregion
	
	#region IndividualStatusEffects (acquire status effects from something other than a bullet/trap/aoe)
	public void GetChainPoisoned(float pstrength, float pduration){
		if(chainPoisoned)
			return;//you already got poisoned this way
		Debug.Log ("tried to chain poison");
		if(poisoned){
			if(poisonPerTick < pstrength)
				poisonPerTick = pstrength;
		}else{
			poisonPerTick = pstrength;
		}
		poisonDuration = pduration;
		chainPoisoned = true;
		poisonTimer.Restart();
		poisoned = true;
	}
	public void RefreshSlow(){
		if(knockbackInProgress || stunInProgress){
			slowWaiting = true;
			slowInProgress = false;
		}else{
			slowInProgress = true;
			slowWaiting = false;
			slowTimer.Restart();
			steering.Slow(savedSlowSpeed);
		}
		slowDuration = savedSlowDuration;
		slowedSpeed = savedSlowSpeed;
	}
	public void SelfKnockback(float f){
		//Debug.Log("selfknock called");
		knockbackInProgress = true;
		knockbackPower = f;
		knockbackTimer.Restart();
		if(stunInProgress)
			stunWaiting = true;
		if(slowInProgress)
			slowWaiting = true;
		stunInProgress = false;
		slowInProgress = false;
		steering.Knockback(f);
	}
	public void SelfStun(float secs){
		if(knockbackInProgress){
			stunInProgress = false;
			stunWaiting = true;
		}else{
			stunInProgress = true;
			stunWaiting = false;
			stunTimer.Restart();
			if(slowInProgress)
				slowWaiting = true;
			steering.Stun ();
		}
		stunDuration = secs;
	}
	#endregion
	
	#region BossShenanigans (stuff bosses do to enemies)
	public void Freeze(){
		frozen = true;
	}
	public void Unfreeze(){
		frozen = false;
	}
	public bool IsFrozen(){
		return frozen;
	}
	public void MakeBulkDrainTarget(){
		beingShieldDrainedByBulk = true;
	}
	public void UndoBulkDrainTarget(){
		beingShieldDrainedByBulk = false;
	}
	public bool IsBeingBulkDrained(){
		return beingShieldDrainedByBulk;
	}
	public void SpeedUp(float mult, float secs){
		steering.SpeedBoost(mult,secs);
	}
	#endregion
}

