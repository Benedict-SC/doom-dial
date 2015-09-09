using UnityEngine;
using System.Collections;

public class TowerSelect : MonoBehaviour, EventHandler {
	public GameObject Dial;
	public GameObject nameHolder;
	TowerLoad loader;
	public int menuPosition = 0;
	public string towerName = "";
	// Use this for initialization
	void Start () {
		EventManager em = EventManager.Instance ();
		em.RegisterForEventType ("mouse_release", this);
		em.RegisterForEventType ("mouse_click", this);
		loader = nameHolder.GetComponent<TowerLoad> ();
	}
	public void HandleEvent(GameEvent ge){

		if (ge.type.Equals ("mouse_release")) {
			float rotation = Dial.gameObject.transform.eulerAngles.z;
			float lockRot = Mathf.Round(rotation /60)*60;
			menuPosition = (int) lockRot/60;
			if (menuPosition == 6){
				menuPosition = 0;
			}
			loader.towerName = "joetower" + (menuPosition+1).ToString();
		}else if(ge.type.Equals("mouse_click")){
			RaycastHit targetFind;
			
			Ray targetSeek = Camera.main.ScreenPointToRay (InputWatcher.GetTouchPosition ());
			if (Physics.Raycast (targetSeek, out targetFind)) {
				//sees if ray collided with the start button
				if (targetFind.collider.gameObject == this.gameObject) {

					Application.LoadLevel("TowerEditor");
				}
			}
		}
	}
	// Update is called once per frame
	void Update () {
	
	}
}
