using UnityEngine;
using System.Collections;

public class Pause : MonoBehaviour {
	public static bool paused = false;
	public static float pausableTime = 0f;
	//darkens the screen when game is paused
	public GameObject tintBox = null;
	//holds the menu button so it can pop in when paused
	public GameObject returnButton = null;
	//keeps track of where buttons will go when paused
	public GameObject[] anchorPoints = null;
	//Keep track of all things needed for pause
	// Use this for initialization
	void Start () {

	}
	
	// Update is called once per frame
	void Update () {
	
	}
	public void PauseTrigger(){
		if(!paused){
			//moves buttons to set locations, and darkens screen
			this.gameObject.transform.position = anchorPoints[0].gameObject.transform.position;
			returnButton.transform.position = anchorPoints[2].gameObject.transform.position;
			GetComponentInChildren<TextMesh>().text = "Resume";
			tintBox.GetComponent<Renderer> ().material.color = new Color (0.0f, 0.0f, 0.0f, 0.5f);
		}else{
			//moves buttons back, sets screen back to normal color
			this.gameObject.transform.position = anchorPoints[1].gameObject.transform.position;
			returnButton.transform.position = anchorPoints[3].gameObject.transform.position;
			GetComponentInChildren<TextMesh>().text = "Pause";
			tintBox.GetComponent<Renderer> ().material.color = new Color (0.0f, 0.0f, 0.0f, 0.0f);
		}
		paused = !paused;
	}
}
