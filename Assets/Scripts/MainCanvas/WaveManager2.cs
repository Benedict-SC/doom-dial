using UnityEngine;
using System.Collections.Generic;
using System.IO;
using MiniJSON;

public class WaveManager2 : MonoBehaviour {
	
	Timer levelProgress;
	Timer waveProgress;
	
	List<Wave> waves;
	Wave activeWave;
	int activeWaveIndex;
	
	bool onBreather = true;
	
	int bosscode = 0;
	
	public void Start(){
		waves = new List<Wave> ();
		levelProgress = new Timer();
		waveProgress = new Timer();
		
		//do a ton of JSON parsing
		FileLoader leveldata = new FileLoader ("JSONData" + Path.DirectorySeparatorChar + "Worlds" + Path.DirectorySeparatorChar + worldVar + Path.DirectorySeparatorChar + levelVar, "wavedata");
		bool loadingUserLevel = false;
		if(WorldData.loadUserLevel){
			leveldata = new FileLoader (Application.persistentDataPath,"UserLevels","userlevel");
			//reset world data
			loadingUserLevel = true;
			WorldData.loadUserLevel = false;
		}
		
		Dictionary<string,System.Object> levelraw = Json.Deserialize (leveldata.Read ()) as Dictionary<string,System.Object>;
		if(levelraw.ContainsKey("boss"))
			bosscode = (int)(long)levelraw["boss"];
		List<System.Object> wavesdata = levelraw ["waves"] as List<System.Object>;
		foreach (System.Object thing in wavesdata) {
			Dictionary<string,System.Object> dict = thing as Dictionary<string,System.Object>;
			string filename = dict["wavename"] as string;
			string path = "JSONData" + Path.DirectorySeparatorChar + "Waves";
			FileLoader file = new FileLoader (path, filename);
			if(loadingUserLevel){
				file = new FileLoader(Application.persistentDataPath,"UserLevels",filename);
			}
			Dictionary<string,System.Object> raw = Json.Deserialize (file.Read()) as Dictionary<string,System.Object>;
			
			waves.Add(new Wave(raw));
		}
		
		if (waves.Count <= 0) { //check to make sure reading worked
			Debug.Log("JSON parsing failed!");
			return; //SHOULD NOT HAPPEN
		}
		
		activeWaveIndex = -1;
		
		/*
		if(bosscode == 2){
			GameObject boss = GameObject.Instantiate (Resources.Load ("Prefabs/Megaboid")) as GameObject;
		}else if(bosscode == 3){
			GameObject boss = GameObject.Instantiate (Resources.Load ("Prefabs/BigBulk")) as GameObject;
		}
		*/
	}
	public void Update(){
		if(Pause.paused)
			return;
		
		if(!onBreather){
			//Debug.Log("not on breather");
			List<GameObject> spawnedThisCycle = new List<GameObject> ();
			foreach (GameObject enemy in activeWave.GetEnemies()) {
				Enemy e = enemy.GetComponent<Enemy> ();
				if (e.GetSpawnTime () - ZoneWarning.HEAD_START*1000 < waveProgress.TimeElapsedMillis()
				    && !e.HasWarned ()) {
					e.Warn();
					GameEvent warning = new GameEvent ("warning");
					warning.addArgument (e);
					EventManager.Instance ().RaiseEvent (warning);
				}
				if (e.GetSpawnTime () < waveProgress.TimeElapsedMillis()) {
					spawnedThisCycle.Add (enemy);
					enemy.SetActive (true);
					e.StartMoving ();
				}
			}
			foreach (GameObject spawned in spawnedThisCycle) {
				activeWave.RemoveEnemy (spawned);
			}
			if (activeWave.IsEverythingDead ()) {
				waveProgress.Restart ();
				onBreather = true;
			}
		}else{
			//Debug.Log("on breather");
			if(waveProgress.TimeElapsedSecs() > 8){
				onBreather = false;
				activeWaveIndex++;
				if (activeWaveIndex < waves.Count) {
					activeWave = waves [activeWaveIndex];
				}else{
					Debug.Log(activeWaveIndex + " >= " + waves.Count);
				}
				waveProgress.Restart();
				if(activeWaveIndex == 5){
					//spawn late-spawning bosses
					/*if(bosscode == 1){
						GameObject boss = GameObject.Instantiate (Resources.Load ("Prefabs/SwarmMaster")) as GameObject;
					}*/
				}
			}
			return;
		}
		
	}
	
	string worldVar = "World1";
	string levelVar = "Level1";
	public void setLocations(string world, string level){
		worldVar = world;
		levelVar = level;
	}
}
