﻿using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using MiniJSON;

public class EnemyController : MonoBehaviour,EventHandler {

	public readonly float DIAL_RADIUS = 1.5f; //hard coded to avoid constantly querying dial
	//if dial size ever needs to change, replace references to this with calls to a getter
	public static readonly float NORMALNESS_RANGE = 2.0f;//constant for determining if an enemy is "slow" or "fast" - ***balance later
	public static readonly float KNOCK_CONSTANT = 0.03f;// constant for knockback per update - ***balance this at some point!
	public static readonly float KNOCK_DURATION = 0.75f;// constant for knockback time


	public DialController dialCon;

	protected long spawntime = 0;
	protected bool warnedFor = false;
	public int trackID = 0;
	protected int trackLane = 0;

	protected float maxhp = 100.0f;
	protected float hp = 100.0f;
	protected string srcFileName;

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

	//ability?
	//weakness?

	// Use this for initialization
	public virtual void Start () {
		/*dialCon = GameObject.FindWithTag ("Dial").GetComponent<DialController>();
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
		*/
	}
	public void StartMoving(){
		/*ConfigureEnemy (); 

		//some scaling- could maybe be done through transform.scale, but I don't trust Unity to handle the collider
		SpriteRenderer sr = transform.gameObject.GetComponent<SpriteRenderer> ();
		float scalefactor = (radius * 2) / sr.bounds.size.x;
		transform.localScale = new Vector3 (scalefactor, scalefactor, 1);
		//CircleCollider2D collider = transform.gameObject.GetComponent<CircleCollider2D> ();
		//collider.radius = scalefactor/2;

		timer.Restart ();
		moving = true;

		//float angle = Mathf.Atan2(transform.position.y , transform.position.x);
		//ySpeed = Mathf.Sin (angle) * speed;
		//xSpeed = Mathf.Cos (angle) * speed;*/
	}
	public void OverrideMoverLane(float f){
		moverLaneOverride = f;
	}
	public void ConfigureEnemy(){
		/*FileLoader fl = new FileLoader ("JSONData" + Path.DirectorySeparatorChar + "Bestiary",srcFileName);
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

		rarityUpWithHits = (bool)data ["rarityUpWithHits"];
		rareDropThreshold = (int)(long)data ["rareDropThreshold"];
		rareChance = (float)(double)data ["rareChance"];
		normalChance = (float)(double)data ["normalChance"];
		
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
		*/
	}
	
	// Update is called once per frame
	public virtual void Update () {
		/*//handle whether or not to update, pause stuff
		moving = !GamePause.paused;
		if (lastPause != moving) {
			timer.Restart();
		}
			//Debug.Log (moving + " " + lastPause);
		lastPause = moving;
		if (!moving)
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

		//transform.position = new Vector3 (transform.position.x - xSpeed, transform.position.y - ySpeed, transform.position.z);
		Vector2 point = mover.PositionFromProgress(progress);
		transform.position = new Vector3 (point.x, point.y, transform.position.z);
		float distanceFromCenter = Mathf.Sqrt ((transform.position.x) * (transform.position.x) + (transform.position.y) * (transform.position.y));
		if ( distanceFromCenter < DIAL_RADIUS ) {
			GameEvent ge = new GameEvent("enemy_arrived");
			ge.addArgument(transform.gameObject);
			EventManager.Instance().RaiseEvent(ge);
		}
		GameObject healthCircle = transform.Find ("Health").gameObject;
		healthCircle.transform.localScale = new Vector3 (hp / maxhp, hp / maxhp, 1);
		*/
	}

