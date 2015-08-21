using UnityEngine;
using System.Collections;

public class GamePause : MonoBehaviour, EventHandler {
	bool isPaused = false;
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
					if(isPaused){
					//Time.timeScale = 1.0f;
					}else{
						//Time.timeScale = 0.0f;
					}
					Time.timeScale = 0.0f;
					Time.fixedDeltaTime = 0.0f;
					Debug.Log ("Paused " + Time.timeScale);
					isPaused = !isPaused;
				}
			}
			
		}
	}
	// Update is called once per frame
	void Update () {
	
	}
}
