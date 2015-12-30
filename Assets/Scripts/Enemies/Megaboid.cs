using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.IO;
using MiniJSON;

public class Megaboid : Boss{
	
	float[] mergepoints = {0.5236f,1.5708f,2.6180f,3.6652f,4.7124f,5.7596f}; //the middle of each zone, in radians from x-axis
	int mergeIdx = 0;
	bool goingRight = true;
	Timer mergeTimer;
	float mergeDelay = 3f;
	bool hitYet = false;
	
	Timer directiontimer;
	float secondsToReverse = 6f;
	readonly float MIN_SECS = 6f;
	readonly float MAX_SECS = 25f;
	bool clockwise = false;
	
	List<GameObject> facsimiles  = new List<GameObject>();
	Vector2 center = new Vector2(0f,0f);
	float progcenter = 0f;
	
	enum state{MERGING,SPAWNING,MOVING,DECIDING};
	state currentState = state.DECIDING;
	
	GameObject overlay;
	
	public override void Start(){
		base.Start();
		maxHP = 200;
		hp = 200;
		thetas = new Vector3(mergepoints[5]+0.05f,0f,0f);
		mergeTimer = new Timer();
		directiontimer = new Timer();
		overlay = transform.FindChild("Health").gameObject;
	}
	public override void Update(){
		Debug.Log ("hit yet: " + hitYet + ", mode: " + mode + ", state: " + currentState);
		base.Update();
		moving = !GamePause.paused;
		if (!moving)
			return;
			
		//handle movement and merging
		if(currentState == state.DECIDING){
			goingRight = !ShouldWeGoLeft(mergepoints[mergeIdx]); //pick a direction to move in
			currentState = state.MOVING;
		}else if(currentState == state.MOVING){
			Arrive ();
		}else if(currentState == state.SPAWNING){
			System.Random rand = new System.Random();
			List<Enemy> enemies = Dial.GetAllEnemiesInZone(SpawnIndexToZoneID(mergeIdx));
			bool hasBlob = false;
			bool hasMega = false;
			foreach(Enemy e in enemies){
				if(e.GetSrcFileName().Equals("blob")){
					hasBlob = true;
				}
				if(e.GetSrcFileName().Equals("megasplit") || e.GetSrcFileName().Equals("minisplit")){
					hasMega = true;
				}
			}
			//decide whether to drop an enemy, and which one
			if(hasBlob){
				if(hasMega){
					BeginMerge();
					return; //short circuit this update so the next frame is in the MERGING state
				}else{
					DropMegasplit();
				}
			}else{
				if(hasMega){
					DropBlob();
				}else{
					if(rand.NextDouble() < 0.5)
						DropMegasplit();
					else
						DropBlob();
				}
			}
			//end spawn state
			PickTargetZone();
			hitYet = false;
			currentState = state.DECIDING;
		}else if(currentState == state.MERGING){
			//update enemy facsimiles if necessary
			
			//check if it's time to move on, then move on
			if(mergeTimer.TimeElapsedSecs() >= mergeDelay || mode == 0){
				FinishMerge();
				PickTargetZone();
				hitYet = false;
				currentState = state.DECIDING;
			}
		}
	}
	void BeginMerge(){
		//get everything in the lane
		List<Enemy> zoneGuys = Dial.GetAllEnemiesInZone(SpawnIndexToZoneID(mergeIdx));
		if(zoneGuys.Count < 2){
			//abort merge and move on
			Debug.Log ("not enough enemies, merge aborted");
			PickTargetZone();
			hitYet = false;
			currentState = state.DECIDING;
			return;
		}
		//create facsimilies
		facsimiles.Clear();
		Vector2 centerTotals = new Vector2(0f,0f);
		float progTotals = 0f;
		foreach(Enemy e in zoneGuys){
			GameObject facs = GameObject.Instantiate(e.gameObject);
			GameObject.Destroy(facs.GetComponent<Enemy>());
			MegaboidFacsimile mf = facs.AddComponent<MegaboidFacsimile>() as MegaboidFacsimile;
			facs.transform.SetParent(Dial.spawnLayer,false);
			facsimiles.Add(facs);
			//get their centers
			centerTotals += e.gameObject.GetComponent<RectTransform>().anchoredPosition;
			progTotals += e.GetProgress();
		}
		center = new Vector2(centerTotals.x/zoneGuys.Count,centerTotals.y/zoneGuys.Count);
		progcenter = progTotals/zoneGuys.Count;
		foreach(GameObject mfg in facsimiles){
			mfg.GetComponent<MegaboidFacsimile>().InitializeMovement(center,mergeTimer);
		}
		
		//old guys: kill 'em all
		foreach(Enemy e in zoneGuys){
			Dial.CallEnemyAddBonus(e);
		}
		foreach(Enemy e in zoneGuys){
			Destroy (e.gameObject);
		}
	
		currentState = state.MERGING;
		mergeTimer.Restart();
	}
	void FinishMerge(){
		foreach(GameObject facs in facsimiles){
			Destroy (facs);
		}
		CreateSuper();
	}
	
