using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class MenuScript : MonoBehaviour {

	public int menuPosition = 0;
	public GameObject textMesh;
	public GameObject startButton;
	public string[] descHolder;
	public string[] levelHolder;
	string levelName = "";
	int lastPosition = 1;
	// Use this for initialization
	void Start () {

	}
	
	// Update is called once per frame
	void Update () {
		menuPosition = (int)((gameObject.transform.eulerAngles.z /30) +2)%4;
		if (menuPosition != lastPosition) {
			textMesh.GetComponent<Text>().text = descHolder[menuPosition];
			lastPosition = menuPosition;
		}
	}
	public void MenuTrigger(){
		Debug.Log (levelHolder[menuPosition]);
		if(levelHolder[menuPosition] == "Return"){
			Camera.main.transform.position = new Vector3(Camera.main.transform.position.x,Camera.main.transform.position.y,-10f);	
		}else{
			WorldData.lastScene = Application.loadedLevelName;
			levelName = levelHolder[menuPosition];
			Pause.paused = false;
			Application.LoadLevel(levelName);
		}
	}
}
