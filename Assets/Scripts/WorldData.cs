using UnityEngine;
using System.Collections;

public class WorldData : MonoBehaviour {
	public string levelSelected = "";
	// Use this for initialization
	void Start () {
	
	}
	void Awake(){
		DontDestroyOnLoad(this);
		Debug.Log (Application.loadedLevelName);
		//if(Application.loadedLevelName)
	}
	// Update is called once per frame
	void Update () {
	
	}
}
