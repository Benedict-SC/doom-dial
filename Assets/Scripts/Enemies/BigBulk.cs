using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.IO;
using MiniJSON;

public class BigBulk : Boss{
	
	float[] thingpoints = {0.5236f,1.5708f,2.6180f,3.6652f,4.7124f,5.7596f}; //the middle of each zone, in radians from x-axis
	int thingIdx = 0;
	bool goingRight = true;
	
	Timer thingDoingTimer;
	float shieldDelay = 2f;
	float boostDelay = 1.5f;
	float stealDelay = 2.5f;
	
	Timer eaterTimer;
	float eatDelay = .9f;
	
	float hitboxCorrection = 5f;
	
	Timer directiontimer;
	float secondsToReverse = 6f;
	readonly float MIN_SECS = 6f;
	readonly float MAX_SECS = 25f;
	bool clockwise = false;
	bool mode3reversing = false;
	
	bool reachedMiddle = false;
	
	enum state{SHIELDING,BOOSTING,DIVING,RETREATING,STEALING,MOVING,DECIDING};
	state currentState = state.DECIDING;
	
	GameObject overlay;
	
	Image shieldImage;
	bool shieldActive = true;
	float referenceCapacity = 300f;
	float capacity = 300f;
	float power = 300f;
	float regenSpeed = 0.25f;
	
	float slowedAmount = 0f;
	bool leeched = false;
	Timer brokenTimer;
	float breakWait = 2f;
	bool waitingToRecharge = false;
	bool growing = false;
	Timer growTimer;
	float growDur = 2f;
	
