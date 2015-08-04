using UnityEngine;
using System.Collections;

public class WorldData : MonoBehaviour {
	//Saves data for later use
	public string worldSelected = "";
	public string levelSelected = "";
	public string lastScene = "";
	// Use this for initialization
	void Start () {
	
	}
	void Awake(){
		DontDestroyOnLoad(this);
	}
	// Update is called once per frame
	void Update () {
	
	}
}
