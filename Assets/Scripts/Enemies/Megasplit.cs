using UnityEngine;
using UnityEngine.UI;

public class Megasplit : Enemy{
	
	Timer countdown;
	float splitDelay = 5f;
	float[] spawnThetas = {-28.125f,-24.375f,-20.625f,-16.875f,-13.125f,-9.375f,-5.625f,-1.875f,
							1.875f,5.625f,9.375f,13.125f,16.875f,20.625f,24.375f,28.125f}; //in degrees
	float spawnRadius = 0f;
	
	public override void Start(){
		base.Start ();
		countdown = new Timer();
	}
	
	public override void Update(){
		base.Update();
		if(countdown.TimeElapsedSecs() >= splitDelay){
			//split up
			spawnRadius = Mathf.Sqrt((rt.anchoredPosition.x*rt.anchoredPosition.x)+(rt.anchoredPosition.y*rt.anchoredPosition.y));
			for(int i = 0; i < spawnThetas.Length; i++){
				SpawnMiniSplit(spawnThetas[i]);
			}
			//die
			canDropPiece = false;
			Die ();
		}
	}
	public void SpawnMiniSplit(float theta){
		GameObject enemyspawn = GameObject.Instantiate (Resources.Load ("Prefabs/MainCanvas/Enemy")) as GameObject;
		Destroy (enemyspawn.GetComponent<Enemy>());
		Minisplit mini = enemyspawn.AddComponent<Minisplit>() as Minisplit;
		enemyspawn.transform.SetParent(Dial.spawnLayer,false);		
		
		mini.SetSrcFileName("minisplit");
		mini.SetTrackID(trackID);
		mini.SetTrackLane(trackLane);
		mini.OverrideMoverLane(theta);
		
		//calculate and set position
		/*float degrees = (trackID-1)*60; //clockwise of y-axis
		degrees += 15*trackLane; //negative trackpos is left side, positive is right side, 0 is middle
		degrees = ((360-degrees) + 90)%360; //convert to counterclockwise of x axis
		degrees *= Mathf.Deg2Rad;
		enemyspawn.transform.position = new Vector3(Dial.ENEMY_SPAWN_LENGTH*Mathf.Cos(degrees),Dial.ENEMY_SPAWN_LENGTH*Mathf.Sin(degrees),0);
		
		mini.SetProgress(progress);*/
		mini.GetComponent<RectTransform>().anchoredPosition = rt.anchoredPosition;
		mini.StartMoving();
		
		mini.SetPolarTarget(theta,spawnRadius,this);
				
	}
	
}
