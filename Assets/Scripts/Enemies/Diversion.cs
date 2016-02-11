using UnityEngine.UI;
using UnityEngine;
using System.Collections.Generic;

public class Diversion : Enemy{
	
	int numberOfFollowers = 10;
	float delay = .8f;
	Timer followerSpawn;
	bool[] spawnsDone = {false,false,false,false};
	List<DiversionMinion> followers = new List<DiversionMinion>();
	bool playingDead = false;
	
	public bool groupAddedToBonus = false;
	
	bool doneSpawning = false;
	float batch1;
	float batch2;
	float batch3;
	float batch4;
	
	public override void Start(){
		base.Start();
		followerSpawn = new Timer();
		followerSpawn.Restart();
	}
	public override void Update(){
		if (!moving){
			base.Update();
			return;
		}
		if(!doneSpawning && followers.Count < numberOfFollowers && followerSpawn.TimeElapsedSecs() >= delay){
			SpawnBatch();
		}
		if(!playingDead)
			base.Update();
	}
	public void SetDelay(float f){
		delay = f;
		batch1 = f;
		batch2 = 1.5f*f;
		batch3 = 2f*f;
		batch4 = 2.5f*f;
	}
	public void SpawnBatch(){
		if(!spawnsDone[0]){
			//spawn first set
			for(int i = 0; i < 4; i++){
				GameObject enemyspawn = GameObject.Instantiate (Resources.Load ("Prefabs/MainCanvas/Enemy")) as GameObject;
				Destroy (enemyspawn.GetComponent<Enemy>());
				DiversionMinion minion = enemyspawn.AddComponent<DiversionMinion>() as DiversionMinion;
				enemyspawn.transform.SetParent(Dial.spawnLayer,false);
				minion.numberOfFollowers = numberOfFollowers;
				minion.leader = this;
				minion.SetSrcFileName("diversionminion");
				minion.SetTrackID(trackID);
				minion.SetTrackLane(trackLane);
				minion.groupAddedToBonus = groupAddedToBonus;
				if(i == 0)
					minion.OverrideMoverLane(9f);
				else if(i == 1)
					minion.OverrideMoverLane(3f);
				else if(i == 2)
					minion.OverrideMoverLane(-3f);
				else if(i == 3)
					minion.OverrideMoverLane(-9f);
				
				followers.Add(minion);
				foreach(DiversionMinion dm in followers){
					dm.AddFollower(minion);
					minion.AddFollower(dm);
				}
				minion.SetPositionBasedOnAngle();
				minion.StartMoving();
			}
			spawnsDone[0] = true;
			delay = batch2;
		}else if(!spawnsDone[1]){
			//spawn second set
			for(int i = 0; i < 3; i++){
				GameObject enemyspawn = GameObject.Instantiate (Resources.Load ("Prefabs/MainCanvas/Enemy")) as GameObject;
				Destroy (enemyspawn.GetComponent<Enemy>());
				DiversionMinion minion = enemyspawn.AddComponent<DiversionMinion>() as DiversionMinion;
				enemyspawn.transform.SetParent(Dial.spawnLayer,false);
				minion.numberOfFollowers = numberOfFollowers;
				minion.leader = this;
				minion.SetSrcFileName("diversionminion");
				minion.SetTrackID(trackID);
				minion.SetTrackLane(trackLane);
				minion.groupAddedToBonus = groupAddedToBonus;
				if(i == 0)
					minion.OverrideMoverLane(7.5f);
				else if(i == 1)
					minion.OverrideMoverLane(0f);
				else if(i == 2)
					minion.OverrideMoverLane(-7.5f);
				
				followers.Add(minion);
				foreach(DiversionMinion dm in followers){
					dm.AddFollower(minion);
					minion.AddFollower(dm);
				}
				minion.SetPositionBasedOnAngle();
				minion.StartMoving();
			}
			spawnsDone[1] = true;
			delay = batch3;
		}else if(!spawnsDone[2]){
			//spawn third set
			Debug.Log ("spawning third wave");
			for(int i = 0; i < 2; i++){
				GameObject enemyspawn = GameObject.Instantiate (Resources.Load ("Prefabs/MainCanvas/Enemy")) as GameObject;
				Destroy (enemyspawn.GetComponent<Enemy>());
				DiversionMinion minion = enemyspawn.AddComponent<DiversionMinion>() as DiversionMinion;
				enemyspawn.transform.SetParent(Dial.spawnLayer,false);
				minion.numberOfFollowers = numberOfFollowers;
				minion.leader = this;
				minion.SetSrcFileName("diversionminion");
				minion.SetTrackID(trackID);
				minion.SetTrackLane(trackLane);
				minion.groupAddedToBonus = groupAddedToBonus;
				if(i == 0)
					minion.OverrideMoverLane(-5f);
				else if(i == 1)
					minion.OverrideMoverLane(5f);
				
				followers.Add(minion);
				foreach(DiversionMinion dm in followers){
					dm.AddFollower(minion);
					minion.AddFollower(dm);
				}
				minion.SetPositionBasedOnAngle();
				minion.StartMoving();
			}
			spawnsDone[2] = true;
			delay = batch4;
		}else if(!spawnsDone[3]){
			//spawn fourth set
			Debug.Log ("spawning fourth wave");
			GameObject enemyspawn = GameObject.Instantiate (Resources.Load ("Prefabs/MainCanvas/Enemy")) as GameObject;
			Destroy (enemyspawn.GetComponent<Enemy>());
			DiversionMinion minion = enemyspawn.AddComponent<DiversionMinion>() as DiversionMinion;
			enemyspawn.transform.SetParent(Dial.spawnLayer,false);
			minion.numberOfFollowers = numberOfFollowers;
			minion.leader = this;
			minion.SetSrcFileName("diversionminion");
			minion.SetTrackID(trackID);
			minion.SetTrackLane(trackLane);
			minion.OverrideMoverLane(0f);
			minion.groupAddedToBonus = groupAddedToBonus;
			
			followers.Add(minion);
			foreach(DiversionMinion dm in followers){
				dm.AddFollower(minion);
				minion.AddFollower(dm);
			}
			minion.SetPositionBasedOnAngle();
			minion.StartMoving();
			
			spawnsDone[3] = true;
			doneSpawning = true;
		}else{
			Debug.Log("we're trying to spawn even though we're done");
		}
	}
	public void TellAboutNewFollower(DiversionMinion dm){
		if(!followers.Contains(dm))
			followers.Add(dm);
	}
	public void FillFollowers(List<DiversionMinion> list){
		foreach(DiversionMinion dm in list){
			followers.Add(dm);
		}
		//Debug.Log(followers.Count + " followers");
	}
	public override void AddToBonus(List<System.Object> bonusList){
		if(!groupAddedToBonus){
			Debug.Log ("adding a diversion");
			Dictionary<string,System.Object> enemyDict = new Dictionary<string,System.Object>();
			enemyDict.Add("enemyID",srcFileName);
			enemyDict.Add("trackID",(long)GetCurrentTrackID());
			if(!spawnedByBoss)
				bonusList.Add(enemyDict);
			
			//tell everyone else not to do the thing
			groupAddedToBonus = true;
			foreach(DiversionMinion dm in followers){
				dm.groupAddedToBonus = true;
			}
		}
	}
	public override void Die(){
		dead = true;
		if (hp <= 0.0f) {
			dialCon.IncreaseSuperPercent();
		}
		bool everyonesHere = followers.Count + 1 >= numberOfFollowers;
		bool playing = true;
		foreach(DiversionMinion dm in followers){
			if(!dm.IsPlayingDead()){
				playing = false;
				break;
			}
		}
		Debug.Log ("diversion: " + everyonesHere + " and " + playing);
		bool everyonesPlayingDead = everyonesHere && playing;
		
		if(everyonesPlayingDead){
			RealDie();
		}else{
			PlayDead();
		}
		
	}
	public void PlayDead(){
		playingDead = true;
		gameObject.GetComponent<Image>().enabled = false;
		transform.FindChild("Health").GetComponent<Image>().enabled = false;
		Destroy (GetComponent<Collider2D>());
	}
	public void RealDie(){
		RectTransform rt = (RectTransform)transform;
		if(hp <= 0){
			System.Random r = new System.Random ();
			float rng = (float)r.NextDouble() * 100; //random float between 0 and 100
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
		//TODO:put yourself in the missedwave queue
		foreach(DiversionMinion dm in followers){
			Destroy (dm.gameObject);
		}
		Destroy (this.gameObject);
	}
	public override void OnTriggerEnter2D(Collider2D coll){
		if(playingDead)
			return;
		base.OnTriggerEnter2D(coll);
	}
	public bool IsPlayingDead(){
		return playingDead;
	}
}