using UnityEngine;
using System.Collections;

public class MenuInGame : MonoBehaviour, EventHandler {

	public int menuPosition = 0;
	public GameObject textMesh;
	public GameObject startButton;
	public GameObject menuButton;
	public GameObject worldHolder;
	public GameObject returnButton;
	public GameObject cameraLock2;
	string levelName = "";
	int lastPosition = 1;
	public string test;
	// Use this for initialization
	void Start () {
		EventManager em = EventManager.Instance ();
		em.RegisterForEventType ("mouse_release", this);
		em.RegisterForEventType ("mouse_click", this);
		worldHolder = GameObject.FindWithTag ("DataHolder");
	}
	public void HandleEvent(GameEvent ge){
		if (ge.type.Equals ("mouse_release")) {
			RaycastHit targetFind;

			Ray targetSeek = Camera.main.ScreenPointToRay (InputWatcher.GetTouchPosition ());
			if (Physics.Raycast (targetSeek, out targetFind)) {
				Debug.Log (targetFind.collider.name);
				//sees if ray collided with the start button
				if (targetFind.collider.gameObject == startButton) {
					//Debug.Log ("try and load level select");
					if(menuPosition == 3 || menuPosition == 1){
						worldHolder.GetComponent<WorldData>().lastScene = Application.loadedLevelName;
						Application.LoadLevel(levelName);
					}else if(menuPosition == 2){
						Camera.main.transform.position = cameraLock2.transform.position;
					}
				}
			}	
		}
	}
	// Update is called once per frame
	void Update () {
		//Stops entire statement from running every frame to save overhead
		if (menuPosition != lastPosition) {
			switch(menuPosition){
				//Sets values for WorldData, the on screen text, and the level that will be loaded
			case 0:
				textMesh.GetComponent<TextMesh>().text = "Settings";
				Debug.Log("Settings not done, add in later");
				break;
			case 1:
				textMesh.GetComponent<TextMesh>().text = "Tower Editor";
				levelName = "TowerSelect";

				break;
			case 2:
				textMesh.GetComponent<TextMesh>().text = "Back";
				//levelName = worldHolder.GetComponent<WorldData>().lastScene;
				break;
			case 3:
				textMesh.GetComponent<TextMesh>().text = "Main Menu";
				levelName = "MenuTest";
				break;
			default:
				break;
			}
			lastPosition = menuPosition;
		}
	}
}