	public void HandleEvent(GameEvent ge){
		//unpack shot location argument, check for collision, if it collided with you take damage from it
		//actually never mind that, Unity has its own collision detection system!
	}
	public virtual void OnTriggerEnter2D(Collider2D coll){ //this is said system.
		/*//Debug.Log ("a collision happened!");
		if (coll.gameObject.tag == "Bullet") //if it's a bullet
		{
			BulletController bc = coll.gameObject.GetComponent<BulletController> ();
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
						Die ();
					}
				}
			}
		}
		else if (coll.gameObject.tag == "Trap") //if it's a trap
		{
			TrapController tc = coll.gameObject.GetComponent<TrapController> ();
			if (tc != null) {
				if (tc.CheckActive()) //if we get a Yes, this bullet/trap/shield is active
				{
					tc.enemyHit = this.gameObject;
					//StartCoroutine (StatusEffectsTrap (tc));
					hp -= tc.dmg;
					tc.Collide();
					if(hp <= 0){
						Die ();
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
			AoEController ac = obj.GetComponent<AoEController>();
			if (ac.parent == "Bullet")
			{
				if (ac.aoeBulletCon.enemyHit != this.gameObject) //if this isn't the enemy originally hit
				{
					//Debug.Log ("parent is bullet@");
					BulletController bc = ac.aoeBulletCon;
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
					TrapController tc = ac.aoeTrapCon;
					//StartCoroutine (StatusEffectsTrap (tc));
					hp -= tc.dmg;
					if(hp <= 0){
						Die ();
					}
				}
			}

		}
		//other types of collision?
		*/
	}

