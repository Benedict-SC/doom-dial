using UnityEngine;
using System.Collections.Generic;

public class TipOfTheSpear : EnemyController{
	
	int numberOfFollowers = 5;
	float delay = .8f;
	public bool leader = false;
	Timer partnerSpawn;
	bool[] spawnsDone = {false,false};
	List<TipOfTheSpear> partners = new List<TipOfTheSpear>();
	bool playingDead = false;
	
	bool doneSpawning = false;
	float batch1;
	float batch2;
	
	public override void Start(){
		base.Start();
		partnerSpawn = new Timer();
		partnerSpawn.Restart();
	}
	public override void Update(){
		if (!moving)
			return;
		if(leader && !doneSpawning && partners.Count < numberOfFollowers && partnerSpawn.TimeElapsedSecs() >= delay){
			SpawnBatch();
		}
		if(!playingDead)
			base.Update();
	}
	public void SetDelay(float f){
		delay = f;
		batch1 = f;
		batch2 = 2f*f;
	}
	public void SpawnBatch(){
		if(!spawnsDone[0]){
			//spawn first set
			for(int i = 0; i < 2; i++){
				GameObject enemyspawn = GameObject.Instantiate (Resources.Load ("Prefabs/Enemy")) as GameObject;
				Destroy (enemyspawn.GetComponent<EnemyController>());
				TipOfTheSpear minion = enemyspawn.AddComponent<TipOfTheSpear>() as TipOfTheSpear;
				minion.numberOfFollowers = numberOfFollowers;
				minion.SetSrcFileName("tipofthespear");
				minion.SetTrackID(trackID);
				minion.SetTrackLane(trackLane);
				if(i == 0)
					minion.OverrideMoverLane(7.5f);
				else if(i == 1)
					minion.OverrideMoverLane(-7.5f);
				
				partners.Add(minion);
				foreach(TipOfTheSpear tots in partners){
					tots.AddPartner(minion);
				}
				minion.StartMoving();
			}
			spawnsDone[0] = true;
			delay = batch2;
		}else if(!spawnsDone[1]){
			//spawn second set
			for(int i = 0; i < 2; i++){
				GameObject enemyspawn = GameObject.Instantiate (Resources.Load ("Prefabs/Enemy")) as GameObject;
				Destroy (enemyspawn.GetComponent<EnemyController>());
				TipOfTheSpear minion = enemyspawn.AddComponent<TipOfTheSpear>() as TipOfTheSpear;
				minion.numberOfFollowers = numberOfFollowers;
				minion.SetSrcFileName("tipofthespear");
				minion.SetTrackID(trackID);
				minion.SetTrackLane(trackLane);
				if(i == 0)
					minion.OverrideMoverLane(15f);
				else if(i == 1)
					minion.OverrideMoverLane(-15f);
				
				partners.Add(minion);
				foreach(TipOfTheSpear tots in partners){
					tots.AddPartner(minion);
				}
				minion.StartMoving();
			}
			
			spawnsDone[1] = true;
			doneSpawning = true;
		}else{
			Debug.Log("we're trying to spawn even though we're done");
		}
	}
	public void AddPartner(TipOfTheSpear tots){
		partners.Add(tots);
	}
	public void FillPartners(List<TipOfTheSpear> list){
		foreach(TipOfTheSpear tots in list){
			partners.Add(tots);
		}
		//Debug.Log(followers.Count + " followers");
	}
	public override void Die(){
		if (hp <= 0.0f) {
			dialCon.IncreaseSuperPercent();
		}
		bool everyonesHere = partners.Count >= numberOfFollowers;
		bool playing = true;
		foreach(TipOfTheSpear tots in partners){
			if(!tots.IsPlayingDead()){
				playing = false;
				break;
			}
		}
		bool everyonesPlayingDead = everyonesHere && playing;
		
		if(everyonesPlayingDead){
			RealDie();
		}else{
			PlayDead();
		}
		
	}
	public void PlayDead(){
		playingDead = true;
		gameObject.GetComponent<SpriteRenderer>().enabled = false;
		transform.FindChild("Health").GetComponent<SpriteRenderer>().enabled = false;
	}
	public void RealDie(){
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
		//TODO:put yourself in the missedwave queue
		foreach(TipOfTheSpear tots in partners){
			Destroy (tots.gameObject);
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
