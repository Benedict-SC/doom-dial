using UnityEngine;
using System.Collections.Generic;
using System.IO;
//using System.Diagnostics; //Debug class name conflict in here
using MiniJSON;

public class WaveManager : MonoBehaviour {

	Timer timer;
	List<Wave> waves;
	Wave activeWave;
	int activeWaveIndex;
	TrackController ring;
	string worldVar = "World1";
	string levelVar = "Level1";
	bool onBreather = false;
	bool isPaused = false;
	long ellapsedTime = 0;
	long pauseTime = 0;
	int bosscode = 0;
	public List<GameObject> enemiesOnscreen;

	// Use this for initialization
	void Start () {
		ring = GameObject.Find ("OuterRing").gameObject.GetComponent<TrackController>();
		waves = new List<Wave> ();

		//do a ton of JSON parsing
		FileLoader leveldata = new FileLoader ("JSONData" + Path.DirectorySeparatorChar + "Worlds" + Path.DirectorySeparatorChar + worldVar + Path.DirectorySeparatorChar + levelVar, "wavedata");
		WorldData wd = GameObject.Find ("WorldData").GetComponent<WorldData>();
		bool loadingUserLevel = false;
		if(wd.loadUserLevel){
			leveldata = new FileLoader (Application.persistentDataPath,"UserLevels","userlevel");
			//reset world data
			loadingUserLevel = true;
			wd.loadUserLevel = false;
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

		activeWaveIndex = 0;
		activeWave = waves [activeWaveIndex];

		enemiesOnscreen = new List<GameObject>();

		timer = new Timer ();
		timer.Restart ();
		pauseTime = 0;
		
		if(bosscode == 2){
			GameObject boss = GameObject.Instantiate (Resources.Load ("Prefabs/Megaboid")) as GameObject;
		}else if(bosscode == 3){
			GameObject boss = GameObject.Instantiate (Resources.Load ("Prefabs/BigBulk")) as GameObject;
		}
	}
	
	// Update is called once per frame
	void Update () {

		ellapsedTime = timer.TimeElapsedMillis() + pauseTime;
		//stops any spawning from happening while paused
		isPaused = GamePause.paused;
		if (!isPaused) {
			pauseTime += timer.TimeElapsedMillis();
			if (onBreather) {
				//Debug.Log("on breather");
				if (ellapsedTime > 8000) {
					onBreather = false;
					activeWaveIndex++;
					if (activeWaveIndex < waves.Count) {
						activeWave = waves [activeWaveIndex];
					}
					timer.Restart ();
					pauseTime = 0;
					if(activeWaveIndex == 5){
						//spawn late-spawning bosses
						if(bosscode == 1){
							GameObject boss = GameObject.Instantiate (Resources.Load ("Prefabs/SwarmMaster")) as GameObject;
						}
					}
				}
				return;
			}

			List<GameObject> spawnedThisCycle = new List<GameObject> ();
			foreach (GameObject enemy in activeWave.GetEnemies()) {
				EnemyController e = enemy.GetComponent<EnemyController> ();
				if (e.GetSpawnTime () - ring.GetHeadStartOfTrack (e.GetTrackID ()) < ellapsedTime
					&& !e.HasWarned ()) {
					e.Warn ();
					GameEvent warning = new GameEvent ("warning");
					warning.addArgument (e);
					EventManager.Instance ().RaiseEvent (warning);
				}
				if (e.GetSpawnTime () < ellapsedTime) {
					spawnedThisCycle.Add (enemy);
					//Debug.Log ("should have added an enemy to enemiesOnscreen");
					enemy.SetActive (true);
					enemiesOnscreen.Add (enemy);
					//Debug.Log ("enemiesOnscreen size: " + enemiesOnscreen.Count);
					e.StartMoving ();
				}
			}
			foreach (GameObject spawned in spawnedThisCycle) {
				activeWave.RemoveEnemy (spawned);
			}

			if (activeWave.IsEverythingDead ()) {
				timer.Restart ();
				pauseTime = 0;
				onBreather = true;
			}
		}
	}
	public void setLocations(string world, string level){
		worldVar = world;
		levelVar = level;
	}
	/*public void triggerFreeze(){
		if (!isPaused) {
			//gets current time when paused, to keep enemy spawning from desyncing when unpaused
			pauseTime += timer.TimeElapsedMillis();
		} 
		//stop timer
		timer.PauseTrigger ();
		isPaused = !isPaused;
	
	}*/
}
