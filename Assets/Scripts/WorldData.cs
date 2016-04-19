using UnityEngine;
using System.Collections.Generic;
using System.IO;
using MiniJSON;

public class WorldData : MonoBehaviour {
	//Saves data for later use
	//0 means this is the one at the very start, 1 means it is placed manually to deal with scenes that depend
	//on it, and will auto-delete normally.
	public int placeholder = 0;
	public static string worldSelected = "";
	public static int levelIndex = -1;
	public static string levelSelected = "testlevel";
	public static string lastScene = "";
	public static string dialSelected = "devdial";
	
	//a bool for use by the wave manager to tell if it should be loading a user level
	public static bool loadUserLevel = false;
	
	// Use this for initialization
	void Start () {
	}
	// Update is called once per frame
	void Update () {
	
	}
	public static void LoadNextLevel(){
		if(levelIndex < 0 || levelIndex > 4){ //last level has no next level
			return;
		}
		levelIndex++;
		FileLoader fl = new FileLoader ("JSONData" + Path.DirectorySeparatorChar + "Campaign" + Path.DirectorySeparatorChar + "Worlds",WorldData.worldSelected);
		string json = fl.Read ();
		Dictionary<string,System.Object> data = (Dictionary<string,System.Object>)Json.Deserialize (json);
		
		List<System.Object> levels = (List<System.Object>)data["levels"];
		Dictionary<string,System.Object> ldata = (Dictionary<string,System.Object>)levels[levelIndex];
		levelSelected = (string)ldata["filename"];
		Debug.Log("level is: " + levelSelected);
	}
}