	public virtual void Die(){
		/*
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
				float distanceFromCenter = Mathf.Sqrt ((transform.position.x) * (transform.position.x) + (transform.position.y) * (transform.position.y));
				if (distanceFromCenter > DialController.middle_radius) { //died in outer ring
					if (rng < highDropRate) {
						DropPiece ();
					}
				} else if (distanceFromCenter > DialController.inner_radius) { //died in middle ring
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
				float distanceFromCenter = Mathf.Sqrt ((transform.position.x) * (transform.position.x) + (transform.position.y) * (transform.position.y));
				if (distanceFromCenter > DialController.middle_radius) { //died in outer ring
					if (rng < lowDropRate) {
						DropPiece ();
					}
				} else if (distanceFromCenter > DialController.inner_radius) { //died in middle ring
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
		*/
	}
	public void DropPiece(){
		/*
		GameObject piece = Instantiate (Resources.Load ("Prefabs/DroppedPiece")) as GameObject;
		Vector3 position = new Vector3 (transform.position.x, transform.position.y, transform.position.z);
		piece.transform.position = position;
		DropController dc = piece.GetComponent<DropController> ();
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

		//piece.GetComponent<DropController> ().MakeRare ();

		GameEvent ge = new GameEvent ("piece_dropped"); //in case other systems need to know about drop events
		//add relevant arguments
		ge.addArgument (position);
		*/
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
	public void SetHP(float f){
		hp = f;
	}
	public void SetMaxHP(float f){
		maxhp = f;
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
	bool slowInProgress = false;
	public void GetStatused(BulletController bc){
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

	/*Coroutines for Status Effects*/
	//which i'm commenting out because i just tried to overhaul status
	/*

	//handles several of the status effect coroutines from Bullets
	//sorry for all the extra sync checks ._.
	IEnumerator StatusEffectsBullet(BulletController bc)
	{
		float bulletDamage = bc.dmg; //damage the bullet dealt -- might be used for lifedrain (percentage of dmg)
		float lifeDrain = bc.lifeDrain; //lifedrain on enemy
		float poison = bc.poison; //poison damage on enemy
		float poisonDur = bc.poisonDur; //how long poison lasts
		float knockback = bc.knockback; //knockback -- positive value for distance knocked back
		float stun = bc.stun; //time of enemy stun
		float slowdown = bc.slowdown; //enemy slowdown -- percentage of normal speed
		float slowDur = bc.slowDur; //how long slowdown lasts
		Debug.Log ("slowDur is " + slowDur);

		//Life Drain - immediate
		if (lifeDrain != 0)
		{
			StartCoroutine (LifeDrain (lifeDrain, bulletDamage));
		}

		//Poison - begins immediately, continues This coroutine w/o waiting to end
		if (poison != 0)
		{
			StartCoroutine (PoisonEffect (poisonDur, poison));
		}

		//Knockback - priority 0
		if (knockback != 0)
		{
			yield return _sync();
			float oldMod;
			yield return _sync();
			if (progressModifier == 0.0f)
			{
				yield return _sync();
				oldMod = progressModAlt;
			}
			else
			{
				yield return _sync();
				oldMod = progressModifier;
			}
			yield return _sync();
			progressModifier = -knockback;
			yield return _sync();
			yield return StartCoroutine(WaitSecCustom(KNOCK_CONSTANT));
			yield return _sync();
			progressModifier = oldMod;
			yield return _sync();
		}

		//Stun - priority 1
		if (stun != 0)
		{
			yield return _sync();
			//Debug.Log ("sync 1 done");
			float oldMod;
			if (progressModifier == 0.0f)
			{
				yield return _sync();
				//Debug.Log ("sync 2 done");
				oldMod = progressModAlt;
			}
			else
			{
				yield return _sync();
				//Debug.Log ("sync 3 done");
				oldMod = progressModifier;
			}
			yield return _sync();
			//Debug.Log ("sync 4 done");
			progressModifier = 0.0f;
			yield return _sync();
			//Debug.Log ("sync 5 done");
			yield return StartCoroutine(WaitSecCustom(stun - KNOCK_CONSTANT));
			yield return _sync();
			//Debug.Log ("sync 6 done");
			progressModifier = oldMod;
			yield return _sync();
			//Debug.Log ("sync 7 done");
		}

		//Slowdown - priority 2
		if (slowdown != 0)
		{
			yield return _sync();
			if (!isSlow)
			{
				yield return _sync();
				isSlow = true;
				//Debug.Log ("entered slowdown if!");
				//Debug.Log ("progressmod old: " + progressModifier);
				float oldMod;
				yield return _sync();
				if (progressModifier == 0.0f)
				{
					yield return _sync();
					oldMod = progressModAlt;
				}
				else
				{
					yield return _sync();
					oldMod = progressModifier;
				}
				yield return _sync();
				progressModifier *= slowdown;
				yield return _sync();
				//Debug.Log ("progressmod new: " + progressModifier);
				yield return StartCoroutine(WaitSecCustom(slowDur - stun - KNOCK_CONSTANT));
				yield return _sync();
				//Debug.Log ("final slowdonw time was: " + ((slowDur - stun - KNOCK_CONSTANT)));
				progressModifier = oldMod;
				yield return _sync();
				isSlow = false;
				yield return _sync();
			}

		}

		yield break;
	}

	//handles several of the status effect coroutines from Traps
	IEnumerator StatusEffectsTrap(TrapController tc)
	{
		float trapDamage = tc.dmg; //damage the trap dealt -- might be used for lifedrain (percentage of dmg)
		float lifeDrain = tc.lifeDrain; //lifedrain on enemy -- percentage of damage given back to player
		float poison = tc.poison; //poison damage on enemy
		float poisonDur = tc.poisonDur; //how long poison lasts
		float knockback = tc.knockback; //knockback
		float stun = tc.stun; //time of enemy stun
		float slowdown = tc.slowdown; //enemy slowdown
		float slowDur = tc.slowDur; //how long slowdown lasts
		float totalTime = stun + slowDur;
		Debug.Log ("slowdown:" + slowdown);

		//Life Drain - immediate
		if (lifeDrain != 0)
		{
			StartCoroutine (LifeDrain (lifeDrain, trapDamage));
		}
		
		//Poison - begins immediately, continues This coroutine w/o waiting to end
		if (poison != 0)
		{
			StartCoroutine (PoisonEffect (poisonDur, poison));
		}
		
		//Knockback - priority 0
		if (knockback != 0)
		{
			yield return _sync();
			float oldMod;
			yield return _sync();
			if (progressModifier == 0.0f)
			{
				yield return _sync();
				oldMod = progressModAlt;
			}
			else
			{
				yield return _sync();
				oldMod = progressModifier;
			}
			progressModifier = -knockback;
			yield return _sync();
			yield return StartCoroutine(WaitSecCustom(KNOCK_CONSTANT));
			yield return _sync();
			progressModifier = oldMod;
		}
		
		//Stun - priority 1
		if (stun != 0)
		{
			yield return _sync();
			float oldMod;
			yield return _sync();
			if (progressModifier == 0.0f)
			{
				yield return _sync();
				oldMod = progressModAlt;
			}
			else
			{
				yield return _sync();
				oldMod = progressModifier;
			}
			yield return _sync();
			progressModifier = 0.0f;
			yield return _sync();
			yield return StartCoroutine(WaitSecCustom(stun - KNOCK_CONSTANT));
			yield return _sync();
			progressModifier = oldMod;
		}
		
		//Slowdown - priority 2
		if (slowdown != 0)
		{
			yield return _sync();
			if (!isSlow)
			{
				yield return _sync();
				isSlow = true;
				yield return _sync();
				float oldMod;
				yield return _sync();
				if (progressModifier == 0.0f)
				{
					yield return _sync();
					oldMod = progressModAlt;
				}
				else
				{
					yield return _sync();
					oldMod = progressModifier;
				}
				progressModifier *= slowdown;
				yield return _sync();
				yield return StartCoroutine(WaitSecCustom(slowDur - stun - KNOCK_CONSTANT));
				yield return _sync();
				progressModifier = oldMod;
				yield return _sync();
				isSlow = false;
			}

		}

		yield break;
	}

	//lifeDrain status effect
	IEnumerator LifeDrain (float amt, float dmg)
	{
		yield return _sync();
		//Debug.Log ("lifedrain started");
		hp -= amt;
		yield return _sync();
		dialCon.health += (float)(int)(amt * dmg);
		yield return _sync();
		if (dialCon.health > dialCon.maxHealth)
		{
			yield return _sync();
			dialCon.health = dialCon.maxHealth;
		}
			
		//Debug.Log ("dial health + " + (float)(int)(amt * dmg));
		//Debug.Log ("new health: " + dialCon.health);
		yield break;
	}

	//poison status effect
	IEnumerator PoisonEffect (float duration, float amt)
	{
		yield return _sync();
		//Debug.Log ("poison started");
		float dur = duration;
		while (dur > 0)
		{
			yield return _sync();
			hp -= maxhp * amt;
			yield return _sync();
			dur -= 0.5f;
			yield return _sync();
			yield return StartCoroutine(WaitSecCustom(0.5f));
			yield return _sync();
		}
		yield break;
	}

	//modifies enemy speed for a given duration -- use for stun, and slowdown
	IEnumerator ChangeSpeed(float duration, float value)
	{
		yield return _sync();
		progressModifier = value;
		yield return new WaitForSeconds(duration);
		progressModifier = 1.0f;
	}
	public void Freeze(){
		timer.PauseTrigger();
		moving = !moving;

	}

	public Coroutine _sync(){
		return StartCoroutine(PauseRoutine()); 
	}
	
	public IEnumerator PauseRoutine(){
		while (GamePause.paused) {
			yield return new WaitForFixedUpdate();
			// This code ^^^ doesn't work.  I want it to just sit
			// and wait until GamePause.paused == true, but
			// I don't know how to do that :P
			//  
			//Debug.Log ("waiting for moving to be true!");
		}
		//Debug.Log ("moving is now true");
		yield return new WaitForEndOfFrame();  
	}

	public IEnumerator WaitSecCustom(float seconds)
	{
		yield return _sync();
		float timer = Time.time + seconds;
		Debug.Log ("timer starts at " + timer);
		yield return _sync();
		while (!moving)
		{
			timer += Time.deltaTime;
			Debug.Log ("timer = " + timer);
		}
		while (Time.time < timer)
		{
			yield return _sync();
			yield return null;
		}
		yield return _sync();
	}*/



}
