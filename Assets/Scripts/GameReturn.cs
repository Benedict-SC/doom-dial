using UnityEngine;
using System.Collections;

public class GameReturn : MonoBehaviour, EventHandler {
	public GameObject CamLock;
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
			if (Physics.Raycast (targetSeek, out targetFind)) {
				//sees if ray collided with the start button
				if (targetFind.collider.gameObject == this.gameObject) {
					/*GameObject temp = GameObject.FindGameObjectWithTag("DataHolder");
					temp.GetComponent<WorldData>().lastScene = "TestScene";
					Application.LoadLevel("Menu");*/
					Camera.main.transform.position = CamLock.transform.position;
				}
			}
		}
	}
	// Update is called once per frame
	void Update () {
	
	}
}
