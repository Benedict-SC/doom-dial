using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class Melder : Enemy{
	
	List<Melder> partners = new List<Melder>();
	bool playingDead = false;
	
	public bool groupAddedToBonus = false;
	
	public bool someoneReached50 = false;
	public bool someoneMelded = false;
	public Vector2 centerPos = new Vector2(0f,0f);
	public float centerProg = 0f;
	public Vector2 initPos = new Vector2(0f,0f);
	float meldTime = 1.5f;
	public Timer meldTimer = new Timer();
	public RectTransform mrt;
	
	public override void Start(){
		base.Start();
		mrt = (RectTransform)transform;
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
			//do melder checks
			if(!someoneReached50){
				if(progress >= .5){
					Vector2 posTotals = new Vector2(0f,0f);
					int numberAlive = 0;
					float progTotals = 0f;
					foreach(Melder m in partners){//begin melding
						m.someoneReached50 = true;
						
						if(!m.IsPlayingDead()){
							posTotals += m.mrt.anchoredPosition;
							progTotals += m.GetProgress();
							numberAlive++;
						}
					}
					foreach(Melder m in partners){
						m.centerPos = new Vector2(posTotals.x/(float)numberAlive,posTotals.y/(float)numberAlive);
						m.initPos = m.mrt.anchoredPosition;
						m.centerProg = progTotals / (float)numberAlive;
						m.meldTimer.Restart();
					}
				}else{
					//just keep going
				}
			}else{//we're melding
				if(meldTimer.TimeElapsedSecs() > meldTime){
					if(!someoneMelded){
						foreach(Melder m in partners){
							m.someoneMelded = true;
						}
						Meld();
					}
				}else{
					float meldProg = meldTimer.TimeElapsedSecs()/meldTime;
					//move to where you should be in your journey towards the center
					mrt.anchoredPosition = new Vector2(initPos.x + meldProg*(centerPos.x-initPos.x),initPos.y + meldProg*(centerPos.y-initPos.y));
				}
			}
			
		}
	}
	public void SpawnPartners(){
		//Debug.Log ("spawning");
		GameObject enemyspawn1 = GameObject.Instantiate (Resources.Load ("Prefabs/MainCanvas/Enemy")) as GameObject;
		Destroy (enemyspawn1.GetComponent<Enemy>());
		Melder wall = enemyspawn1.AddComponent<Melder>() as Melder;
		enemyspawn1.transform.SetParent(Dial.spawnLayer,false);
		wall.groupAddedToBonus = groupAddedToBonus;
		GameObject enemyspawn2 = GameObject.Instantiate (Resources.Load ("Prefabs/MainCanvas/Enemy")) as GameObject;
		Destroy (enemyspawn2.GetComponent<Enemy>());
		Melder wall2 = enemyspawn2.AddComponent<Melder>() as Melder;
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
		
		//calculate and set positions
		/*float degrees1 = (trackID-1)*60; //clockwise of y-axis
		degrees1 += 15*wall.GetTrackLane(); //negative trackpos is left side, positive is right side, 0 is middle
		degrees1 = ((360-degrees1) + 90)%360; //convert to counterclockwise of x axis
		degrees1 *= Mathf.Deg2Rad;
		enemyspawn1.transform.position = new Vector3(Dial.ENEMY_SPAWN_LENGTH*Mathf.Cos(degrees1),Dial.ENEMY_SPAWN_LENGTH*Mathf.Sin(degrees1),0);
		
		float degrees2 = (trackID-1)*60; //clockwise of y-axis
		degrees2 += 15*wall2.GetTrackLane(); //negative trackpos is left side, positive is right side, 0 is middle
		degrees2 = ((360-degrees2) + 90)%360; //convert to counterclockwise of x axis
		degrees2 *= Mathf.Deg2Rad;
		enemyspawn2.transform.position = new Vector3(Dial.ENEMY_SPAWN_LENGTH*Mathf.Cos(degrees2),Dial.ENEMY_SPAWN_LENGTH*Mathf.Sin(degrees2),0);
		*/
		wall.SetPositionBasedOnAngle();
		wall2.SetPositionBasedOnAngle();
		wall.StartMoving();
		wall2.StartMoving();
	}
	public void FillPartners(List<Melder> list){
		foreach(Melder wod in list){
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
			foreach(Melder wod in partners){
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
		foreach(Melder wod in partners){
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
		transform.Find("Health").GetComponent<Image>().enabled = false;
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
		foreach(Melder wod in partners){
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
	public void Meld(){
		//spawn super unit
		GameObject enemyspawn = GameObject.Instantiate (Resources.Load ("Prefabs/MainCanvas/Enemy")) as GameObject;
		Destroy (enemyspawn.GetComponent<Enemy>());
		Melded mini = enemyspawn.AddComponent<Melded>() as Melded;
		enemyspawn.transform.SetParent(Dial.spawnLayer,false);		
		
		mini.SetSrcFileName("melded");
		mini.SetTrackID(trackID);
		mini.SetTrackLane(trackLane);
		
		//calculate and set position
		/*float degrees = (trackID-1)*60; //clockwise of y-axis
		degrees += 15*trackLane; //negative trackpos is left side, positive is right side, 0 is middle
		degrees = ((360-degrees) + 90)%360; //convert to counterclockwise of x axis
		degrees *= Mathf.Deg2Rad;
		enemyspawn.transform.position = new Vector3(Dial.ENEMY_SPAWN_LENGTH*Mathf.Cos(degrees),Dial.ENEMY_SPAWN_LENGTH*Mathf.Sin(degrees),0);
		*/
		//give super unit correct progress
		mini.SetPositionBasedOnProgress(centerProg);
		mini.StartMoving();
		
		//damage super unit according to thing
		float damage = 0;
		foreach(Melder m in partners){
			damage += (m.GetMaxHP() - m.GetHP());
		}
		mini.TakeDamage(damage);
		
		
		//kill everyone
		foreach(Melder m in partners){
			if(m != this){
				Destroy(m.gameObject);
			}
		}
		Destroy (this.gameObject);
		
	}
}
