using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class DiversionMinion : Enemy{
	
	public int numberOfFollowers;
	public Diversion leader;
	List<DiversionMinion> followers = new List<DiversionMinion>();
	bool playingDead = false;
	
	public bool groupAddedToBonus = false;
	
	public override void Start(){
		base.Start();
		followers.Add(this);
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
		}
	}
	public void FillFollowers(List<DiversionMinion> list){
		foreach(DiversionMinion dm in list){
			followers.Add(dm);
		}
		//Debug.Log(followers.Count + " followers");
	}
	public void AddFollower(DiversionMinion dm){
		if(!followers.Contains(dm))
			followers.Add(dm);
	}
	public override void AddToBonus(List<System.Object> bonusList){
		if(!groupAddedToBonus){
			//Debug.Log ("adding a diversion (from minion)");
			Dictionary<string,System.Object> enemyDict = new Dictionary<string,System.Object>();
			enemyDict.Add("enemyID",leader.GetSrcFileName());
			enemyDict.Add("trackID",(long)leader.GetCurrentTrackID());
			if(!spawnedByBoss)
				bonusList.Add(enemyDict);
			
			//tell everyone else not to do the thing
			groupAddedToBonus = true;
			foreach(DiversionMinion dm in followers){
				dm.groupAddedToBonus = true;
			}
			leader.groupAddedToBonus = true;
		}
	}
	public override void Die(){
		if (hp <= 0.0f) {
			dialCon.IncreaseSuperPercent();
		}
		bool everyonesHere = followers.Count >= numberOfFollowers;
		bool playing = true;
		foreach(DiversionMinion dm in followers){
			if(!dm.IsPlayingDead() && this != dm){
				playing = false;
				break;
			}
		}
		//Debug.Log ("dminion: " + everyonesHere + " and " + playing);
		bool everyonesPlayingDead = everyonesHere && playing && leader.IsPlayingDead();
		
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
		//TODO:put leader in the missedwave queue
		foreach(DiversionMinion dm in followers){
			if(dm != this)
				Destroy (dm.gameObject);
		}
		Destroy(leader.gameObject);
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