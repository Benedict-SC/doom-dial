using System;
using System.Collections.Generic;
using UnityEngine;

public class Wave{

	float radius = 5.0f;

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

			//calculate and set position
			float degrees = (track-1)*60; //clockwise of y-axis
			degrees += 15*trackpos; //negative trackpos is left side, positive is right side, 0 is middle
			degrees = ((360-degrees) + 90)%360; //convert to counterclockwise of x axis
			degrees *= Mathf.Deg2Rad;
			enemyspawn.transform.position = new Vector3(radius*Mathf.Cos(degrees),radius*Mathf.Sin(degrees),0);

			//set spawn time
			enemyspawn.GetComponent<EnemyController>().SetSpawnTime(spawntimeInMillis);
			enemies.Add(enemyspawn);
		}
	}

	public List<GameObject> GetEnemies(){
		return enemies;
	}
	public void RemoveEnemy(GameObject spawned){
		enemies.Remove (spawned);
	}
}
