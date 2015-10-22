using UnityEngine;
using System.Collections;

public class LevelSelect : MonoBehaviour, EventHandler {
	public int menuPosition = 0;
	public GameObject textMesh;
	public GameObject startButton;
	public WorldData WorldData;
	public GameObject menuButton;
	int levelName = 0;
	int lastPosition = 1;
	// Use this for initialization
	void Start () {
		EventManager em = EventManager.Instance ();
		em.RegisterForEventType ("mouse_release", this);
		em.RegisterForEventType ("mouse_click", this);
		//Need to find old object by hand as it isn't on the scene to start.
		WorldData = GameObject.FindWithTag ("DataHolder").GetComponent<WorldData> ();
	}
	public void HandleEvent(GameEvent ge){
		if (ge.type.Equals ("mouse_release")) {
			RaycastHit targetFind;
			
			Ray targetSeek = Camera.main.ScreenPointToRay (InputWatcher.GetTouchPosition ());
			if (Physics.Raycast (targetSeek, out targetFind)) {
				//sees if ray collided with the start button
				if (targetFind.collider.gameObject == startButton) {
					
					Application.LoadLevel(levelName);
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
				textMesh.GetComponent<TextMesh>().text = WorldData.worldSelected + "-2";
				WorldData.levelSelected = "Level2";
				levelName = 8;
				break;
			case 1:
				textMesh.GetComponent<TextMesh>().text = WorldData.worldSelected + "-3";
				WorldData.levelSelected = "Level3";
				levelName = 8;
				break;
			case 2:
				textMesh.GetComponent<TextMesh>().text = WorldData.worldSelected + "-4";
				WorldData.levelSelected = "Level4";
				levelName = 8;
				break;
			case 3:
				textMesh.GetComponent<TextMesh>().text = WorldData.worldSelected + "-1";
				WorldData.levelSelected = "Level1";
				levelName = 8;
				break;
			default:
				break;
			}
			lastPosition = menuPosition;
		}
	}
}