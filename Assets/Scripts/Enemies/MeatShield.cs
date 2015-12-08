using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class MeatShield : Enemy{
	
	int numberOfFollowers = 8;
	float delay = .8f;
	public bool leader = false;
	Timer partnerSpawn;
	bool[] spawnsDone = {false,false,false,false,false};
	List<MeatShield> partners = new List<MeatShield>();
	bool playingDead = false;
	
	public bool groupAddedToBonus = false;
	
	bool doneSpawning = false;
	float batch1;
	float batch2;
	float batch3;
	float batch4;
	float batch5;
	
	public override void Start(){
		base.Start();
		partnerSpawn = new Timer();
		partnerSpawn.Restart();
		AddPartner(this);
	}
	public override void Update(){
		if (!moving){
			base.Update();
			return;
		}
		if(leader && !doneSpawning && partners.Count < numberOfFollowers && partnerSpawn.TimeElapsedSecs() >= delay){
			SpawnBatch();
		}
		if(!playingDead)
			base.Update();
	}
	public void SetDelay(float f){
		delay = f;
		batch1 = f;
		batch2 = 1.75f*f;
		batch3 = 2.5f*f;
		batch4 = 3.25f*f;
		batch5 = 6f*f;
	}
	public void SpawnBatch(){
		if(!spawnsDone[0]){
			//spawn first set
			for(int i = 0; i < 2; i++){
				GameObject enemyspawn = GameObject.Instantiate (Resources.Load ("Prefabs/MainCanvas/Enemy")) as GameObject;
				Destroy (enemyspawn.GetComponent<Enemy>());
				MeatShield minion = enemyspawn.AddComponent<MeatShield>() as MeatShield;
				enemyspawn.transform.SetParent(Dial.spawnLayer,false);
				RectTransform ert = enemyspawn.GetComponent<RectTransform>();
				ert.anchoredPosition = new Vector2(0f,300f);
				minion.numberOfFollowers = numberOfFollowers;
				minion.SetSrcFileName("meatshieldsmall");
				minion.SetTrackID(trackID);
				minion.SetTrackLane(trackLane);
				minion.groupAddedToBonus = groupAddedToBonus;
				if(i == 0)
					minion.OverrideMoverLane(6f);
				else if(i == 1)
					minion.OverrideMoverLane(-6f);
				
				AddPartner(minion);
				foreach(MeatShield ms in partners){
					ms.AddPartner(minion);
					minion.AddPartner(ms);
				}
				minion.StartMoving();
			}
			spawnsDone[0] = true;
			delay = batch2;
		}else if(!spawnsDone[1]){
			//spawn first set
			for(int i = 0; i < 1; i++){
				GameObject enemyspawn = GameObject.Instantiate (Resources.Load ("Prefabs/MainCanvas/Enemy")) as GameObject;
				Destroy (enemyspawn.GetComponent<Enemy>());
				MeatShield minion = enemyspawn.AddComponent<MeatShield>() as MeatShield;
				enemyspawn.transform.SetParent(Dial.spawnLayer,false);
				RectTransform ert = enemyspawn.GetComponent<RectTransform>();
				ert.anchoredPosition = new Vector2(0f,300f);
				minion.numberOfFollowers = numberOfFollowers;
				minion.SetSrcFileName("meatshieldsmall");
				minion.SetTrackID(trackID);
				minion.SetTrackLane(trackLane);
				minion.groupAddedToBonus = groupAddedToBonus;
				if(i == 0)
					minion.OverrideMoverLane(0f);
				
				AddPartner(minion);
				foreach(MeatShield ms in partners){
					ms.AddPartner(minion);
					minion.AddPartner(ms);
				}
				minion.StartMoving();
			}
			spawnsDone[1] = true;
			delay = batch3;
		}else if(!spawnsDone[2]){
			//spawn first set
			for(int i = 0; i < 2; i++){
				GameObject enemyspawn = GameObject.Instantiate (Resources.Load ("Prefabs/MainCanvas/Enemy")) as GameObject;
				Destroy (enemyspawn.GetComponent<Enemy>());
				MeatShield minion = enemyspawn.AddComponent<MeatShield>() as MeatShield;
				enemyspawn.transform.SetParent(Dial.spawnLayer,false);
				RectTransform ert = enemyspawn.GetComponent<RectTransform>();
				ert.anchoredPosition = new Vector2(0f,300f);
				minion.numberOfFollowers = numberOfFollowers;
				minion.SetSrcFileName("meatshieldsmall");
				minion.SetTrackID(trackID);
				minion.SetTrackLane(trackLane);
				minion.groupAddedToBonus = groupAddedToBonus;
				if(i == 0)
					minion.OverrideMoverLane(6f);
				else if(i == 1)
					minion.OverrideMoverLane(-6f);
				
				AddPartner(minion);
				foreach(MeatShield ms in partners){
					ms.AddPartner(minion);
					minion.AddPartner(ms);
				}
				minion.StartMoving();
			}
			spawnsDone[2] = true;
			delay = batch4;
		}else if(!spawnsDone[3]){
			//spawn first set
			for(int i = 0; i < 1; i++){
				GameObject enemyspawn = GameObject.Instantiate (Resources.Load ("Prefabs/MainCanvas/Enemy")) as GameObject;
				Destroy (enemyspawn.GetComponent<Enemy>());
				MeatShield minion = enemyspawn.AddComponent<MeatShield>() as MeatShield;
				enemyspawn.transform.SetParent(Dial.spawnLayer,false);
				RectTransform ert = enemyspawn.GetComponent<RectTransform>();
				ert.anchoredPosition = new Vector2(0f,300f);
				minion.numberOfFollowers = numberOfFollowers;
				minion.SetSrcFileName("meatshieldsmall");
				minion.SetTrackID(trackID);
				minion.SetTrackLane(trackLane);
				minion.groupAddedToBonus = groupAddedToBonus;
				if(i == 0)
					minion.OverrideMoverLane(0);
				
				AddPartner(minion);
				foreach(MeatShield ms in partners){
					ms.AddPartner(minion);
					minion.AddPartner(ms);
				}
				minion.StartMoving();
			}
			spawnsDone[3] = true;
			delay = batch5;
		}else if(!spawnsDone[4]){
			//spawn second set
			for(int i = 0; i < 1; i++){
				GameObject enemyspawn = GameObject.Instantiate (Resources.Load ("Prefabs/MainCanvas/Enemy")) as GameObject;
				Destroy (enemyspawn.GetComponent<Enemy>());
				MeatShield minion = enemyspawn.AddComponent<MeatShield>() as MeatShield;
				enemyspawn.transform.SetParent(Dial.spawnLayer,false);
				RectTransform ert = enemyspawn.GetComponent<RectTransform>();
				ert.anchoredPosition = new Vector2(0f,300f);
				minion.numberOfFollowers = numberOfFollowers;
				minion.SetSrcFileName("meatshieldbig");
				minion.SetTrackID(trackID);
				minion.SetTrackLane(trackLane);
				minion.groupAddedToBonus = groupAddedToBonus;
				if(i == 0)
					minion.OverrideMoverLane(0f);
				
				AddPartner(minion);
				foreach(MeatShield ms in partners){
					ms.AddPartner(minion);
					minion.AddPartner(ms);
				}
				minion.StartMoving();
			}
			
			spawnsDone[4] = true;
			doneSpawning = true;
		}else{
			Debug.Log("we're trying to spawn even though we're done");
		}
	}
	public void AddPartner(MeatShield ms){
		if(!partners.Contains(ms))
			partners.Add(ms);
	}
	public void FillPartners(List<MeatShield> list){
		foreach(MeatShield ms in list){
			AddPartner (ms);
		}
		//Debug.Log(followers.Count + " followers");
	}
	public override void AddToBonus(List<System.Object> bonusList){
		if(!groupAddedToBonus){
			//Debug.Log ("adding a big one");
			Dictionary<string,System.Object> enemyDict = new Dictionary<string,System.Object>();
			enemyDict.Add("enemyID","meatshieldsmall");
			enemyDict.Add("trackID",(long)GetCurrentTrackID());
			bonusList.Add(enemyDict);
			
			//tell everyone else not to do the thing
			groupAddedToBonus = true;
			foreach(MeatShield ms in partners){
				ms.groupAddedToBonus = true;
			}
		}
	}
	public override void Die(){
		if (hp <= 0.0f) {
			dialCon.IncreaseSuperPercent();
			Debug.Log ("increasing super percent");
		}
		bool everyonesHere = partners.Count >= numberOfFollowers;
		//Debug.Log("partner count: "+partners.Count);
		bool playing = true;
		foreach(MeatShield ms in partners){
			if(ms != this && !ms.IsPlayingDead()){
				playing = false;
				break;
			}
		}
		//Debug.Log ("big one: "+everyonesHere + " and " + playing);
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
		foreach(MeatShield ms in partners){
			if(ms != this)
				Destroy (ms.gameObject);
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
