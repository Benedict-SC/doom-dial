using UnityEngine;
using System.Collections;

public class InputWatcher : MonoBehaviour {

	public static readonly bool INPUT_DEBUG = false;

	// Use this for initialization
	void Start () {
	
	}

	private bool isdown = false;
	private TouchPhase lastPhase = TouchPhase.Ended;

	// Update is called once per frame
	void Update () {
		if (INPUT_DEBUG) {
				if (isdown) {
					bool down = Input.GetMouseButton (0);
					if (!down) {
						Debug.Log ("released!!!");
						isdown = false;
						GameEvent releaseEvent = new GameEvent ("mouse_release");
						releaseEvent.addArgument (GetInputPosition ());
						EventManager.Instance ().RaiseEvent (releaseEvent);
					}
				} else {
					bool down = Input.GetMouseButton (0);
					if (down) {
						isdown = true;
						GameEvent clickEvent = new GameEvent ("mouse_click");
						clickEvent.addArgument (GetInputPosition ());
						EventManager.Instance ().RaiseEvent (clickEvent);
					}
				}
		} else {
			//first handle the main touch
			Touch main = Input.GetTouch(0);
			switch(main.phase){
				case TouchPhase.Began:
				if(lastPhase.Equals(TouchPhase.Began))
					break; //avoid double touch events
				GameEvent clickEvent = new GameEvent ("mouse_click");
				clickEvent.addArgument (GetInputPosition ());
				EventManager.Instance ().RaiseEvent (clickEvent);
				lastPhase = TouchPhase.Began;
				break;
				case TouchPhase.Ended:
				if(lastPhase.Equals(TouchPhase.Ended) || lastPhase.Equals (TouchPhase.Canceled))
					break; //avoid double touch events
				GameEvent clickEvent2 = new GameEvent ("mouse_release");
				clickEvent2.addArgument (GetInputPosition ());
				EventManager.Instance ().RaiseEvent (clickEvent2);
				lastPhase = TouchPhase.Ended;
				break;
				case TouchPhase.Canceled:
				if(lastPhase.Equals(TouchPhase.Ended) || lastPhase.Equals (TouchPhase.Canceled))
					break; //avoid double touch events
				GameEvent clickEvent3 = new GameEvent ("mouse_release");
				clickEvent3.addArgument (GetInputPosition ());
				EventManager.Instance ().RaiseEvent (clickEvent3);
				lastPhase = TouchPhase.Canceled;
				break;
				default:
				break;
			}
			
		}
	}

	public static Vector3 GetInputPosition(){
		if (INPUT_DEBUG) { //return mouse position in world
			return Camera.main.ScreenToWorldPoint (Input.mousePosition);
		} else { //return first finger position in world, or null if no touches exist
			Touch t = Input.GetTouch(0); 
			//apparently there's no null touch or null vector3, which is inconvenient
			/*if(t == null)
				return null;
			else*/
				return Camera.main.ScreenToWorldPoint (Input.GetTouch(0).position);
		}
	}
}
