using UnityEngine;
using System.Collections;

public class MenuSelect : MonoBehaviour, EventHandler {
	public int menuPosition = 0;
	public GameObject textMesh;
	public GameObject startButton;
	public GameObject menuButton;
	public GameObject worldHolder;
	string levelName = "WorldSelect";
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
				//sees if ray collided with the start button
				if (targetFind.collider.gameObject == startButton) {
					//Debug.Log ("try and load level select");
					Application.LoadLevel(levelName);
				}
				if (targetFind.collider.gameObject == menuButton) {
					Debug.Log("Test");
					test = worldHolder.GetComponent<WorldData>().lastScene;
					Application.LoadLevel(test);
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

				break;
			case 1:

				break;
			case 2:

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