﻿using UnityEngine;
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

	bool onBreather = false;

	// Use this for initialization
	void Start () {
		ring = GameObject.Find ("OuterRing").gameObject.GetComponent<TrackController>();

		waves = new List<Wave> ();

		//do a ton of JSON parsing
		FileLoader leveldata = new FileLoader ("JSONData" + Path.DirectorySeparatorChar + "Levels", "testlevel");
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
	}
	
	// Update is called once per frame
	void Update () {
		if (onBreather) {
			//Debug.Log("on breather");
			if(timer.TimeElapsedMillis() > 8000){
				onBreather = false;
				activeWaveIndex++;
				if(activeWaveIndex < waves.Count){
					activeWave = waves[activeWaveIndex];
				}
				timer.Restart();
			}
			return;
		}

		List<GameObject> spawnedThisCycle = new List<GameObject> ();
		foreach (GameObject enemy in activeWave.GetEnemies()) {
			EnemyController e = enemy.GetComponent<EnemyController>();
			if(e.GetSpawnTime() - ring.GetHeadStartOfTrack(e.GetTrackID()) < timer.TimeElapsedMillis()
							   && !e.HasWarned()){
				e.Warn();
				GameEvent warning = new GameEvent("warning");
				warning.addArgument(e);
				EventManager.Instance().RaiseEvent(warning);
			}
			if(e.GetSpawnTime() < timer.TimeElapsedMillis()){
				spawnedThisCycle.Add(enemy);
				enemy.SetActive(true);
				e.StartMoving();
			}
		}
		foreach (GameObject spawned in spawnedThisCycle) {
			activeWave.RemoveEnemy(spawned);
		}

		if (activeWave.IsEverythingDead ()) {
			timer.Restart();
			onBreather = true;
		}
	}
}