	public override void Start(){
		base.Start();
		maxHP = 250;//50;
		hp = 250;//50;
		thetas = new Vector3(thingpoints[5]+0.05f,0f,0f);
		radii.x -= hitboxCorrection;
		thingDoingTimer = new Timer();
		eaterTimer = new Timer();
		directiontimer = new Timer();
		overlay = transform.FindChild("Health").gameObject;
		shieldImage = transform.FindChild("ShieldImage").GetComponent<Image>();
	}
	public override void HandleModeStuff(){
		float healthRatio = power/referenceCapacity;
		if(level == 1)
			return;
		else if(level == 2){
			if(mode < 1 && healthRatio <= .5)
				mode = 1;
		}else if(level == 3){
			if(mode < 2 && healthRatio <= .333)
				mode = 2;
			else if(mode < 1 && healthRatio <= .666)
				mode = 1;
		}else if(level >= 4){
			if(mode < 3 && healthRatio <= .25)
				mode = 3;
			else if(mode < 2 && healthRatio <= .5)
				mode = 2;
			else if(mode < 1 && healthRatio <= .75)
				mode = 1;	
		}
	}
	public override void Update(){
		base.Update();
		moving = !GamePause.paused;
		if (!moving)
			return;
			
		ShieldUpdate();
			
		if(mode == 0){ //shielding
			if(currentState == state.DECIDING){
				goingRight = !ShouldWeGoLeft(thingpoints[thingIdx]); //pick a direction to move in
				currentState = state.MOVING;
			}else if(currentState == state.MOVING){
				Arrive ();
			}else if(currentState == state.SHIELDING){
				ShieldingLoop ();
			}
		}
		else if(mode == 1){ //boosting regen
			if(currentState == state.DECIDING){
				goingRight = !ShouldWeGoLeft(thingpoints[thingIdx]); //pick a direction to move in
				currentState = state.MOVING;
			}else if(currentState == state.MOVING){
				Arrive ();
			}else if(currentState == state.SHIELDING){
				ShieldingLoop ();
			}else if(currentState == state.BOOSTING){
				BoostingLoop ();
			}
		}
		else if(mode == 2){ //blocking shots
			if(currentState == state.DECIDING){
				if(reachedMiddle)
					currentState = state.MOVING;
				else
					currentState = state.DIVING;
			}else if(currentState == state.MOVING){
				if(!reachedMiddle){
					currentState = state.DIVING;
					return;
				}
				if(mode3reversing){
					if(clockwise){
						thetas.z = -0.00005f;
					}else{
						thetas.z = 0.00005f;
					}
					if((!clockwise && thetas.y >= 0.008f)||(clockwise && thetas.y <= -0.008f))
						mode3reversing = false;
				}else{	
					thetas.y = 0.008f;
					if(clockwise)
						thetas.y *= -1;
				}
				if(directiontimer.TimeElapsedSecs() > secondsToReverse){
					clockwise = !clockwise;
					System.Random rand = new System.Random();
					secondsToReverse = MIN_SECS + (float)(rand.NextDouble()*(MAX_SECS-MIN_SECS));
					directiontimer.Restart();
					mode3reversing = true;
				}
			}else if(currentState == state.SHIELDING){
				ShieldingLoop ();
			}else if(currentState == state.BOOSTING){
				BoostingLoop ();
			}else if(currentState == state.DIVING){
				if(radii.x > 3*Dial.FULL_LENGTH/4f){
					radii.z = -0.05f;
				}else if(radii.x > Dial.FULL_LENGTH/2f){
					radii.z = 0.04f;
				}else{
					Debug.Log ("we finished diving");
					radii.x = Dial.FULL_LENGTH/2f;
					radii.y = 0f;
					radii.z = 0f;
					PickTargetZone();
					reachedMiddle = true;
					currentState = state.DECIDING;
				}
			}/*else if(currentState == state.RETREATING){
				if(radii.x < 3f*Dial.FULL_LENGTH/4f){
					radii.z = 0.05f;
				}else if(radii.x < Dial.FULL_LENGTH-hitboxCorrection){
					radii.z = -0.04f;
				}else{
					radii.x = Dial.FULL_LENGTH-hitboxCorrection;
					radii.y = 0f;
					radii.z = 0f;
					PickTargetZone();
					currentState = state.DECIDING;
				}
			}*/
		}
		else if(mode == 3){ //stealing
			//beams proceed independent of state
			List<Enemy> potentialTargets = Dial.GetAllShieldedEnemies();
			if(eaterTimer.TimeElapsedSecs() > eatDelay && potentialTargets.Count > 0){
				eaterTimer.Restart();
				GameObject beam = GameObject.Instantiate (Resources.Load ("Prefabs/MainCanvas/ShieldEater")) as GameObject;
				beam.transform.SetParent(transform,false);
				BigBulkShieldEater bbse = beam.GetComponent<BigBulkShieldEater>();
				bbse.SetBigBulk(this);
				int random = Random.Range(0,potentialTargets.Count);
				bbse.SetTarget(potentialTargets[random].gameObject);
				potentialTargets[random].MakeBulkDrainTarget();
			}
			//state handling
			if(currentState == state.DECIDING){
				goingRight = !ShouldWeGoLeft(thingpoints[thingIdx]); //pick a direction to move in
				currentState = state.MOVING;
			}else if(currentState == state.MOVING){
				if(reachedMiddle){
					reachedMiddle = false;
					currentState = state.RETREATING;
					return;
				}
				if(mode3reversing){
					if(clockwise){
						thetas.z = -0.00005f;
					}else{
						thetas.z = 0.00005f;
					}
					if((!clockwise && thetas.y >= 0.008f)||(clockwise && thetas.y <= -0.008f))
						mode3reversing = false;
				}else{	
					thetas.y = 0.008f;
					if(clockwise)
						thetas.y *= -1;
				}
				if(directiontimer.TimeElapsedSecs() > secondsToReverse){
					clockwise = !clockwise;
					System.Random rand = new System.Random();
					secondsToReverse = MIN_SECS + (float)(rand.NextDouble()*(MAX_SECS-MIN_SECS));
					directiontimer.Restart();
					mode3reversing = true;
				}
			}else if(currentState == state.SHIELDING){
				ShieldingLoop ();
			}else if(currentState == state.BOOSTING){
				BoostingLoop ();
			}else if(currentState == state.DIVING){
				if(radii.x > 3*Dial.FULL_LENGTH/4f){
					radii.z = -0.05f;
				}else if(radii.x > Dial.FULL_LENGTH/2f){
					radii.z = 0.04f;
				}else{
					radii.x = Dial.FULL_LENGTH/2f;
					currentState = state.RETREATING;
				}
			}else if(currentState == state.RETREATING){
				if(radii.x < 3f*Dial.FULL_LENGTH/4f){
					radii.z = 0.05f;
				}else if(radii.x < Dial.FULL_LENGTH-hitboxCorrection){
					radii.z = -0.04f;
				}else{
					radii.x = Dial.FULL_LENGTH-hitboxCorrection;
					radii.y = 0f;
					radii.z = 0f;
					currentState = state.DECIDING;
				}
			}
		}
	}
	#region BulkBehavior (stuff this boss does)
	List<Enemy> targets = new List<Enemy>();
	public void PrepTargets(){
		targets = Dial.GetAllEnemiesInZone(SpawnIndexToZoneID(thingIdx));
		if(mode == 0){
			List<System.Object> fragList = new List<System.Object>();
			Dictionary<string,System.Object> fragDict = new Dictionary<string,System.Object>();
			fragDict.Add("fragAngle",0.0);
			fragDict.Add("fragArc",360.0);
			fragList.Add(fragDict);
			
			foreach(Enemy e in targets){
				e.Freeze();
				if(e.GetShield() == null){
					e.GiveShield(10f,2f,0.04f,fragList);
					//e.GetShield().GrowShields();
				}else{
					e.GetShield().IncreaseAllShieldHP(10f);
				}
			}
		}else if(mode == 1){
			for(int i = 0; i < targets.Count; i++){ //remove unshielded enemies from targets
				if(targets[i].GetShield() == null){
					targets.RemoveAt(i);
					i--;
				}
			}
			particleTimer = new Timer();
			foreach(Enemy e in targets){
				e.GetShield().bulked = true;
				e.Freeze();
			}
		}
		
	}
	public void ShieldingLoop(){
		if(thingDoingTimer.TimeElapsedSecs() < shieldDelay){
			//do shield stuff
			Debug.Log ("adding shields to things");
		}else{
			foreach(Enemy e in targets){
				e.Unfreeze();
			}
			PickTargetZone();
			currentState = state.DECIDING;
		}
	}
	Timer particleTimer = null;
	public void BoostingLoop(){
		
		if(thingDoingTimer.TimeElapsedSecs() < boostDelay){
			//emit boost particles
			Debug.Log ("boosting things");
			if(particleTimer.TimeElapsedSecs() > 0.3f){
				if(targets.Count > 0){
					particleTimer.Restart();
					float randEnemyIdxF = Random.Range(0,targets.Count);
					int randEnemyIdx = (int)randEnemyIdxF;
					if(randEnemyIdx == targets.Count){randEnemyIdx--;}//remove inclusive max
					//Debug.Log(randEnemyIdx);
					Enemy e = targets[randEnemyIdx];
					GameObject particle = GameObject.Instantiate (Resources.Load ("Prefabs/MainCanvas/BoostParticle")) as GameObject;
					//Debug.Log ("we spawned it");
					particle.transform.SetParent(Dial.spawnLayer,false);
					Seeker s = particle.GetComponent<Seeker>();
					s.target = e.GetComponent<RectTransform>();
					s.GetComponent<RectTransform>().anchoredPosition = this.GetComponent<RectTransform>().anchoredPosition;
				}
			}
		}else{
			foreach(Enemy e in targets){
				e.Unfreeze();
			}
			PickTargetZone();
			currentState = state.DECIDING;
		}
	}
	#endregion
	#region Shields (stuff to do with its own shields)
	public void ShieldUpdate(){
		if(growing){
			float percent = growTimer.TimeElapsedSecs()/growDur;
			if(percent > 1){
				growing = false;
				shieldActive = true;
			}else{
				shieldImage.transform.localScale = new Vector3(percent,percent,1f);
			}
		}
		if(waitingToRecharge){
			if(brokenTimer.TimeElapsedSecs() >= breakWait){
				waitingToRecharge = false;
				BeginRegrowth();
			}
		}
		
		if(shieldActive){
			Regenerate ();
		}
	}
	public void ReceiveDrainedShields(float amount){
		//do nothing yet because it doesn't have a shield
	}
	public void GetBroken(){
		power = 0f;
		//shieldImage.gameObject.SetActive(false);
		shieldActive = false;
		MakeStuffRealTinyInPreparationForGrowing();
		brokenTimer = new Timer();
		waitingToRecharge = true;
	}
	public void MakeStuffRealTinyInPreparationForGrowing(){
		shieldImage.transform.localScale = new Vector3(0.01f,0.01f,1f);
	}
	public void SlowRegen(float slow){
		slowedAmount += slow;
		if(slowedAmount > 1)
			slowedAmount = 1f;
	}
	public void GrowShields(){
		growing = true;
		growTimer = new Timer();
	}
	public void BeginRegrowth(){
		GrowShields();
	}
	public float Drain(float hp){ //takes hp and returns how much was successfully taken
		power -= hp;
		if(power < 0){
			hp += power;
		}
		RefreshShieldColors();
		return hp;
	}
	public void ShieldAgainstBullet(Bullet b){
		float amount = b.dmg;
		power -= amount;
		RefreshShieldColors();
		if(power <= 0){
			GetBroken ();
		}

		GetStatused(b);
		
		if(b.penetration > 0){
			float penDamage = b.penetration*b.dmg;
			TakeDamage(penDamage);
		}
        //leftovers from old weapon system
		/*if(b.shieldShred > 0){
			float shredDamage = b.shieldShred*b.dmg;
			capacity -= shredDamage;
			if(power > capacity){
				power = capacity;
			}
		}
		if(b.slowsShields != 0){
			SlowRegen(b.slowsShields);
		}*/
	}
	public void ShieldAgainstTrap(Trap t){
		float amount = t.dmg;
		power -= amount;
		RefreshShieldColors();
		if(power <= 0){
			GetBroken ();
		}
		
		GetStatused(t);
		if(t.penetration > 0){
			float penDamage = t.penetration*t.dmg;
			TakeDamage(penDamage);
		}
		if(t.shieldShred > 0){
			float shredDamage = t.shieldShred*t.dmg;
			capacity -= shredDamage;
			if(power > capacity){
				power = capacity;
			}
		}
		/*if(t.slowsShields != 0){
			SlowRegen(t.slowsShields);
		}*/
	}
	public void Regenerate(){
		
		if(!leeched){
			if(power >= capacity)
				return;
			float regen = regenSpeed;
			if(slowedAmount > 0)
				regen *= (1f-slowedAmount);
			power += regen;
			if(power > capacity)
				power = capacity;
			RefreshShieldColors();
		}else{//leeched
			float regen = regenSpeed;
			if(slowedAmount > 0)
				regen *= (1f-slowedAmount);
			GameEvent ge = new GameEvent("health_leeched");
			ge.addArgument(regen);
			EventManager.Instance().RaiseEvent(ge);
			RefreshShieldColors();
		}
	}
	public void RefreshShieldColors(){
		float percent = power/referenceCapacity;
		shieldImage.color = new Color(percent,percent,1f);
	}
	
	
	public void HandleUnshieldedCollision(Collider2D coll){
		base.OnTriggerEnter2D(coll);
		overlay.transform.localScale = new Vector3(1f,hp/maxHP,1f);
	}
	public void HandleShieldedCollision(Collider2D coll){
		if (coll.gameObject.tag == "Bullet") //if it's a bullet
		{
			Bullet bc = coll.gameObject.GetComponent<Bullet> ();
			if (bc != null) {
				if (bc.CheckActive()){ //if we get a Yes, this bullet/trap/shield is active
                    bc.enemyHit = this.gameObject;
                    ShieldAgainstBullet(bc);
                    bc.Collide();
				}
			}
		}
		else if (coll.gameObject.tag == "Trap") //if it's a trap
		{
			Trap tc = coll.gameObject.GetComponent<Trap> ();
			if (tc != null) {
				if (tc.CheckActive()) //if we get a Yes, this bullet/trap/shield is active
				{
					tc.enemyHit = this.gameObject;
					ShieldAgainstTrap(tc);
					tc.Collide();
				}
			}
		}
		else if (coll.gameObject.tag == "Shield") //if it's a shield
		{}
		else if (coll.gameObject.tag == "AoE")
		{
			Debug.Log ("shield collided with AoE");
			GameObject obj = coll.gameObject;
			AoE ac = obj.GetComponent<AoE>();
			if (ac.parent == "Bullet")
			{
				if (ac.aoeBulletCon.enemyHit != this.gameObject) //if this isn't the enemy originally hit
				{
					//Debug.Log ("parent is bullet@");
					Bullet bc = ac.aoeBulletCon;
					ShieldAgainstBullet(bc);
				}
			}
			else if (ac.parent == "Trap")
			{
				if (ac.aoeTrapCon.enemyHit != this.gameObject) //if this isn't the enemy originally hit
				{
					Trap tc = ac.aoeTrapCon;
					ShieldAgainstTrap (tc);
				}
			}
			
		}
		//other types of collision?
	}
	public void ReceiveCollisionFromChildCollider(Collider2D coll,bool shieldColl){
		if(shieldColl && shieldActive){
			HandleShieldedCollision(coll);
		}else if(!shieldColl && !shieldActive){
			HandleUnshieldedCollision(coll);
		}
	}
	#endregion
	#region Movement (special movement stuff)
	public void ReverseDirection(){
		clockwise = !clockwise;
		System.Random rand = new System.Random();
		secondsToReverse = MIN_SECS + (float)(rand.NextDouble()*(MAX_SECS-MIN_SECS));
		directiontimer.Restart();
	}
	
