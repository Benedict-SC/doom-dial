using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class Chainer : Enemy{
	
	int targetGroupSize = 5;
	public float delay = .8f;
	Timer followerSpawn;
	public bool spawnedAlready = false;
	List<Chainer> followers = new List<Chainer>();
	bool playingDead = false;
	
	public bool groupAddedToBonus = false;
	
	public override void Start(){
		base.Start();
		followerSpawn = new Timer();
		followerSpawn.Restart();
		AddFollower(this);
	}
	public override void Update(){
		if (!moving){
			base.Update();
			return;
		}
		if(playingDead){
			return;
		}else{
			base.Update();
			if(!spawnedAlready && followers.Count < targetGroupSize && followerSpawn.TimeElapsedSecs() >= delay){
				spawnedAlready = true;
				SpawnFollower();
			}else if(followers.Count == targetGroupSize){
				spawnedAlready = true;
			}
		}
	}
	public void SpawnFollower(){
		GameObject enemyspawn = GameObject.Instantiate (Resources.Load ("Prefabs/MainCanvas/Enemy")) as GameObject;
		Destroy (enemyspawn.GetComponent<Enemy>());
		Chainer newchainer = enemyspawn.AddComponent<Chainer>() as Chainer;
		enemyspawn.transform.SetParent(Dial.spawnLayer,false);
		followers.Add(newchainer);
		newchainer.FillFollowers(followers);
		newchainer.delay = delay;
		newchainer.groupAddedToBonus = groupAddedToBonus;
		newchainer.spawnedByBoss = spawnedByBoss;
		
		
		newchainer.SetSrcFileName(srcFileName);
		newchainer.SetTrackID(trackID);
		newchainer.SetTrackLane(trackLane);
		//calculate and set position
		float degrees = (trackID-1)*60; //clockwise of y-axis
		degrees += 15*trackLane; //negative trackpos is left side, positive is right side, 0 is middle
		degrees = ((360-degrees) + 90)%360; //convert to counterclockwise of x axis
		degrees *= Mathf.Deg2Rad;
		enemyspawn.transform.position = new Vector3(Dial.ENEMY_SPAWN_LENGTH*Mathf.Cos(degrees),Dial.ENEMY_SPAWN_LENGTH*Mathf.Sin(degrees),0);
		
		newchainer.StartMoving();
		
	}
	public void FillFollowers(List<Chainer> list){
		foreach(Chainer c in list){
			AddFollower(c);
			c.AddFollower(this);
		}
		//Debug.Log(followers.Count + " followers");
	}
	public void AddFollower(Chainer c){
		if(!HasFollower(c))
			followers.Add(c);
	}
	public bool HasFollower(Chainer c){
		return followers.Contains(c);
	}
	public override void AddToBonus(List<System.Object> bonusList){
		if(!groupAddedToBonus){
			//Debug.Log ("adding a chainer");
			Dictionary<string,System.Object> enemyDict = new Dictionary<string,System.Object>();
			enemyDict.Add("enemyID",srcFileName);
			enemyDict.Add("trackID",(long)GetCurrentTrackID());
			if(!spawnedByBoss)
				bonusList.Add(enemyDict);
			
			//tell everyone else not to do the thing
			groupAddedToBonus = true;
			foreach(Chainer c in followers){
				c.groupAddedToBonus = true;
			}
		}
	}
	public override void Die(){
		dead = true;
		if (hp <= 0.0f) {
			dialCon.IncreaseSuperPercent();
		}
			bool everyonesHere = followers.Count >= targetGroupSize;
			bool playing = true;
			foreach(Chainer c in followers){
				if(!c.IsPlayingDead() && this != c){
					playing = false;
					break;
				}
			}
			//Debug.Log ("chainers: " + everyonesHere + " and " + playing);
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
		foreach(Chainer c in followers){
			if(c != this)
				Destroy (c.gameObject);
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