using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using MiniJSON;

public class EnemyController : MonoBehaviour,EventHandler {

	public readonly float DIAL_RADIUS = 1.5f; //hard coded to avoid constantly querying dial
	//if dial size ever needs to change, replace references to this with calls to a getter
	public static readonly float NORMALNESS_RANGE = 2.0f; //constant for determining if an enemy is "slow" or "fast" - ***balance later
	public static readonly float KNOCK_CONSTANT = 0.5f; //constant for knockback time - ***balance this at some point!

	public DialController dialCon;

	long spawntime = 0;
	bool warnedFor = false;
	int trackID = 0;
	int trackLane = 0;

	float maxhp = 100.0f;
	float hp = 100.0f;
	string srcFileName;

	//float ySpeed;
	//float xSpeed;

	Timer timer = new Timer();
	EnemyMover mover;
	bool moving = false;
	float progress = 0.0f;
	float progressModifier = 1.0f;
	bool isSlow = false;

	float timesShot = 0.0f;

	float impactTime; //"speed"
	float impactDamage;
	float radius;
	float maxShields;
	float shields;

	float highDropRate;
	float medDropRate;
	float lowDropRate;
	bool rarityUpWithHits;
	int rareDropThreshold;
	float rareChance;
	float normalChance;

	//ability?
	//weakness?

	// Use this for initialization
	void Start () {
		dialCon = GameObject.FindWithTag ("Dial").GetComponent<DialController>();
		EventManager.Instance ().RegisterForEventType ("shot_collided", this);
		SpriteRenderer sr = transform.gameObject.GetComponent<SpriteRenderer> ();
		float rad = sr.bounds.size.x / 2;
		CircleCollider2D collider = transform.gameObject.GetComponent<CircleCollider2D> ();
		collider.radius = rad;

		highDropRate = 100.0f;
		medDropRate = 33.3f;
		lowDropRate = 0.0f;

		//timer = new Timer ();
		mover = new ZigzagMover (this);
		//Debug.Log ("enemy radius is " + radius);

	}
	public void StartMoving(){
		ConfigureEnemy (); 

		//some scaling- could maybe be done through transform.scale, but I don't trust Unity to handle the collider
		SpriteRenderer sr = transform.gameObject.GetComponent<SpriteRenderer> ();
		float scalefactor = (radius * 2) / sr.bounds.size.x;
		transform.localScale = new Vector3 (scalefactor, scalefactor, 1);
		CircleCollider2D collider = transform.gameObject.GetComponent<CircleCollider2D> ();
		collider.radius = radius;

		timer.Restart ();
		moving = true;

		//float angle = Mathf.Atan2(transform.position.y , transform.position.x);
		//ySpeed = Mathf.Sin (angle) * speed;
		//xSpeed = Mathf.Cos (angle) * speed;
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

		rarityUpWithHits = (bool)data ["rarityUpWithHits"];
		rareDropThreshold = (int)(long)data ["rareDropThreshold"];
		rareChance = (float)(double)data ["rareChance"];
		normalChance = (float)(double)data ["normalChance"];
	}
	
	// Update is called once per frame
	void Update () {
		if (hp <= 0.0f)
		{
			Die ();
		}
		if (!moving)
			return;
		//make progress
		float secsPassed = timer.TimeElapsedSecs ();
		timer.Restart ();
		float progressIncrement = secsPassed / impactTime;
		progressIncrement *= progressModifier;
		Debug.Log ("pMod = " + progressModifier);
		//Debug.Log ("progressIncrement = " + progressIncrement);
		progress += progressIncrement;

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
	}

