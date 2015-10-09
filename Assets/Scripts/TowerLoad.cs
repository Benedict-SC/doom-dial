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
	if (Application.loadedLevelName == "TowerSelect" || Application.loadedLevelName == "TowerEditor") { 
			killCheck = false;
		} else {
			killCheck = true;
		}
		if(killCheck){
			Destroy(gameObject);
		}
	}
}
