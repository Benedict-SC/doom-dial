using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.IO;
using MiniJSON;

public class SwarmMaster : Boss{

	float[] spawnpoints = {0.5236f,1.5708f,2.6180f,3.6652f,4.7124f,5.7596f}; //the middle of each zone, in radians from x-axis
	int spawnIdx = 0;
	bool goingRight = true;
	Timer spawntimer;
	float spawnDelay = 5f;
	bool spawnedOnce = false;
	
	Timer directiontimer;
	float secondsToReverse = 6f;
	readonly float MIN_SECS = 6f;
	readonly float MAX_SECS = 25f;
	bool clockwise = false;
	bool mode3reversing = false;
	
	Timer pooptimer;
	float secondsToPoop = 1f;
	readonly float MIN_POOP = 0.5f;
	readonly float MAX_POOP = 2.5f;
	
	enum state{SPAWNING,MOVING,DECIDING};
	state currentState = state.DECIDING;
	
	GameObject overlay;
	
	public override void Start(){
		base.Start();
		maxHP = 200;
		hp = 200;
		thetas = new Vector3(spawnpoints[5]+0.05f,0f,0f);
		spawntimer = new Timer();
		directiontimer = new Timer();
		pooptimer = new Timer();
		overlay = transform.FindChild("Health").gameObject;
	}
	public override void Update(){
		base.Update();
		moving = !GamePause.paused;
		if (!moving)
			return;
		if(mode == 0){
			if(currentState == state.DECIDING){
				goingRight = !ShouldWeGoLeft(spawnpoints[spawnIdx]); //pick a direction to move in
				currentState = state.MOVING;
			}else if(currentState == state.MOVING){
				Arrive ();
			}else if(currentState == state.SPAWNING){
				System.Random rand = new System.Random();
				if(rand.NextDouble() < 0.5)
					DropChainers();
				else
					DropSpear();
				if(directiontimer.TimeElapsedSecs() >= secondsToReverse){
					ReverseDirection(); 		
				}
				if(clockwise)
					spawnIdx--;
				else
					spawnIdx++;
				if(spawnIdx >= spawnpoints.Length)
					spawnIdx -= spawnpoints.Length;
				if(spawnIdx <= -1)
					spawnIdx += spawnpoints.Length;
					
				currentState = state.DECIDING;
			}
		}else if(mode == 1){
			if(currentState == state.DECIDING){
				goingRight = !ShouldWeGoLeft(spawnpoints[spawnIdx]); //pick a direction to move in
				currentState = state.MOVING;
			}else if(currentState == state.MOVING){
				Arrive ();
			}else if(currentState == state.SPAWNING){
				System.Random rand = new System.Random();
				if(rand.NextDouble() < 0.5)
					DropWall();
				else
					DropDiversion();
				if(directiontimer.TimeElapsedSecs() >= secondsToReverse){
					ReverseDirection(); 		
				}
				if(clockwise)
					spawnIdx--;
				else
					spawnIdx++;
				if(spawnIdx >= spawnpoints.Length)
					spawnIdx -= spawnpoints.Length;
				if(spawnIdx <= -1)
					spawnIdx += spawnpoints.Length;
				
				currentState = state.DECIDING;
			}
		}else if(mode == 2){
			if(currentState == state.DECIDING){
				goingRight = !ShouldWeGoLeft(spawnpoints[spawnIdx]); //pick a direction to move in
				currentState = state.MOVING;
			}else if(currentState == state.MOVING){
				Arrive ();
			}else if(currentState == state.SPAWNING){
				if(!spawnedOnce){
					System.Random rand = new System.Random();
					if(rand.NextDouble() < 0.5)
						DropChainers();
					else
						DropSpear();
					spawnedOnce = true;
					return;
				}else if(spawntimer.TimeElapsedSecs() < spawnDelay){
					return;
				}else{
					System.Random rand = new System.Random();
					if(rand.NextDouble() < 0.5)
						DropChainers();
					else
						DropSpear();
					spawnedOnce = false;
				}
				if(directiontimer.TimeElapsedSecs() >= secondsToReverse){
					ReverseDirection(); 		
				}
				if(clockwise)
					spawnIdx--;
				else
					spawnIdx++;
				if(spawnIdx >= spawnpoints.Length)
					spawnIdx -= spawnpoints.Length;
				if(spawnIdx <= -1)
					spawnIdx += spawnpoints.Length;
				
				currentState = state.DECIDING;
			}
		}else if(mode == 3){
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
			if(pooptimer.TimeElapsedSecs() > secondsToPoop){
				pooptimer.Restart();
				System.Random rand = new System.Random();
				secondsToPoop = MIN_POOP + (float)(rand.NextDouble()*(MAX_POOP-MIN_POOP));
				Poop ();
			}
			if(directiontimer.TimeElapsedSecs() > secondsToReverse){
				clockwise = !clockwise;
				System.Random rand = new System.Random();
				secondsToReverse = MIN_SECS + (float)(rand.NextDouble()*(MAX_SECS-MIN_SECS));
				directiontimer.Restart();
				mode3reversing = true;
			}
		}
	}
	public override void OnTriggerEnter2D(Collider2D coll){
		base.OnTriggerEnter2D(coll);
		overlay.transform.localScale = new Vector3(1f,hp/maxHP,1f);
	}
	public void ReverseDirection(){
		clockwise = !clockwise;
		System.Random rand = new System.Random();
		secondsToReverse = MIN_SECS + (float)(rand.NextDouble()*(MAX_SECS-MIN_SECS));
		directiontimer.Restart();
	}
	public void Poop(){
		if(PositionToZoneID() == 999){ //don't poop on zone borders
			secondsToPoop = 0.25f;
			pooptimer.Restart();
		}
		FileLoader fl = new FileLoader ("JSONData" + Path.DirectorySeparatorChar + "Bestiary","swarmminion");
		string actualenemyjson = fl.Read ();
		Dictionary<string,System.Object> actualenemydict = Json.Deserialize(actualenemyjson) as Dictionary<string,System.Object>;
		string filename = "swarmminion";
		int track = PositionToZoneID();
		
		//int poopdegrees = (int)(thetas.x * Mathf.Rad2Deg);
		//Debug.Log (poopdegrees + " pooping on track " + track);
		
		int trackpos = PositionToLaneID();
		
		GameObject enemyobj = GameObject.Instantiate (Resources.Load ("Prefabs/MainCanvas/Enemy")) as GameObject;
		Enemy ec = enemyobj.GetComponent<Enemy>();
		enemyobj.transform.SetParent(Dial.spawnLayer,false);
		ec.SetSrcFileName(filename);
		ec.SetTrackID(track);
		ec.SetTrackLane(trackpos);
		//calculate and set position
		float degrees = (track-1)*60; //clockwise of y-axis
		degrees += 15*trackpos; //negative trackpos is left side, positive is right side, 0 is middle
		degrees = ((360-degrees) + 90)%360; //convert to counterclockwise of x axis
		degrees *= Mathf.Deg2Rad;
		((RectTransform)enemyobj.transform).anchoredPosition = new Vector2(Dial.ENEMY_SPAWN_LENGTH*Mathf.Cos(degrees),
		                                                                   Dial.ENEMY_SPAWN_LENGTH*Mathf.Sin(degrees));
		ec.StartMoving();	
	}
	public void DropChainers(){
		FileLoader fl = new FileLoader ("JSONData" + Path.DirectorySeparatorChar + "Bestiary","chainers");
		string actualenemyjson = fl.Read ();
		Dictionary<string,System.Object> actualenemydict = Json.Deserialize(actualenemyjson) as Dictionary<string,System.Object>;
		string filename = "chainers";
		int track = SpawnIndexToZoneID(spawnIdx);
		int trackpos = 0;
		
		GameObject enemyobj = GameObject.Instantiate (Resources.Load ("Prefabs/MainCanvas/Enemy")) as GameObject;
		GameObject.Destroy(enemyobj.GetComponent<Enemy>());
		Chainer c = enemyobj.AddComponent<Chainer>() as Chainer;
		enemyobj.transform.SetParent(Dial.spawnLayer,false);
		float chaindelay = (float)(double)actualenemydict["delay"];
		c.delay = chaindelay;
		
		c.SetSrcFileName(filename);
		c.SetTrackID(track);
		c.SetTrackLane(trackpos);
		//calculate and set position
		float degrees = (track-1)*60; //clockwise of y-axis
		degrees += 15*trackpos; //negative trackpos is left side, positive is right side, 0 is middle
		degrees = ((360-degrees) + 90)%360; //convert to counterclockwise of x axis
		degrees *= Mathf.Deg2Rad;
		((RectTransform)enemyobj.transform).anchoredPosition = new Vector2(Dial.ENEMY_SPAWN_LENGTH*Mathf.Cos(degrees),
										Dial.ENEMY_SPAWN_LENGTH*Mathf.Sin(degrees));
		c.spawnedByBoss = true;
		c.StartMoving();
	}
	public void DropSpear(){
		FileLoader fl = new FileLoader ("JSONData" + Path.DirectorySeparatorChar + "Bestiary","tipofthespear");
		string actualenemyjson = fl.Read ();
		Dictionary<string,System.Object> actualenemydict = Json.Deserialize(actualenemyjson) as Dictionary<string,System.Object>;
		string filename = "tipofthespear";
		int track = SpawnIndexToZoneID(spawnIdx);
		int trackpos = 0;
		
		GameObject enemyobj = GameObject.Instantiate (Resources.Load ("Prefabs/MainCanvas/Enemy")) as GameObject;
		GameObject.Destroy(enemyobj.GetComponent<Enemy>());
		TipOfTheSpear tots = enemyobj.AddComponent<TipOfTheSpear>() as TipOfTheSpear;
		enemyobj.transform.SetParent(Dial.spawnLayer,false);
		float chaindelay = (float)(double)actualenemydict["delay"];
		tots.SetDelay(chaindelay);
		tots.leader = true;
		
		tots.SetSrcFileName(filename);
		tots.SetTrackID(track);
		tots.SetTrackLane(trackpos);
		//calculate and set position
		float degrees = (track-1)*60; //clockwise of y-axis
		degrees += 15*trackpos; //negative trackpos is left side, positive is right side, 0 is middle
		degrees = ((360-degrees) + 90)%360; //convert to counterclockwise of x axis
		degrees *= Mathf.Deg2Rad;
		((RectTransform)enemyobj.transform).anchoredPosition = new Vector2(Dial.ENEMY_SPAWN_LENGTH*Mathf.Cos(degrees),
		                                                                   Dial.ENEMY_SPAWN_LENGTH*Mathf.Sin(degrees));
		tots.spawnedByBoss = true;
		tots.StartMoving();
	}
	public void DropWall(){
		FileLoader fl = new FileLoader ("JSONData" + Path.DirectorySeparatorChar + "Bestiary","wallofdoom");
		string actualenemyjson = fl.Read ();
		Dictionary<string,System.Object> actualenemydict = Json.Deserialize(actualenemyjson) as Dictionary<string,System.Object>;
		string filename = "wallofdoom";
		int track = SpawnIndexToZoneID(spawnIdx);
		int trackpos = 0;
		
		GameObject enemyobj = GameObject.Instantiate (Resources.Load ("Prefabs/MainCanvas/Enemy")) as GameObject;
		GameObject.Destroy(enemyobj.GetComponent<Enemy>());
		WallOfDoom wod = enemyobj.AddComponent<WallOfDoom>() as WallOfDoom;
		enemyobj.transform.SetParent(Dial.spawnLayer,false);
		
		wod.SetSrcFileName(filename);
		wod.SetTrackID(track);
		wod.SetTrackLane(trackpos);
		//calculate and set position
		float degrees = (track-1)*60; //clockwise of y-axis
		degrees += 15*trackpos; //negative trackpos is left side, positive is right side, 0 is middle
		degrees = ((360-degrees) + 90)%360; //convert to counterclockwise of x axis
		degrees *= Mathf.Deg2Rad;
		((RectTransform)enemyobj.transform).anchoredPosition = new Vector2(Dial.ENEMY_SPAWN_LENGTH*Mathf.Cos(degrees),
		                                                                   Dial.ENEMY_SPAWN_LENGTH*Mathf.Sin(degrees));
		wod.spawnedByBoss = true;
		wod.StartMoving();
	}
	public void DropDiversion(){
		FileLoader fl = new FileLoader ("JSONData" + Path.DirectorySeparatorChar + "Bestiary","thediversion");
		string actualenemyjson = fl.Read ();
		Dictionary<string,System.Object> actualenemydict = Json.Deserialize(actualenemyjson) as Dictionary<string,System.Object>;
		string filename = "thediversion";
		int track = SpawnIndexToZoneID(spawnIdx);
		int trackpos = 0;
		
		GameObject enemyobj = GameObject.Instantiate (Resources.Load ("Prefabs/MainCanvas/Enemy")) as GameObject;
		GameObject.Destroy(enemyobj.GetComponent<Enemy>());
		Diversion d = enemyobj.AddComponent<Diversion>() as Diversion;
		enemyobj.transform.SetParent(Dial.spawnLayer,false);
		float chaindelay = (float)(double)actualenemydict["delay"];
		d.SetDelay(chaindelay);
		
		d.SetSrcFileName(filename);
		d.SetTrackID(track);
		d.SetTrackLane(trackpos);
		//calculate and set position
		float degrees = (track-1)*60; //clockwise of y-axis
		degrees += 15*trackpos; //negative trackpos is left side, positive is right side, 0 is middle
		degrees = ((360-degrees) + 90)%360; //convert to counterclockwise of x axis
		degrees *= Mathf.Deg2Rad;
		((RectTransform)enemyobj.transform).anchoredPosition = new Vector2(Dial.ENEMY_SPAWN_LENGTH*Mathf.Cos(degrees),
		                                                                   Dial.ENEMY_SPAWN_LENGTH*Mathf.Sin(degrees));
		d.spawnedByBoss = true;
		d.StartMoving();
	}
	
	float maxAcc = 0.0008f;
	float maxVel = 0.003f;
	float arriveRadius = 0.005f;
	float decelRadius = 0.2f;
	float timeToDecel = 1f;
	public void Arrive(){
		float targetTheta = spawnpoints[spawnIdx];
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
			currentState = state.SPAWNING;
			spawntimer.Restart();
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
