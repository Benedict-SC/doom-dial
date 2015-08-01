using UnityEngine;
using System.Collections;

public class LevelSelect : MonoBehaviour, EventHandler {
	public int menuPosition = 0;
	public GameObject textMesh;
	public GameObject startButton;
	public WorldData WorldData;
	string levelName = "LevelHolder";
	int lastPosition = 1;
	// Use this for initialization
	void Start () {
		EventManager em = EventManager.Instance ();
		em.RegisterForEventType ("mouse_release", this);
		em.RegisterForEventType ("mouse_click", this);
		WorldData = GameObject.FindWithTag ("DataHolder").GetComponent<WorldData> ();
	}
	public void HandleEvent(GameEvent ge){
		if (ge.type.Equals ("mouse_release")) {
			RaycastHit targetFind;
			
			Ray targetSeek = Camera.main.ScreenPointToRay (InputWatcher.GetTouchPosition ());
			Debug.Log(InputWatcher.GetInputPosition());
			if (Physics.Raycast (targetSeek, out targetFind)) {
				//gets stats of clicked building, triggers the GUI popups
				if (targetFind.collider.gameObject == startButton) {
					
					//Application.LoadLevel(levelName);
				}
			}
			
		}
	}
	// Update is called once per frame
	void Update () {
		if (menuPosition != lastPosition) {
			switch(menuPosition){
			case 0:
				textMesh.GetComponent<TextMesh>().text = WorldData.levelSelected + "-2";
				levelName = "TestScene";
				break;
			case 1:
				textMesh.GetComponent<TextMesh>().text = WorldData.levelSelected + "-3";
				levelName = "TestScene";
				break;
			case 2:
				textMesh.GetComponent<TextMesh>().text = WorldData.levelSelected + "-4";
				levelName = "TestScene";
				break;
			case 3:
				textMesh.GetComponent<TextMesh>().text = WorldData.levelSelected + "-1";
				levelName = "TestScene";
				break;
			default:
				break;
			}
			lastPosition = menuPosition;
		}
	}
}