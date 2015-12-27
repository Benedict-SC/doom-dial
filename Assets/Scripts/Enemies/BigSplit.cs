using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class BigSplit : Enemy{
	bool playingDead = false;
	List<BigSplit> partners = null; //contains self
	public int size = 3;
	public bool justStarted = false;
	public bool groupAddedToBonus = false;
	
	float bounceDist;
	float traveled = 0f;
	bool bouncing;
	
	public bool imtheleftone = false;
	public bool talktome = false;
	
	public override void Start(){
		base.Start();
		if(partners == null){
			partners = new List<BigSplit>();
			partners.Add(this);
		}
	}
	public void InitializePartnersForSomeReason(){
		if(partners == null){
			partners = new List<BigSplit>();
			partners.Add(this);
		}
	}
	public override void Update(){
		if (!moving){
			base.Update();
			return;
		}
		if(playingDead){
			return;
		}
		base.Update();
		if(justStarted){
			SelfKnockback(0.05f);
			justStarted = false;
		}
		if(size > 2 && hp/maxhp < 0.5f){
			Split ();
		}else{
			//Debug.Log (size + " is size and hp is " + hp);
		}
		if(bouncing){
			float increment = bounceDist/7f;
			if(traveled/bounceDist < .5f){
				traveled += increment;
				mover.RightOffset(increment);
			}else if(traveled/bounceDist < .8f){
				traveled += increment/2f;
				mover.RightOffset(increment/2f);
			}else{
				traveled += increment/4f;
				mover.RightOffset(increment/4f);
			}
			if(Mathf.Abs(traveled) > Mathf.Abs(bounceDist)){
				float correction = traveled - bounceDist;
				mover.LeftOffset(correction);
				bouncing = false;
				traveled = 0;
			}
			
		}
	}
	public void Split(){
		//Debug.Log ("we should be splitting");
		GameObject enemyspawn1 = GameObject.Instantiate (Resources.Load ("Prefabs/MainCanvas/Enemy")) as GameObject;
		Destroy (enemyspawn1.GetComponent<Enemy>());
		BigSplit split = enemyspawn1.AddComponent<BigSplit>() as BigSplit;
		enemyspawn1.transform.SetParent(Dial.spawnLayer,false);
		GameObject enemyspawn2 = GameObject.Instantiate (Resources.Load ("Prefabs/MainCanvas/Enemy")) as GameObject;
		Destroy (enemyspawn2.GetComponent<Enemy>());
		BigSplit split2 = enemyspawn2.AddComponent<BigSplit>() as BigSplit;
		enemyspawn2.transform.SetParent(Dial.spawnLayer,false);
		
		split.groupAddedToBonus = groupAddedToBonus;
		split2.groupAddedToBonus = groupAddedToBonus;
		
		split.size = size - 1;
		split2.size = size - 1;
		if(size == 3){
			split.SetSrcFileName("bigsplit2");
			split2.SetSrcFileName("bigsplit2");
			split.SetTrackLane(0);
			split2.SetTrackLane(0);
		}
		split.InitializePartnersForSomeReason();
		split2.InitializePartnersForSomeReason();
		
		foreach(BigSplit s in partners){
			if(s == this)
				continue;
			s.AddPartner(split);
			s.AddPartner(split2);
			split.AddPartner(s);
			split2.AddPartner(s);
		}
		AddPartner(split);
		AddPartner(split2);
		split.AddPartner(split2);
		split2.AddPartner(split);
		
		split.SetProgress(progress);
		split2.SetProgress(progress);	
		split.SetTrackID(trackID);
		split2.SetTrackID(trackID);
		
		/*if(imtheleftone){
			split.talktome = true;
			split2.talktome = true;
		}*/
		
		split.StartMoving();
		split2.StartMoving();
		split.justStarted = true;
		split2.justStarted = true;
		split.Bounce (true);
		split2.Bounce (false);
		
		PlayDead();
	}
	public void AddPartner(BigSplit s){
		if(!partners.Contains(s))
			partners.Add (s);
	}
	public override void AddToBonus(List<System.Object> bonusList){
		if(!groupAddedToBonus){
			Dictionary<string,System.Object> enemyDict = new Dictionary<string,System.Object>();
			enemyDict.Add("enemyID","bigsplit");
			enemyDict.Add("trackID",(long)GetCurrentTrackID());
			bonusList.Add(enemyDict);
			
			//tell everyone else not to do the thing
			groupAddedToBonus = true;
			foreach(BigSplit s in partners){
				s.groupAddedToBonus = true;
			}
		}
	}
	public void Bounce(bool left){
		bouncing = true;
		if(size == 2){
			if(left){
				imtheleftone = true;
				bounceDist = 15f;
			}else {
				bounceDist = -15f;
			}
		}else if(size == 1){
			if(left){
				imtheleftone = true;
				bounceDist = 5f;
				
			}else {
				bounceDist = -5f;
			}
		}
	}
	
	public override void Die(){
		if (hp <= 0.0f) {
			dialCon.IncreaseSuperPercent();
		}
		bool playing = true;
		foreach(BigSplit s in partners){
			if(!s.IsPlayingDead() && this != s){
				playing = false;
				break;
			}
		}
		if(playing){
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
		
		foreach(BigSplit s in partners){
			if(s != this)
				Destroy (s.gameObject);
		}
		Destroy (this.gameObject);
	}
	public bool IsPlayingDead(){
		return playingDead;
	}
}

