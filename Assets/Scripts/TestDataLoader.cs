using UnityEngine;
using MiniJSON;
using System.IO;
using System.Collections.Generic;

public class TestDataLoader : MonoBehaviour{
	public void Start(){
	
	}
	public void Update(){
	
	}
	public static void StaticLoadGameData(){
		FileLoader src = new FileLoader ("JSONData" + Path.DirectorySeparatorChar + "MiscData","inventory");
		FileLoader dest = new FileLoader (Application.persistentDataPath,"Inventory","inventory");
		dest.Write(src.Read());
		/*FileLoader dialsrc = new FileLoader ("JSONData" + Path.DirectorySeparatorChar + "DialConfigs","testdial");
		FileLoader dialdest = new FileLoader (Application.persistentDataPath,"Dials","testdial");
		string json = dialsrc.Read ();
		dialdest.Write(json);*/
		//Debug.Log (json);
		/*Dictionary<string,System.Object> data = (Dictionary<string,System.Object>)Json.Deserialize (json);
		
		List<System.Object> entries = data ["towers"] as List<System.Object>;
		for(int i = 0; i < 6; i++) {
			Dictionary<string,System.Object> entry = entries[i] as Dictionary<string,System.Object>;
			string towerfile = entry["filename"] as string;
			FileLoader towersrc = new FileLoader ("JSONData" + Path.DirectorySeparatorChar + "Towers",towerfile);
			FileLoader towerdest = new FileLoader (Application.persistentDataPath,"Towers",towerfile);
			towerdest.Write(towersrc.Read());
		}*/
		string[] dials = {"devdial","onedial","twodial","threedial","fourdial","fivedial","sixdial"};
		foreach(string dialname in dials){
			FileLoader dialsrc = new FileLoader ("JSONData" + Path.DirectorySeparatorChar + "DialConfigs",dialname);
			FileLoader dialdest = new FileLoader (Application.persistentDataPath,"Dials",dialname);
			string json = dialsrc.Read ();
			dialdest.Write(json);
			Dictionary<string,System.Object> data = (Dictionary<string,System.Object>)Json.Deserialize (json);
			
			if(dialname.Equals("devdial")){
				List<System.Object> entries = data ["towers"] as List<System.Object>;
				for(int i = 0; i < 6; i++) {
					Dictionary<string,System.Object> entry = entries[i] as Dictionary<string,System.Object>;
					string towerfile = entry["filename"] as string;
					FileLoader towersrc = new FileLoader ("JSONData" + Path.DirectorySeparatorChar + "Towers",towerfile);
					FileLoader towerdest = new FileLoader (Application.persistentDataPath,"Towers",towerfile);
					towerdest.Write(towersrc.Read());
				}
			}
		}
		//load bestiary data and totals
		FileLoader enemylist = new FileLoader ("JSONData" + Path.DirectorySeparatorChar + "Bestiary","ENEMY_LIST");
		FileLoader bestiarydata = new FileLoader(Application.persistentDataPath,"Bestiary","bestiary_logging");
		Dictionary<string,System.Object> bdict = new Dictionary<string,System.Object>();
		bdict.Add("totalHits",0);
		bdict.Add("totalHitBy",0);
		bdict.Add("totalDeaths",0);
		bdict.Add("totalKills",0);
		List<System.Object> enemies = new List<System.Object>();
		Dictionary<string,System.Object> enemylistdict = Json.Deserialize(enemylist.Read()) as Dictionary<string,System.Object>;
		List<System.Object> enemyfilenames = enemylistdict["enemies"] as List<System.Object>;
		foreach (System.Object eobj in enemyfilenames){
			string efilename = (string)eobj;
			Dictionary<string,System.Object> subObject = new Dictionary<string,System.Object>();
			subObject.Add("name",efilename);
			subObject.Add("timesSeen",0);
			subObject.Add("timesHit",0);
			subObject.Add("timesKilled",0);
			subObject.Add("timesHitBy",0);
			subObject.Add("timesKilledBy",0);
			enemies.Add(subObject);
		}
		bdict.Add("enemyLogs",enemies);
		bestiarydata.Write(Json.Serialize(bdict));

		
		Debug.Log("Loaded");
	}
	public void LoadGameData(){
		StaticLoadGameData();
	}
	public void SetDial(string dial){
		WorldData.dialSelected = dial;
	}
	public void StartGame(){
		Application.LoadLevel("WaveEditorAdmin");
	}
    public void StartSingleTowerGame() {
        Application.LoadLevel("MainMenu");
    }
}