	void CreateSuper(){
		int modey = mode;
		if(modey > 3)
			modey = 3; //shouldn't happen, but jic
		string minionfile = "boidminion" + modey;
		FileLoader fl = new FileLoader ("JSONData" + Path.DirectorySeparatorChar + "Bestiary",minionfile);
		string actualenemyjson = fl.Read ();
		Dictionary<string,System.Object> actualenemydict = Json.Deserialize(actualenemyjson) as Dictionary<string,System.Object>;
		string filename = minionfile;
		int track = SpawnIndexToZoneID(mergeIdx);
		int trackpos = 0;
		
		GameObject enemyobj = GameObject.Instantiate (Resources.Load ("Prefabs/MainCanvas/Enemy")) as GameObject;
		enemyobj.transform.SetParent(Dial.spawnLayer,false);
		Enemy e = enemyobj.GetComponent<Enemy>();
		
		e.SetSrcFileName(filename);
		e.SetTrackID(track);
		e.SetTrackLane(trackpos);
		e.SetProgress(progcenter);
		//calculate and set position
		((RectTransform)enemyobj.transform).anchoredPosition = center;
		
		e.spawnedByBoss = true;
		e.StartMoving();
	}
	
	void DropMegasplit(){
		FileLoader fl = new FileLoader ("JSONData" + Path.DirectorySeparatorChar + "Bestiary","megasplit");
		string actualenemyjson = fl.Read ();
		Dictionary<string,System.Object> actualenemydict = Json.Deserialize(actualenemyjson) as Dictionary<string,System.Object>;
		string filename = "megasplit";
		int track = SpawnIndexToZoneID(mergeIdx);
		int trackpos = 0;
		
		GameObject enemyobj = GameObject.Instantiate (Resources.Load ("Prefabs/MainCanvas/Enemy")) as GameObject;
		GameObject.Destroy(enemyobj.GetComponent<Enemy>());
		Megasplit ms = enemyobj.AddComponent<Megasplit>() as Megasplit;
		enemyobj.transform.SetParent(Dial.spawnLayer,false);
		
		ms.SetSrcFileName(filename);
		ms.SetTrackID(track);
		ms.SetTrackLane(trackpos);
		//calculate and set position
		float degrees = (track-1)*60; //clockwise of y-axis
		degrees += 15*trackpos; //negative trackpos is left side, positive is right side, 0 is middle
		degrees = ((360-degrees) + 90)%360; //convert to counterclockwise of x axis
		degrees *= Mathf.Deg2Rad;
		((RectTransform)enemyobj.transform).anchoredPosition = new Vector2(Dial.ENEMY_SPAWN_LENGTH*Mathf.Cos(degrees),
		                                                                   Dial.ENEMY_SPAWN_LENGTH*Mathf.Sin(degrees));
		                                                                   
		ms.spawnedByBoss = true;
		ms.StartMoving();
	}
	void DropBlob(){
		FileLoader fl = new FileLoader ("JSONData" + Path.DirectorySeparatorChar + "Bestiary","blob");
		string actualenemyjson = fl.Read ();
		Dictionary<string,System.Object> actualenemydict = Json.Deserialize(actualenemyjson) as Dictionary<string,System.Object>;
		string filename = "blob";
		int track = SpawnIndexToZoneID(mergeIdx);
		int trackpos = 0;
		
		GameObject enemyobj = GameObject.Instantiate (Resources.Load ("Prefabs/MainCanvas/Enemy")) as GameObject;
		GameObject.Destroy(enemyobj.GetComponent<Enemy>());
		Blob b = enemyobj.AddComponent<Blob>() as Blob;
		enemyobj.transform.SetParent(Dial.spawnLayer,false);
		
		b.SetSrcFileName(filename);
		b.SetTrackID(track);
		b.SetTrackLane(trackpos);
		//calculate and set position
		float degrees = (track-1)*60; //clockwise of y-axis
		degrees += 15*trackpos; //negative trackpos is left side, positive is right side, 0 is middle
		degrees = ((360-degrees) + 90)%360; //convert to counterclockwise of x axis
		degrees *= Mathf.Deg2Rad;
		((RectTransform)enemyobj.transform).anchoredPosition = new Vector2(Dial.ENEMY_SPAWN_LENGTH*Mathf.Cos(degrees),
		                                                                   Dial.ENEMY_SPAWN_LENGTH*Mathf.Sin(degrees));
		
		b.spawnedByBoss = true;
		b.StartMoving();
	}
	
	public void PickTargetZone(){
		if(directiontimer.TimeElapsedSecs() >= secondsToReverse){
			ReverseDirection(); 		
		}
		if(clockwise)
			mergeIdx--;
		else
			mergeIdx++;
		if(mergeIdx >= mergepoints.Length)
			mergeIdx -= mergepoints.Length;
		if(mergeIdx <= -1)
			mergeIdx += mergepoints.Length;
	}
	public override void OnTriggerEnter2D(Collider2D coll){
		base.OnTriggerEnter2D(coll);
		overlay.transform.localScale = new Vector3(hp/maxHP,hp/maxHP,1f);
		if(!coll.gameObject.tag.Equals("Enemy"))
			hitYet = true;
	}
	public void ReverseDirection(){
		clockwise = !clockwise;
		System.Random rand = new System.Random();
		secondsToReverse = MIN_SECS + (float)(rand.NextDouble()*(MAX_SECS-MIN_SECS));
		directiontimer.Restart();
	}
	
	
	float maxAcc = 0.0008f;
	float maxVel = 0.003f;
	float arriveRadius = 0.005f;
	float decelRadius = 0.2f;
	float timeToDecel = 1f;
	public void Arrive(){
		float targetTheta = mergepoints[mergeIdx];
		float dist = CircleDistance(targetTheta);
		//Debug.Log(dist + " is circle distance to " + targetTheta);
		float acc = maxAcc;
		if(goingRight){
			acc = -maxAcc;
		}
		if(dist <= arriveRadius){
			//spawn enemy
			thetas.y = 0;
			thetas.z = 0;
			//decide whether to merge or spawn
			if(hitYet){
				currentState = state.SPAWNING;
			}else{
				BeginMerge();
			}
			mergeTimer.Restart();
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
}

