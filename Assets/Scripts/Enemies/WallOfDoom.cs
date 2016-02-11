using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class WallOfDoom : Enemy{
	
	List<WallOfDoom> partners = new List<WallOfDoom>();
	bool playingDead = false;
	
	public bool groupAddedToBonus = false;
	
	public override void Start(){
		base.Start();
		if(partners.Count < 3)
			SpawnPartners();
		else
			Debug.Log (partners.Count);
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
	public void SpawnPartners(){
		//Debug.Log ("spawning");
		GameObject enemyspawn1 = GameObject.Instantiate (Resources.Load ("Prefabs/MainCanvas/Enemy")) as GameObject;
		Destroy (enemyspawn1.GetComponent<Enemy>());
		WallOfDoom wall = enemyspawn1.AddComponent<WallOfDoom>() as WallOfDoom;
		enemyspawn1.transform.SetParent(Dial.spawnLayer,false);
		wall.groupAddedToBonus = groupAddedToBonus;
		GameObject enemyspawn2 = GameObject.Instantiate (Resources.Load ("Prefabs/MainCanvas/Enemy")) as GameObject;
		Destroy (enemyspawn2.GetComponent<Enemy>());
		WallOfDoom wall2 = enemyspawn2.AddComponent<WallOfDoom>() as WallOfDoom;
		enemyspawn2.transform.SetParent(Dial.spawnLayer,false);
		wall2.groupAddedToBonus = groupAddedToBonus;
		
		partners.Add(this);
		partners.Add(wall);
		partners.Add(wall2);
		wall.FillPartners(partners);
		wall2.FillPartners(partners);
		
		wall.SetSrcFileName(srcFileName);
		wall.SetTrackID(trackID);
		wall.SetTrackLane(-1);
		wall2.SetSrcFileName(srcFileName);
		wall2.SetTrackID(trackID);
		wall2.SetTrackLane(1);
		
		//wall.GetComponent<RectTransform>().anchoredPosition = rt.anchoredPosition;
		//wall2.GetComponent<RectTransform>().anchoredPosition = rt.anchoredPosition;
		//calculate and set positions
		/*float degrees1 = (trackID-1)*60; //clockwise of y-axis
		degrees1 += 15*wall.GetTrackLane(); //negative trackpos is left side, positive is right side, 0 is middle
		degrees1 = ((360-degrees1) + 90)%360; //convert to counterclockwise of x axis
		degrees1 *= Mathf.Deg2Rad;
		enemyspawn1.GetComponent<RectTransform>().anchoredPosition = new Vector2(Dial.ENEMY_SPAWN_LENGTH*Mathf.Cos(degrees1),Dial.ENEMY_SPAWN_LENGTH*Mathf.Sin(degrees1));
		
		float degrees2 = (trackID-1)*60; //clockwise of y-axis
		degrees2 += 15*wall2.GetTrackLane(); //negative trackpos is left side, positive is right side, 0 is middle
		degrees2 = ((360-degrees2) + 90)%360; //convert to counterclockwise of x axis
		degrees2 *= Mathf.Deg2Rad;
		enemyspawn2.GetComponent<RectTransform>().anchoredPosition = new Vector2(Dial.ENEMY_SPAWN_LENGTH*Mathf.Cos(degrees2),Dial.ENEMY_SPAWN_LENGTH*Mathf.Sin(degrees2));
		*/
		wall.SetPositionBasedOnAngle();
		wall2.SetPositionBasedOnAngle();
		wall.StartMoving();
		wall2.StartMoving();
	}
	public void FillPartners(List<WallOfDoom> list){
		foreach(WallOfDoom wod in list){
			partners.Add(wod);
		}
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
			foreach(WallOfDoom wod in partners){
				wod.groupAddedToBonus = true;
			}
		}
	}
	public override void Die(){
		dead = true;
		if (hp <= 0.0f) {
			dialCon.IncreaseSuperPercent();
		}
		bool playing = true;
		foreach(WallOfDoom wod in partners){
			if(!wod.IsPlayingDead() && this != wod){
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
		foreach(WallOfDoom wod in partners){
			if(wod != this)
				Destroy (wod.gameObject);
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
