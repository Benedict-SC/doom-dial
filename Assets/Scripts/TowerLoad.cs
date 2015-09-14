using UnityEngine;
using System.Collections;

public class TowerLoad : MonoBehaviour {
	public string towerName = "drainpunch";
	bool killCheck = true;
	// Use this for initialization
	void Start () {
	
	}
	void Awake(){
		DontDestroyOnLoad(this);
	}
	// Update is called once per frame
	void Update () {
	if(Application.loadedLevelName == "TestScene 1" || Application.loadedLevelName == "TowerEditor"){ 
			killCheck = false;
		   }
		if(killCheck){
			Destroy(this);
		}
	}
}
