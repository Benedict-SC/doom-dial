using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class TipOfTheSpear : Enemy{
	
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
	
	public bool groupAddedToBonus = false;
	
	public override void Start(){
		base.Start();
		partnerSpawn = new Timer();
		partnerSpawn.Restart();
	}
	public override void Update(){
		if (!moving){
			base.Update();
			return;
		}if(leader && !doneSpawning && partners.Count < numberOfFollowers && partnerSpawn.TimeElapsedSecs() >= delay){
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
				GameObject enemyspawn = GameObject.Instantiate (Resources.Load ("Prefabs/MainCanvas/Enemy")) as GameObject;
				Destroy (enemyspawn.GetComponent<Enemy>());
				TipOfTheSpear minion = enemyspawn.AddComponent<TipOfTheSpear>() as TipOfTheSpear;
				enemyspawn.transform.SetParent(Dial.spawnLayer,false);
				RectTransform ert = enemyspawn.GetComponent<RectTransform>();
				ert.anchoredPosition = new Vector2(0f,300f);
				minion.numberOfFollowers = numberOfFollowers;
				minion.SetSrcFileName("tipofthespear");
				minion.SetTrackID(trackID);
				minion.SetTrackLane(trackLane);
				minion.groupAddedToBonus = groupAddedToBonus;
				if(i == 0)
					minion.OverrideMoverLane(7.5f);
				else if(i == 1)
					minion.OverrideMoverLane(-7.5f);
				
				AddPartner(minion);
				minion.AddPartner(this);
				foreach(TipOfTheSpear tots in partners){
					tots.AddPartner(minion);
					minion.AddPartner(tots);
				}
				minion.StartMoving();
			}
			spawnsDone[0] = true;
			delay = batch2;
		}else if(!spawnsDone[1]){
			//spawn second set
			for(int i = 0; i < 2; i++){
				GameObject enemyspawn = GameObject.Instantiate (Resources.Load ("Prefabs/MainCanvas/Enemy")) as GameObject;
				Destroy (enemyspawn.GetComponent<Enemy>());
				TipOfTheSpear minion = enemyspawn.AddComponent<TipOfTheSpear>() as TipOfTheSpear;
				enemyspawn.transform.SetParent(Dial.spawnLayer,false);
				RectTransform ert = enemyspawn.GetComponent<RectTransform>();
				ert.anchoredPosition = new Vector2(0f,300f);
				minion.numberOfFollowers = numberOfFollowers;
				minion.SetSrcFileName("tipofthespear");
				minion.SetTrackID(trackID);
				minion.SetTrackLane(trackLane);
				minion.groupAddedToBonus = groupAddedToBonus;
				if(i == 0)
					minion.OverrideMoverLane(15f);
				else if(i == 1)
					minion.OverrideMoverLane(-15f);
				
				AddPartner(minion);
				minion.AddPartner(this);
				foreach(TipOfTheSpear tots in partners){
					tots.AddPartner(minion);
					minion.AddPartner(tots);
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
		if(!partners.Contains(tots))
			partners.Add(tots);
	}
	public void FillPartners(List<TipOfTheSpear> list){
		foreach(TipOfTheSpear tots in list){
			partners.Add(tots);
		}
		//Debug.Log(followers.Count + " followers");
	}
	public override void AddToBonus(List<System.Object> bonusList){
		if(!groupAddedToBonus){
			Dictionary<string,System.Object> enemyDict = new Dictionary<string,System.Object>();
			enemyDict.Add("enemyID",srcFileName);
			enemyDict.Add("trackID",(long)GetCurrentTrackID());
			if(!spawnedByBoss)
				bonusList.Add(enemyDict);
			
			//tell everyone else not to do the thing
			groupAddedToBonus = true;
			foreach(TipOfTheSpear tots in partners){
				tots.groupAddedToBonus = true;
			}
		}
	}
	public override void Die(){
		dead = true;
		if (hp <= 0.0f) {
			dialCon.IncreaseSuperPercent();
		}
		bool everyonesHere = partners.Count >= numberOfFollowers;
		bool playing = true;
		foreach(TipOfTheSpear tots in partners){
			if(tots != this && !tots.IsPlayingDead()){
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
		gameObject.GetComponent<Image>().enabled = false;
		transform.FindChild("Health").GetComponent<Image>().enabled = false;
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