	public void HandleEvent(GameEvent ge){
		//unpack shot location argument, check for collision, if it collided with you take damage from it
		//actually never mind that, Unity has its own collision detection system!
	}
	void OnTriggerEnter2D(Collider2D coll){ //this is said system.
		//Debug.Log ("a collision happened!");
		if (coll.gameObject.tag == "Bullet") //if it's a bullet
		{
			BulletController bc = coll.gameObject.GetComponent<BulletController> ();
			if (bc != null) {
				if (bc.CheckActive()) //if we get a Yes, this bullet/trap/shield is active
				{
					StartCoroutine (StatusEffectsBullet (bc));
					hp -= bc.dmg;
					bc.Collide();
					timesShot++;
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
					StartCoroutine (StatusEffectsTrap (tc));
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

	}

	public void Die(){
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
	}
	public void DropPiece(){
		GameObject piece = Instantiate (Resources.Load ("Prefabs/DroppedPiece")) as GameObject;
		Vector3 position = new Vector3 (transform.position.x, transform.position.y, transform.position.z);
		piece.transform.position = position;
		DropController dc = piece.GetComponent<DropController> ();

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
	public void SetTrackID(int id){
		trackID = id;
	}
	public int GetTrackID(){
		return trackID;
	}
	public int GetCurrentTrackID(){ //in case it's moved between lanes without having set the track ID on purpose
		float degrees = Mathf.Atan2(transform.position.y,transform.position.x) * Mathf.Rad2Deg;
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

	/*Coroutines for Status Effects*/

	//handles several of the status effect coroutines from Bullets
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
			float oldMod = progressModifier;
			progressModifier = -knockback;
			yield return new WaitForSeconds(KNOCK_CONSTANT);
			progressModifier = oldMod;
		}

		//Stun - priority 1
		if (stun != 0)
		{
			float oldMod = progressModifier;
			progressModifier = 0.0f;
			yield return new WaitForSeconds(stun - KNOCK_CONSTANT);
			progressModifier = oldMod;
		}

		//Slowdown - priority 2
		if (slowdown != 0)
		{
			if (!isSlow)
			{
				isSlow = true;
				//Debug.Log ("entered slowdown if!");
				//Debug.Log ("progressmod old: " + progressModifier);
				float oldMod = progressModifier;
				progressModifier *= slowdown;
				//Debug.Log ("progressmod new: " + progressModifier);
				yield return new WaitForSeconds((slowDur - stun - KNOCK_CONSTANT));
				//Debug.Log ("final slowdonw time was: " + ((slowDur - stun - KNOCK_CONSTANT)));
				progressModifier = oldMod;
				isSlow = false;
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
			float oldMod = progressModifier;
			progressModifier = -knockback;
			yield return new WaitForSeconds(KNOCK_CONSTANT);
			progressModifier = oldMod;
		}
		
		//Stun - priority 1
		if (stun != 0)
		{
			float oldMod = progressModifier;
			progressModifier = 0.0f;
			yield return new WaitForSeconds(stun - KNOCK_CONSTANT);
			progressModifier = oldMod;
		}
		
		//Slowdown - priority 2
		if (slowdown != 0)
		{
			if (!isSlow)
			{
				isSlow = true;
				float oldMod = progressModifier;
				progressModifier *= slowdown;
				yield return new WaitForSeconds(slowDur - stun - KNOCK_CONSTANT);
				progressModifier = oldMod;
				isSlow = false;
			}

		}

		yield break;
	}

	//lifeDrain status effect
	IEnumerator LifeDrain (float amt, float dmg)
	{
		//Debug.Log ("lifedrain started");
		hp -= amt;
		dialCon.health += (float)(int)(amt * dmg);
		if (dialCon.health > dialCon.maxHealth)
			dialCon.health = dialCon.maxHealth;
		//Debug.Log ("dial health + " + (float)(int)(amt * dmg));
		//Debug.Log ("new health: " + dialCon.health);
		yield break;
	}

	//poison status effect
	IEnumerator PoisonEffect (float duration, float amt)
	{
		//Debug.Log ("poison started");
		int dur = (int)duration;
		while (dur > 0)
		{
			hp -= amt;
			dur--;
			yield return new WaitForSeconds(1);
		}
		yield break;
	}

	//modifies enemy speed for a given duration -- use for stun, and slowdown
	IEnumerator ChangeSpeed(float duration, float value)
	{
		progressModifier = value;
		yield return new WaitForSeconds(duration);
		progressModifier = 1.0f;
	}
}
