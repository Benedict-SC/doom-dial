using System;
using System.Collections.Generic;
using UnityEngine;

public class Wave{

	float radius = DialController.FULL_LENGTH;

	int levelID;
	int waveID;
	List<GameObject> enemies;

	public Wave (Dictionary<string,System.Object> json){
		enemies = new List<GameObject> ();
		//note: ints in MiniJSON come out as longs, so must be cast twice
		levelID = (int)(long)json ["levelID"];
		waveID = (int)(long)json ["waveID"];
		List<System.Object> enemyjson = (List<System.Object>)json ["enemies"];
		foreach (System.Object enemy in enemyjson) {
			Dictionary<string,System.Object> enemydict = (Dictionary<string,System.Object>)enemy;
			//would load from bestiary using (string)enemydict["enemyID"], but no bestiary yet
			long spawntimeInMillis = (long)enemydict["spawntime"];
			int track = (int)(long)enemydict["trackID"];
			int trackpos = (int)(long)enemydict["trackpos"];
			//make enemy
			GameObject enemyspawn = GameObject.Instantiate (Resources.Load ("Prefabs/Enemy")) as GameObject;
			enemyspawn.SetActive(false);
			EnemyController ec = enemyspawn.GetComponent<EnemyController>();

			//give enemy a filename to load from
			string filename = (string) enemydict["enemyID"];
			ec.SetSrcFileName(filename);
			ec.SetTrackID(track);
			ec.SetTrackLane(trackpos);

			//calculate and set position
			float degrees = (track-1)*60; //clockwise of y-axis
			degrees += 15*trackpos; //negative trackpos is left side, positive is right side, 0 is middle
			degrees = ((360-degrees) + 90)%360; //convert to counterclockwise of x axis
			degrees *= Mathf.Deg2Rad;

			enemyspawn.transform.position = new Vector3(radius*Mathf.Cos(degrees),radius*Mathf.Sin(degrees),0);

			//set spawn time
			ec.SetSpawnTime(spawntimeInMillis);
			enemies.Add(enemyspawn);

			ec.ConfigureEnemy ();
		}
	}

	public List<GameObject> GetEnemies(){
		return enemies;
	}
	public void RemoveEnemy(GameObject spawned){
		enemies.Remove (spawned);
	}
	public bool IsEverythingDead(){
		foreach(GameObject enemy in enemies){
			if(enemy != null)
				return false;
		}
		return true;
	}
}

