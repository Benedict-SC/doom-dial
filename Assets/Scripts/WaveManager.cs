using UnityEngine;
using System.Collections.Generic;
using System.Diagnostics; //Debug class name conflict in here
using MiniJSON;

public class WaveManager : MonoBehaviour {

	Stopwatch timer;
	Wave w;
	TrackController ring;

	// Use this for initialization
	void Start () {
		ring = GameObject.Find ("OuterRing").gameObject.GetComponent<TrackController>();
		FileLoader file = new FileLoader ("JSONData", "testwave2");
		Dictionary<string,System.Object> raw = Json.Deserialize (file.Read()) as Dictionary<string,System.Object>;
		//Dictionary<string,System.Object> wavedata = (Dictionary<string,System.Object>)raw ["wave"];
		w = new Wave (raw);
		timer = new Stopwatch ();
		timer.Reset ();
		timer.Start ();
	}
	
	// Update is called once per frame
	void Update () {
		List<GameObject> spawnedThisCycle = new List<GameObject> ();
		foreach (GameObject enemy in w.GetEnemies()) {
			EnemyController e = enemy.GetComponent<EnemyController>();
			if(e.GetSpawnTime() - ring.GetHeadStartOfTrack(e.GetTrackID()) < timer.ElapsedMilliseconds
							   && !e.HasWarned()){
				e.Warn();
				GameEvent warning = new GameEvent("warning");
				warning.addArgument(e);
				EventManager.Instance().RaiseEvent(warning);
			}
			if(e.GetSpawnTime() < timer.ElapsedMilliseconds){
				spawnedThisCycle.Add(enemy);
				enemy.SetActive(true);
				e.StartMoving();
			}
		}
		foreach (GameObject spawned in spawnedThisCycle) {
			w.RemoveEnemy(spawned);
		}
	}
}
