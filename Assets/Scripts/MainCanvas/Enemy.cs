using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using MiniJSON;

public class Enemy : MonoBehaviour,EventHandler {
	
	public readonly float DIAL_RADIUS = 52.1f; //hard coded to avoid constantly querying dial
	//if dial size ever needs to change, replace references to this with calls to a getter
	public static readonly float NORMALNESS_RANGE = 2.0f;//constant for determining if an enemy is "slow" or "fast" - ***balance later
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
	
	//float ySpeed;
	//float xSpeed;
	
	protected Timer timer = new Timer();
	protected EnemyMover mover;
	public bool moving = false;
	protected float progress = 0.0f;
	protected float moverLaneOverride = 0f;
	
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
	
	protected EnemyShield shield = null;
	protected bool beingShieldDrainedByBulk = false;
	protected bool dead = false; //WE NEED THIS APPARENTLY???
	
	//ability?
	//weakness?
	
	// Use this for initialization
	public virtual void Start () {
		rt = (RectTransform)transform;
		dialCon = GameObject.Find("Dial").GetComponent<Dial>();
		EventManager.Instance ().RegisterForEventType ("shot_collided", this);
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
	public void StartMoving(){
		ConfigureEnemy (); 
		
		//some scaling- could maybe be done through transform.scale, but I don't trust Unity to handle the collider
		ScaleEnemy();
		
		timer.Restart ();
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
		string moveString = (string)data["movementType"];
		if(moveString.Equals("Linear")){
			mover = new LinearMover(this);
		}else if(moveString.Equals("Linear_Right")){
			mover = new LinearMover(this);
			mover.PutInRightLane();
		}else if(moveString.Equals("Linear_Left")){
			mover = new LinearMover(this);
			mover.PutInLeftLane();
		}else if(moveString.Equals("Slowing_Linear")){
			mover = new SlowingLinearMover(this);
		}else if(moveString.Equals("Slowing_Linear_Right")){
			mover = new SlowingLinearMover(this);
			mover.PutInRightLane();
		}else if(moveString.Equals("Slowing_Linear_Left")){
			mover = new SlowingLinearMover(this);
			mover.PutInLeftLane();
		}else if(moveString.Equals ("Zigzag")){
			mover = new ZigzagMover(this);
		}else if(moveString.Equals ("Zigzag_Mirror")){
			ZigzagMover zm = new ZigzagMover(this);
			zm.Mirror();
			mover = zm;
		}else if(moveString.Equals ("Strafing")){
			mover = new StrafingMover(this);
		}else if(moveString.Equals ("Strafing_Mirror")){
			StrafingMover sm = new StrafingMover(this);
			sm.Mirror();
			mover = sm;
		}else if(moveString.Equals ("Sine")){
			mover = new SineMover(this);
		}else if(moveString.Equals ("Sine_Mirror")){
			SineMover sm = new SineMover(this);
			sm.Mirror();
			mover = sm;
		}else if(moveString.Equals ("Swerve_Left")){
			mover = new SwerveMover(this);
		}else if(moveString.Equals ("Swerve_Right")){
			SwerveMover sm = new SwerveMover(this);
			sm.Mirror();
			mover = sm;
		}else if(moveString.Equals ("Swerve_In_Left")){
			SwerveMover sm = new SwerveMover(this);
			sm.PutInRightLane();
			mover = sm;
		}else if(moveString.Equals ("Swerve_In_Right")){
			SwerveMover sm = new SwerveMover(this);
			sm.PutInLeftLane();
			sm.Mirror();
			mover = sm;
		}else if(moveString.Equals ("Semicircle")){
			mover = new SemicircleMover(this);
		}else if(moveString.Equals ("Semicircle_Mirror")){
			SemicircleMover sm = new SemicircleMover(this);
			sm.Mirror();
			mover = sm;
		}else if(moveString.Equals ("Blink")){
			mover = new BlinkMover(this);
		}else if(moveString.Equals ("Blink_Mirror")){
			BlinkMover bm = new BlinkMover(this);
			bm.Mirror();
			mover = bm;
		}else if(moveString.Equals ("Wink")){
			mover = new WinkMover(this);
		}else if(moveString.Equals ("Wink_Mirror")){
			WinkMover wm = new WinkMover(this);
			wm.Mirror();
			mover = wm;
		}else if(moveString.Equals ("Sidestep")){
			mover = new SidestepMover(this);
		}else if(moveString.Equals ("Sidestep_Mirror")){
			SidestepMover sm = new SidestepMover(this);
			sm.Mirror();
			mover = sm;
		}
		//do any lane overriding
		mover.RightOffset(moverLaneOverride);
	}
	
	
	// Update is called once per frame
	public virtual void Update () {
		//handle whether or not to update, pause stuff
		moving = !Pause.paused;
		if (lastPause != moving) {
			timer.Restart();
		}
		if(frozen)
			return;
		//Debug.Log (moving + " " + lastPause);
		lastPause = moving;
		if (!moving)
			return;
			
		CheckShieldCollisions();
		if(dead)
			return;
		
		//handle poison
		if(poisoned){
			Debug.Log("i'm poisoned");
			if(poisonTimer.TimeElapsedSecs() >= poisonDuration){
				poisoned = false;
			}else if(poisonTickTimer.TimeElapsedSecs() > 0.5f){
				float poisonDamage = poisonPerTick*maxhp;
				hp -= poisonDamage;
				poisonTickTimer.Restart();
			}else{
				//do nothing
			}
		}
		
		if (hp <= 0.0f)
		{
			Die ();
		}
		//make progress
		float secsPassed = timer.TimeElapsedSecs ();
		timer.Restart ();
		float progressIncrement = secsPassed / impactTime;
		//calculate progress increment and deal with speed effects
		if(knockbackInProgress){
			//Debug.Log ("knocking back");
			if(knockbackTimer.TimeElapsedSecs() >= KNOCK_DURATION){//we're done knocking back, time to stop
				knockbackInProgress = false;
				if(stunWaiting){
					stunInProgress = true;
					stunWaiting = false;
					stunTimer.Restart();
				}else if(slowWaiting){
					slowInProgress = true;
					slowWaiting = false;
					slowTimer.Restart();
				}
			}else{
				progressIncrement = -knockbackPower*KNOCK_CONSTANT;
			}
		}else if(stunInProgress){
			if(stunTimer.TimeElapsedSecs() >= stunDuration){//done being stunned
				stunInProgress = false;
				if(slowWaiting){
					slowInProgress = true;
					slowWaiting = false;
					slowTimer.Restart();
				}
			}else{
				progressIncrement = 0;
			}	
		}else if(slowInProgress){
			if(slowTimer.TimeElapsedSecs() >= slowDuration){
				slowInProgress = false;
			}else{
				progressIncrement *= slowedSpeed;
			}
		}
		//increment is calculated, so apply
		progress += progressIncrement;
		if(progress < 0)
			progress = 0f;
		
		DoTheMoving();
		
		GameObject healthCircle = transform.FindChild("Health").gameObject;
		healthCircle.transform.localScale = new Vector3 (hp / maxhp, hp / maxhp, 1);
	}
	public void DoTheMoving(){
		Vector2 point = mover.PositionFromProgress(progress);
		rt.anchoredPosition = new Vector2(point.x,point.y);
		float distanceFromCenter = Mathf.Sqrt ((rt.anchoredPosition.x) * (rt.anchoredPosition.x) + (rt.anchoredPosition.y) * (rt.anchoredPosition.y));
		if ( distanceFromCenter < DIAL_RADIUS ) {
			GameEvent ge = new GameEvent("enemy_arrived");
			ge.addArgument(transform.gameObject);
			EventManager.Instance().RaiseEvent(ge);
		}
	}
	public void TakeDamage(float damage){
		if(damage >= hp){
			hp = 0;
			Die ();
		}else{
			hp -= damage;
		}
	}
	public void HandleEvent(GameEvent ge){
		//unpack shot location argument, check for collision, if it collided with you take damage from it
		//actually never mind that, Unity has its own collision detection system!
	}
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
			return;
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
		Debug.Log ("!!! we made it through to our own collision");
		if (coll.gameObject.tag == "Bullet") //if it's a bullet
		{
			Bullet bc = coll.gameObject.GetComponent<Bullet> ();
			if (bc != null) {
				if (bc.CheckActive()) //if we get a Yes, this bullet/trap/shield is active
				{
					if (bc.isSplitBullet && bc.timerElapsed || !bc.isSplitBullet)
					{
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
						if (bc.timerElapsed)
						{
							bc.Collide ();
						}
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
	public void SetProgress(float f){
		progress = f;
	}
	public float GetProgress(){
		return progress;
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
	
	bool poisoned = false;
	float poisonPerTick = 0f;
	float poisonDuration = 0f;
	bool knockbackInProgress = false;
	float knockbackPower = 0f;
	bool stunWaiting = false;
	float stunDuration = 0f;
	bool stunInProgress = false;
	bool slowWaiting = false;
	float slowDuration = 0f;
	float slowedSpeed = 1f;
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
			poisonTimer.Restart();
			poisoned = true;
		}else{
			Debug.Log ("poison was " + bc.poison);
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
			}
			slowDuration = bc.slowDur;
			slowedSpeed = bc.slowdown;
		}
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
	}
	
	public void Freeze(){
		frozen = true;
	}
	public void Unfreeze(){
		frozen = false;
		timer.Restart();
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
}
