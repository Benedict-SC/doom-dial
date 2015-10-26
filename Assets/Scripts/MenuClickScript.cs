using UnityEngine;
using System.Collections;

public class MenuClickScript : MonoBehaviour, EventHandler {
	public GameObject parent;
	public int menuPosition = 0;
	public string[] levelList;
	// Use this for initialization
	void Start () {
		EventManager em = EventManager.Instance ();
		em.RegisterForEventType ("mouse_release", this);
		em.RegisterForEventType ("mouse_click", this);
		GameObject[] temp = new GameObject[2];
		temp =  GameObject.FindGameObjectsWithTag("DataHolder");
		if (temp.Length > 1) {
			Destroy (temp [1].gameObject);
		}

	}
	public void HandleEvent(GameEvent ge){
		if (ge.type.Equals ("mouse_release")) {
			RaycastHit targetFind;
			Ray targetSeek = Camera.main.ScreenPointToRay (InputWatcher.GetTouchPosition());
			if (Physics.Raycast (targetSeek, out targetFind)) {
				//gets stats of clicked building, triggers the GUI popups
				if (targetFind.collider.gameObject.tag == "Button") {
					//what triggers changes based on what menu the camera is focused on.
					if(targetFind.transform.position.x == 0.0f){
						Application.LoadLevel (levelList[menuPosition]);
					}
				}
			}
			
		}
	}
	// Update is called once per frame
	void Update () {

	}
}
