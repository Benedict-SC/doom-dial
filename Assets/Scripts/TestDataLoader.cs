using UnityEngine;
using MiniJSON;
using System.IO;
using System.Collections.Generic;

public class TestDataLoader : MonoBehaviour{
	public void Start(){
	
	}
	public void Update(){
	
	}
	public void LoadGameData(){
		FileLoader src = new FileLoader ("JSONData" + Path.DirectorySeparatorChar + "MiscData","inventory");
		FileLoader dest = new FileLoader (Application.persistentDataPath,"Inventory","inventory");
		dest.Write(src.Read());
		
		FileLoader dialsrc = new FileLoader ("JSONData" + Path.DirectorySeparatorChar + "DialConfigs","testdial");
		FileLoader dialdest = new FileLoader (Application.persistentDataPath,"Dials","testdial");
		string json = dialsrc.Read ();
		dialdest.Write(json);
		Dictionary<string,System.Object> data = (Dictionary<string,System.Object>)Json.Deserialize (json);
		
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
