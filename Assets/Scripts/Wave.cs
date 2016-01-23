using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using MiniJSON;

public class Wave{

	Transform canvas;
	float radius = Dial.ENEMY_SPAWN_LENGTH;

	int levelID;
	int waveID;
	int maxTime;
	int interval;
	List<GameObject> enemies;

	public Wave (Dictionary<string,System.Object> json){
		canvas = GameObject.Find ("Canvas").transform;
		System.Random rand = new System.Random();
	
		enemies = new List<GameObject> ();
		//note: ints in MiniJSON come out as longs, so must be cast twice
		levelID = (int)(long)json ["levelID"];
		waveID = (int)(long)json ["waveID"];
		maxTime = (int)(long)json ["maxMilliseconds"];
		interval = (int)(long)json ["minimumInterval"];
		List<System.Object> enemyjson = (List<System.Object>)json ["enemies"];
		
		//step one: create a randomized list of spawn times
		int slots = maxTime / interval;
		//Debug.Log ("wave "+ waveID + " slots: " + slots);
		int occupants = enemyjson.Count;
		
			//create bool array to randomize
		List<bool> timeslots = new List<bool>();
		for(int i = 0; i < slots; i++){
			if(i < occupants){
				timeslots.Add(true);
			}else {
				timeslots.Add(false);
			}
		}
			//randomize this array (fisher-yates shuffle)
		for(int i = slots - 1; i > 0; i--){
			int j = rand.Next(i+1);
			bool temp = timeslots[i];
			timeslots[i] = timeslots[j];
			timeslots[j] = temp;
		}
			//create corresponding array of random-ish long bonuses to positions
		List<long> timeChaos = new List<long>();
		for(int i = 0; i < timeslots.Count; i++){
			timeChaos.Add(0);
		}
		for(int i = 0; i < timeChaos.Count; i++){
			if(timeslots[i]){ //if an enemy should spawn in this timeframe,
				//get the previous bonus to make sure you're the min distance away
				long previous = 0;
				if(i > 0){
					previous = timeChaos[i-1];
				}
				//create random spawn time within the time slot allotted
				long chaos = rand.Next((int)previous,(int)interval);
				timeChaos[i] = chaos;
			}
		}
			//check to make sure nothing went wrong and timeslots and timechaos are both of length slots
		if(timeslots.Count != slots){
			Debug.Log("timeslots is wrong length! slots is " + slots + " but timeslots length is " + timeslots.Count);
		}
		if(timeChaos.Count != slots){
			Debug.Log("timechaos is wrong length! slots is " + slots + " but timechaos length is " + timeChaos.Count);
		}
		
			//finally, create final list of spawn times
		List<long> spawntimesInMillis = new List<long>();
		for(int i = 0; i < timeslots.Count; i++){
			if(timeslots[i]){
				long spawntime = i * interval;
				spawntime += timeChaos[i];
				spawntimesInMillis.Add(spawntime);
			}
		}
		//for(int i = 0; i < spawntimesInMillis.Count; i++){
		//	Debug.Log ("wave "+ waveID + " spawntime "+i +": " + spawntimesInMillis[i]);
		//}
		//check to make sure nothing went wrong and spawntimes is of length occupants
		if(spawntimesInMillis.Count != enemyjson.Count){
			Debug.Log ("spawntimes and enemies don't match! (" + spawntimesInMillis.Count + "/" + enemyjson.Count + ")"); 
		}
		
		//shuffle the enemy order (fisher-yates)
		for(int i = enemyjson.Count - 1; i > 0; i--){
			int j = rand.Next(i+1);
			System.Object temp = enemyjson[i];
			enemyjson[i] = enemyjson[j];
			enemyjson[j] = temp;
		}
		
		for(int i = 0; i < enemyjson.Count; i++){
			System.Object enemy = enemyjson[i];
			Dictionary<string,System.Object> enemydict = (Dictionary<string,System.Object>)enemy;
			//would load from bestiary using (string)enemydict["enemyID"], but no bestiary yet
			//long spawntimeInMillis = (long)enemydict["spawntime"];
			string filename = (string) enemydict["enemyID"];
			int track = (int)(long)enemydict["trackID"];
			int trackpos = 0;
			if(enemydict.ContainsKey("trackpos"))
				trackpos = (int)(long)enemydict["trackpos"];
			//make enemy
			GameObject enemyspawn = GameObject.Instantiate (Resources.Load ("Prefabs/MainCanvas/Enemy")) as GameObject;
			Debug.Log("we're setting it to the spawn layer");
			//Debug.Log (Dial.spawnLayer == null);
			enemyspawn.transform.SetParent(Dial.spawnLayer,false);
			enemyspawn.SetActive(false);
			Enemy ec = enemyspawn.GetComponent<Enemy>();
			
			FileLoader fl = new FileLoader ("JSONData" + Path.DirectorySeparatorChar + "Bestiary",filename);
			string actualenemyjson = fl.Read ();
			Dictionary<string,System.Object> actualenemydict = Json.Deserialize(actualenemyjson) as Dictionary<string,System.Object>;
			string enemytype = (string)actualenemydict["enemyType"];
			if(enemytype.Equals("Chainers")){
				GameObject enemyobj = ec.gameObject;
				GameObject.Destroy(enemyobj.GetComponent<Enemy>());
				Chainer c = enemyobj.AddComponent<Chainer>() as Chainer;
				float chaindelay = (float)(double)actualenemydict["delay"];
				c.delay = chaindelay;
				ec = c;
			}else if(enemytype.Equals("TipOfTheSpear")){
				GameObject enemyobj = ec.gameObject;
				GameObject.Destroy(enemyobj.GetComponent<Enemy>());
				TipOfTheSpear tots = enemyobj.AddComponent<TipOfTheSpear>() as TipOfTheSpear;
				float chaindelay = (float)(double)actualenemydict["delay"];
				tots.SetDelay(chaindelay);
				tots.leader = true;
				ec = tots;
			}else if(enemytype.Equals("WallOfDoom")){
				GameObject enemyobj = ec.gameObject;
				GameObject.Destroy(enemyobj.GetComponent<Enemy>());
				WallOfDoom wod = enemyobj.AddComponent<WallOfDoom>() as WallOfDoom;
				ec = wod;
			}else if(enemytype.Equals("TheDiversion")){
				GameObject enemyobj = ec.gameObject;
				GameObject.Destroy(enemyobj.GetComponent<Enemy>());
				Diversion d = enemyobj.AddComponent<Diversion>() as Diversion;
				float chaindelay = (float)(double)actualenemydict["delay"];
				d.SetDelay(chaindelay);
				ec = d;
			}else if(enemytype.Equals("MeatShield")){
				GameObject enemyobj = ec.gameObject;
				GameObject.Destroy(enemyobj.GetComponent<Enemy>());
				MeatShield ms = enemyobj.AddComponent<MeatShield>() as MeatShield;
				float chaindelay = (float)(double)actualenemydict["delay"];
				ms.SetDelay(chaindelay);
				ms.leader = true;
				ec = ms;
			}else if(enemytype.Equals("Splitter")){
				GameObject enemyobj = ec.gameObject;
				GameObject.Destroy(enemyobj.GetComponent<Enemy>());
				Splitter s = enemyobj.AddComponent<Splitter>() as Splitter;
				ec = s;
			}else if(enemytype.Equals("Blob")){
				GameObject enemyobj = ec.gameObject;
				GameObject.Destroy(enemyobj.GetComponent<Enemy>());
				Blob b = enemyobj.AddComponent<Blob>() as Blob;
				ec = b;
			}else if(enemytype.Equals("Megasplit")){
				GameObject enemyobj = ec.gameObject;
				GameObject.Destroy(enemyobj.GetComponent<Enemy>());
				Megasplit ms = enemyobj.AddComponent<Megasplit>() as Megasplit;
				ec = ms;
			}else if(enemytype.Equals("Melder")){
				GameObject enemyobj = ec.gameObject;
				GameObject.Destroy(enemyobj.GetComponent<Enemy>());
				Melder m = enemyobj.AddComponent<Melder>() as Melder;
				ec = m;
			}else if(enemytype.Equals("BigSplit")){
				GameObject enemyobj = ec.gameObject;
				GameObject.Destroy(enemyobj.GetComponent<Enemy>());
				BigSplit bs = enemyobj.AddComponent<BigSplit>() as BigSplit;
				ec = bs;
			}else if(enemytype.Equals("Junior")){
				GameObject enemyobj = ec.gameObject;
				GameObject.Destroy(enemyobj.GetComponent<Enemy>());
				Junior j = enemyobj.AddComponent<Junior>() as Junior;
				ec = j;
			}else if(enemytype.Equals("Cheater")){
				GameObject enemyobj = ec.gameObject;
				GameObject.Destroy(enemyobj.GetComponent<Enemy>());
				Cheater ch = enemyobj.AddComponent<Cheater>() as Cheater;
				ec = ch;
			}else if (enemytype.Equals("Spite")){
                GameObject enemyobj = ec.gameObject;
                GameObject.Destroy(enemyobj.GetComponent<Enemy>());
                Spite s = enemyobj.AddComponent<Spite>() as Spite;
                ec = s;
            }else if (enemytype.Equals("Executor")){
                GameObject enemyobj = ec.gameObject;
                GameObject.Destroy(enemyobj.GetComponent<Enemy>());
                Executor s = enemyobj.AddComponent<Executor>() as Executor;
                ec = s;
            }else if (enemytype.Equals("Saboteur")){
                GameObject enemyobj = ec.gameObject;
                GameObject.Destroy(enemyobj.GetComponent<Enemy>());
                Saboteur s = enemyobj.AddComponent<Saboteur>() as Saboteur;
                ec = s;
            }else if (enemytype.Equals("Pusher")){
                GameObject enemyobj = ec.gameObject;
                GameObject.Destroy(enemyobj.GetComponent<Enemy>());
                Pusher s = enemyobj.AddComponent<Pusher>() as Pusher;
                ec = s;
            }

            //give enemy a filename to load from

            ec.SetSrcFileName(filename);
			ec.SetTrackID(track);
			ec.SetTrackLane(trackpos);
			
			//calculate and set position
			float degrees = (track-1)*60; //clockwise of y-axis
			degrees += 15*trackpos; //negative trackpos is left side, positive is right side, 0 is middle
			degrees = ((360-degrees) + 90)%360; //convert to counterclockwise of x axis
			degrees *= Mathf.Deg2Rad;
			
			
			((RectTransform)enemyspawn.transform).anchoredPosition = new Vector2(Dial.ENEMY_SPAWN_LENGTH*Mathf.Cos(degrees),Dial.ENEMY_SPAWN_LENGTH*Mathf.Sin(degrees));
			//set spawn time
			ec.SetSpawnTime(spawntimesInMillis[i]);
			
			enemies.Add(enemyspawn);
		}
		/*foreach (System.Object enemy in enemyjson) {
			//ec.ConfigureEnemy ();
		}*/
	}

	public List<GameObject> GetEnemies(){
		return enemies;
	}
	public void RemoveEnemy(GameObject spawned){
		enemies.Remove (spawned);
	}
	public bool IsEverythingDead(){
		foreach(GameObject enemy in enemies){
			/*try{
				bool b = enemy.activeSelf;
				return false;
			}catch(NullReferenceException nre){
				//well it's dead
			}*/
			if(enemy != null)
				return false;
		}
		return true;
	}
}

