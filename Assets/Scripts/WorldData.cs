using UnityEngine;
using System.Collections;

public class WorldData : MonoBehaviour {
	//Saves data for later use
	//0 means this is the one at the very start, 1 means it is placed manually to deal with scenes that depend
	//on it, and will auto-delete normally.
	public int placeholder = 0;
	public static string worldSelected = "";
	public static string levelSelected = "";
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
}
