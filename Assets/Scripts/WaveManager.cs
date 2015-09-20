using UnityEngine;
using System.Collections.Generic;
using System.IO;
//using System.Diagnostics; //Debug class name conflict in here
using MiniJSON;

public class WaveManager : MonoBehaviour {

	public List<GameObject> enemies;

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
	// Use this for initialization
	void Start () {
		ring = GameObject.Find ("OuterRing").gameObject.GetComponent<TrackController>();
		waves = new List<Wave> ();
		enemies = new List<GameObject>();

		//do a ton of JSON parsing
		FileLoader leveldata = new FileLoader ("JSONData" + Path.DirectorySeparatorChar + "Worlds" + Path.DirectorySeparatorChar + worldVar + Path.DirectorySeparatorChar + levelVar, "wavedata");
		Dictionary<string,System.Object> levelraw = Json.Deserialize (leveldata.Read ()) as Dictionary<string,System.Object>;
		List<System.Object> wavesdata = levelraw ["waves"] as List<System.Object>;
		foreach (System.Object thing in wavesdata) {
			Dictionary<string,System.Object> dict = thing as Dictionary<string,System.Object>;
			string filename = dict["wavename"] as string;
			FileLoader file = new FileLoader ("JSONData" + Path.DirectorySeparatorChar + "Waves" , filename);
			Dictionary<string,System.Object> raw = Json.Deserialize (file.Read()) as Dictionary<string,System.Object>;

			waves.Add(new Wave(raw));
		}

		if (waves.Count <= 0) { //check to make sure reading worked
			Debug.Log("JSON parsing failed!");
			return; //SHOULD NOT HAPPEN
		}

		activeWaveIndex = 0;
		activeWave = waves [activeWaveIndex];

		timer = new Timer ();
		timer.Restart ();
		pauseTime = 0;
	}
	
	// Update is called once per frame
	void Update () {
		ellapsedTime = timer.TimeElapsedMillis() + pauseTime;
		if (!isPaused) {

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
					enemies.Add (enemy); //adds this enemy to the List of current enemies onscreen
					Debug.Log ("enemy list: " + enemies.Count);
					enemy.SetActive (true);
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
	public void triggerFreeze(){
		if (!isPaused) {
			pauseTime = timer.TimeElapsedMillis() + pauseTime;
		} 
		timer.PauseTrigger ();
		isPaused = !isPaused;
	}
}