	state ThingToDo(){
		if(mode == 0){
			return state.SHIELDING;
		}else if(mode == 1){
			return state.BOOSTING;
		}else if(mode == 2){
			return state.MOVING;
		}else if(mode == 3){
			return state.STEALING;
		}
		return state.DECIDING; //shouldn't happen
	}
	
	float maxAcc = 0.0008f;
	float maxVel = 0.003f;
	float arriveRadius = 0.005f;
	float decelRadius = 0.2f;
	float timeToDecel = 1f;
	public void Arrive(){
		float targetTheta = thingpoints[thingIdx];
		float dist = CircleDistance(targetTheta);
		//Debug.Log(dist + " is circle distance to " + targetTheta);
		float acc = maxAcc;
		if(goingRight){
			acc = -maxAcc;
		}
		if(dist <= arriveRadius){
			//thing enemy
			thetas.y = 0;
			thetas.z = 0;
			currentState = ThingToDo();
			PrepTargets();
			thingDoingTimer.Restart();
		}else if(dist > decelRadius){
			//accelerate
			thetas.z = acc;
			if(thetas.y > maxVel)
				thetas.y = maxVel;
			else if(thetas.y < -maxVel)
				thetas.y = -maxVel;
		}else{
			//decelerate
			float targetVel = maxVel * (dist / decelRadius); //get target velocity
			//Debug.Log("maxvel is " + maxVel + " dist is " + dist);
			if(goingRight)
				targetVel *= -1; //correct for direction
			acc = targetVel - thetas.y; //get difference between target and actual velocity
			//cap acceleration
			if(acc > maxAcc)
				acc = maxAcc;
			else if(acc < -maxAcc)
				acc = -maxAcc;
			
			acc /= timeToDecel; //reduce acceleration to slow it down	
			thetas.z = acc; //set acceleration
			
			//cap velocity
			if(thetas.y > maxVel)
				thetas.y = maxVel;
			else if(thetas.y < -maxVel)
				thetas.y = -maxVel;
		}
	}
	public void PickTargetZone(){
		if(directiontimer.TimeElapsedSecs() >= secondsToReverse){
			ReverseDirection(); 		
		}
		if(clockwise)
			thingIdx--;
		else
			thingIdx++;
		
		if(thingIdx >= thingpoints.Length)
			thingIdx -= thingpoints.Length;
		if(thingIdx <= -1)
			thingIdx += thingpoints.Length;
	}
	public bool ShouldWeGoLeft(float targ){
		float nocrossdist = Mathf.Abs(targ-thetas.x); //the distance traveled if you don't cross over the 0	
		bool linecross = nocrossdist > Mathf.PI; //should we cross the line?
		if(linecross){
			return thetas.x > Mathf.PI; //we're either past the halfway mark, which means the line is closest on our left side
			//or we're not past the halfway mark, in which case we need to go right to cross the line
		}else{
			return thetas.x < targ; //we're less than the target so we go left, or we're greater than the target so we go right
		}
	}
	public float CircleDistance(float targ){
		float nocrossdist = Mathf.Abs(targ-thetas.x); //ditto previous method		
		bool linecross = nocrossdist > Mathf.PI; //ditto previous method
		
		if(!linecross){
			//Debug.Log ("linecross and theta is " +thetas.x + " and targ is " + targ);
			return Mathf.Abs(thetas.x - targ); //just take direct distance
		}else {
			//Debug.Log ("no linecross and theta is " +thetas.x + " and targ is " + targ);
			float highval = 0f;
			float lowval = 0f;
			if(thetas.x > targ){
				highval = thetas.x;
				lowval = targ;
			}else{
				highval = targ;
				lowval = thetas.x;
			}
			return Mathf.Abs ((Mathf.PI-highval)+lowval); //don't ask me how this works, i'm not even sure it does
		}
	}
	#endregion
}
