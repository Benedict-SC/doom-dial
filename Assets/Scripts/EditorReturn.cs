using UnityEngine;
using System.Collections;

public class EditorReturn : MonoBehaviour, EventHandler {
	public GameObject worldHolder;
	// Use this for initialization
	void Start () {
		EventManager em = EventManager.Instance ();
		em.RegisterForEventType ("mouse_release", this);
		em.RegisterForEventType ("mouse_click", this);
		worldHolder = GameObject.FindWithTag ("DataHolder");
	}
	public void HandleEvent(GameEvent ge){
		
		if(ge.type.Equals("mouse_click")){
			RaycastHit targetFind;
			
			Ray targetSeek = Camera.main.ScreenPointToRay (InputWatcher.GetTouchPosition ());
			if (Physics.Raycast (targetSeek, out targetFind)) {
				//sees if ray collided with the start button
				if (targetFind.collider.gameObject == this.gameObject) {
					//if(worldHolder){
					//Application.LoadLevel(worldHolder.GetComponent<WorldData>().lastScene);
					//}else{
						Application.LoadLevel(9);
					//}
				}
			}
		}
	}
	// Update is called once per frame
	void Update () {
	
	}
}
