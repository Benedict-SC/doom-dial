using UnityEngine;
using System.Collections.Generic;
using System.IO;
using MiniJSON;

public class WaveManager2 : MonoBehaviour {
	
	Timer levelProgress;
	Timer waveProgress;
	
	Dial dial;
	
	List<Wave> waves;
	Wave activeWave;
	int activeWaveIndex;
	
	public static readonly int BREATHER_SECONDS = 6;
	bool onBreather = true;
	
	int bosscode = 0;

    public int enemycount = 0;
    public int guysKilled = 0;
	
	static bool bonusWaveIsHappening = false;
	
	public void Start(){
		dial = GameObject.Find("Dial").GetComponent<Dial>();
	
		waves = new List<Wave> ();
		levelProgress = new Timer();
		waveProgress = new Timer();
		
		//do a ton of JSON parsing
		FileLoader leveldata = new FileLoader ("JSONData" + Path.DirectorySeparatorChar + "Campaign" + Path.DirectorySeparatorChar + "Levels", WorldData.levelSelected);
		Debug.Log (leveldata.CreatedPath());
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
			string path = "JSONData" + Path.DirectorySeparatorChar + "Campaign" + Path.DirectorySeparatorChar + "Waves";
			FileLoader file = new FileLoader (path, filename);
			if(loadingUserLevel){
				file = new FileLoader(Application.persistentDataPath,"UserLevels",filename);
			}
			Dictionary<string,System.Object> raw = Json.Deserialize (file.Read()) as Dictionary<string,System.Object>;
			
			waves.Add(new Wave(raw));
		}
        foreach(Wave w in waves) {
            enemycount += w.GetEnemies().Count;
        }
		
		if (waves.Count <= 0) { //check to make sure reading worked
			Debug.Log("JSON parsing failed!");
			return; //SHOULD NOT HAPPEN
		}
		
		activeWaveIndex = -1;
		WaveMessageBox.StandardWarning(1);
        KillCountBox.KillDisplay(guysKilled, enemycount, true);
		
		
		if(bosscode == 2){
			GameObject boss = GameObject.Instantiate (Resources.Load ("Prefabs/MainCanvas/Megaboid")) as GameObject;
			boss.transform.SetParent(Dial.unmaskedLayer,false);
		}else if(bosscode == 3){
			GameObject boss = GameObject.Instantiate (Resources.Load ("Prefabs/MainCanvas/BigBulk")) as GameObject;
			boss.transform.SetParent(Dial.unmaskedLayer,false);
		}
	}
	public void Update(){
		if(Pause.paused)
			return;
        
		if(bonusWaveIsHappening){
			BonusWaveUpdate();
			return;
		}
        if (!onBreather){
			//Debug.Log("not on breather");
			//List<GameObject> spawnedThisCycle = new List<GameObject> ();
			foreach (GameObject enemy in activeWave.GetEnemies()) {
                if (enemy == null)
                    continue;
				Enemy e = enemy.GetComponent<Enemy> ();
				if (e.GetSpawnTime () - ZoneWarning.HEAD_START*1000 < waveProgress.TimeElapsedMillis()
				    && !e.HasWarned ()) {
					e.Warn();
					GameEvent warning = new GameEvent ("warning");
					warning.addArgument (e);
					EventManager.Instance ().RaiseEvent (warning);
				}
				if (e.GetSpawnTime () < waveProgress.TimeElapsedMillis() && !e.spawned) {
					//spawnedThisCycle.Add (enemy);
					enemy.SetActive (true);
					e.StartMoving ();
					EnemyIndexManager.LogEnemyAppearance(e.GetSrcFileName());
				}
			}
			//foreach (GameObject spawned in spawnedThisCycle) {
			//	activeWave.RemoveEnemy (spawned);
			//}
			if (activeWave.IsEverythingDead ()) {
				waveProgress.Restart ();
                KillCountBox.KillDisplay(guysKilled, enemycount, false);
                onBreather = true;
				if(activeWaveIndex + 2 <= waves.Count)
					WaveMessageBox.StandardWarning(activeWaveIndex + 2);
            }
		}else{
			//Debug.Log("on breather");
			if(waveProgress.TimeElapsedSecs() > BREATHER_SECONDS){
				onBreather = false;
				activeWaveIndex++;
				if (activeWaveIndex < waves.Count) {
					activeWave = waves [activeWaveIndex];
				}else{
					//Debug.Log(activeWaveIndex + " >= " + waves.Count);
					BonusWaveStart();
				}
				waveProgress.Restart();
				if(activeWaveIndex == 5){
					//spawn late-spawning bosses
					if(bosscode == 1){
						GameObject boss = GameObject.Instantiate (Resources.Load ("Prefabs/MainCanvas/SwarmMaster")) as GameObject;
						boss.transform.SetParent(Dial.unmaskedLayer,false);
					}
					if(bosscode == 4){
						GameObject boss = GameObject.Instantiate (Resources.Load ("Prefabs/MainCanvas/Skizzard")) as GameObject;
						boss.transform.SetParent(Dial.unmaskedLayer,false);
					}
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
	
	Wave bonusWave;
	Timer bonusTimer;
	bool bonusGoing = false;
	Timer delayTimer;
    int bonusWaveCode = -1;
	void BonusWaveStart(){
        if(dial.escapedEnemyCount == 0) {
            GameEvent winevent = new GameEvent("you_won");
            EventManager.Instance().RaiseEvent(winevent);
            return;
        }
		Dictionary<string,System.Object> json = dial.GetBonusJSON();
		bonusWave = new Wave(json);
		dial.ClearBonusJSON();
		bonusTimer = new Timer();
		delayTimer = new Timer();
		bonusWaveIsHappening = true;
        bonusGoing = false;
        delayTimer.Restart();
		WaveMessageBox.StandardWarning(bonusWaveCode);
        bonusWaveCode--;
	}
	void BonusWaveUpdate(){
		if(!bonusGoing){
			if(delayTimer.TimeElapsedSecs() >= 4f){
				bonusGoing = true;
				bonusTimer.Restart();
			}
		}else{
			foreach (GameObject enemy in bonusWave.GetEnemies()) {
                if (enemy == null)
                    continue;
                Enemy e = enemy.GetComponent<Enemy> ();
				if (e.GetSpawnTime () - ZoneWarning.HEAD_START*1000 < bonusTimer.TimeElapsedMillis()
				    && !e.HasWarned ()) {
					e.Warn();
					GameEvent warning = new GameEvent ("warning");
					warning.addArgument (e);
					EventManager.Instance ().RaiseEvent (warning);
				}
				if (e.GetSpawnTime () < bonusTimer.TimeElapsedMillis() && !e.spawned) {
					enemy.SetActive (true);
					e.StartMoving ();
					EnemyIndexManager.LogEnemyAppearance(e.GetSrcFileName());
					//handle junior
					if(e is Junior){
						int bonusWaveNumber = 1; //change this later when we have multiple bonus waves
						float extraShielding = bonusWaveNumber * Junior.extraShieldPerWave;
						e.GetShield().SetAllShieldHP(e.GetShield().GetBaseHP() + extraShielding);
					}
				}
			}
            //check if the wave's over
            if (bonusWave.IsEverythingDead()) {
                waveProgress.Restart();
                onBreather = true;
                BonusWaveStart();
            }
        }
	}
}
