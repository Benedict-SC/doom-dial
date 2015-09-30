using UnityEngine;
using System.Collections;

public class WorldData : MonoBehaviour {
	//Saves data for later use
	//0 means this is the one at the very start, 1 means it is placed manually to deal with scenes that depend
	//on it, and will auto-delete normally.
	public int placeholder = 0;
	public string worldSelected = "";
	public string levelSelected = "";
	public string lastScene = "";
	// Use this for initialization
	void Start () {
		GameObject[] temp = GameObject.FindGameObjectsWithTag ("DataHolder");
		if (temp.Length > 1 && placeholder == 1) {
			Destroy(this.gameObject);
		}
	}
	void Awake(){
		DontDestroyOnLoad(this);
	}
	// Update is called once per frame
	void Update () {
	
	}
}
