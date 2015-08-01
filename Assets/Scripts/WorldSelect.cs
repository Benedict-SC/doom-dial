using UnityEngine;
using System.Collections;

public class WorldSelect : MonoBehaviour, EventHandler {
	public int menuPosition = 0;
	public GameObject textMesh;
	public GameObject startButton;
	public GameObject worldHolder;
	string levelName = "WorldSelect";
	int lastPosition = 1;
	// Use this for initialization
	void Start () {
		EventManager em = EventManager.Instance ();
		em.RegisterForEventType ("mouse_release", this);
		em.RegisterForEventType ("mouse_click", this);
	}
	public void HandleEvent(GameEvent ge){
		if (ge.type.Equals ("mouse_release")) {
			RaycastHit targetFind;

			Ray targetSeek = Camera.main.ScreenPointToRay (InputWatcher.GetTouchPosition ());
			Debug.Log(InputWatcher.GetInputPosition());
			if (Physics.Raycast (targetSeek, out targetFind)) {
				//gets stats of clicked building, triggers the GUI popups
				if (targetFind.collider.gameObject == startButton) {
					//Debug.Log ("try and load level select");
					Application.LoadLevel(levelName);
				}
			}

		}
	}
	// Update is called once per frame
	void Update () {
		if (menuPosition != lastPosition) {
			switch(menuPosition){
			case 0:
				worldHolder.GetComponent<WorldData>().levelSelected = "World 2";
				textMesh.GetComponent<TextMesh>().text = "World 2";
				levelName = "LevelSelect";
				break;
			case 1:
				worldHolder.GetComponent<WorldData>().levelSelected = "World 3";
				textMesh.GetComponent<TextMesh>().text = "World 3";
				levelName = "LevelSelect";
				break;
			case 2:
				worldHolder.GetComponent<WorldData>().levelSelected = "World 4";
				textMesh.GetComponent<TextMesh>().text = "World 4";
				levelName = "LevelSelect";
				break;
			case 3:
				worldHolder.GetComponent<WorldData>().levelSelected = "World 1";
				textMesh.GetComponent<TextMesh>().text = "World 1";
				levelName = "LevelSelect";
				break;
			default:
				break;
			}
			lastPosition = menuPosition;
		}
	}
}
