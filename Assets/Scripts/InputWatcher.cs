using UnityEngine;
using System.Collections;

public class InputWatcher : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}

	private bool isdown = false;

	// Update is called once per frame
	void Update () {
		if (isdown) {
			bool down = Input.GetMouseButtonDown (0);
			if(!down){
				isdown = false;
				GameEvent releaseEvent = new GameEvent("mouse_release");
				releaseEvent.addArgument(Input.mousePosition);
				EventManager.Instance().RaiseEvent(releaseEvent);
			}
		} else {
			bool down = Input.GetMouseButtonDown (0);
			if(down){
				isdown = true;
				GameEvent clickEvent = new GameEvent("mouse_click");
				clickEvent.addArgument(Input.mousePosition);
				EventManager.Instance().RaiseEvent(clickEvent);
			}
		}


	}
}
