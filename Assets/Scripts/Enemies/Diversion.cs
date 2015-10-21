
using UnityEngine;
using System.Collections.Generic;

public class Diversion : EnemyController{
	
	int numberOfFollowers = 10;
	public float delay = .8f;
	Timer followerSpawn;
	bool spawnedAlready = false;
	List<DiversionMinion> followers = new List<DiversionMinion>();
	bool playingDead = false;
	
	public override void Start(){
		base.Start();
		followerSpawn = new Timer();
		followerSpawn.Restart();
	}
	public override void Update(){
		if(playingDead){
			return;
		}else{
			base.Update();
			if(!spawnedAlready && followers.Count < numberOfFollowers && followerSpawn.TimeElapsedSecs() >= delay){
				spawnedAlready = true;
				SpawnFollower();
			}
		}
	}
	public void SpawnFollower(){
		GameObject enemyspawn = GameObject.Instantiate (Resources.Load ("Prefabs/Enemy")) as GameObject;
		Destroy (enemyspawn.GetComponent<EnemyController>());
		DiversionMinion newchainer = enemyspawn.AddComponent<DiversionMinion>() as DiversionMinion;
		newchainer.FillFollowers(followers);
		newchainer.delay = 0.8f;
		newchainer.numberOfFollowers = numberOfFollowers;
		newchainer.leader = this;
		followers.Add(newchainer);
		
		newchainer.SetSrcFileName("diversionminion");
		newchainer.SetTrackID(trackID);
		newchainer.SetTrackLane(trackLane);
		//calculate and set position
		float degrees = (trackID-1)*60; //clockwise of y-axis
		degrees += 15*trackLane; //negative trackpos is left side, positive is right side, 0 is middle
		degrees = ((360-degrees) + 90)%360; //convert to counterclockwise of x axis
		degrees *= Mathf.Deg2Rad;
		enemyspawn.transform.position = new Vector3(radius*Mathf.Cos(degrees),radius*Mathf.Sin(degrees),0);
		
		newchainer.StartMoving();
		
	}
	public void TellAboutNewFollower(DiversionMinion dm){
		followers.Add(dm);
	}
	public void FillFollowers(List<DiversionMinion> list){
		foreach(DiversionMinion dm in list){
			followers.Add(dm);
		}
		//Debug.Log(followers.Count + " followers");
	}
	public override void Die(){
		if (hp <= 0.0f) {
			dialCon.IncreaseSuperPercent();
		}
		bool everyonesHere = followers.Count >= numberOfFollowers;
		bool playing = true;
		foreach(DiversionMinion dm in followers){
			if(!dm.IsPlayingDead()){
				playing = false;
				break;
			}
		}
		bool everyonesPlayingDead = everyonesHere && playing;
		
		if(everyonesPlayingDead){
			RealDie();
		}else{
			if(!spawnedAlready){
				spawnedAlready = true;
				SpawnFollower();
			}
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