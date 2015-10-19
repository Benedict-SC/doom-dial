using UnityEngine;
using System.Collections;

public class InputWatcher : MonoBehaviour {

	public static readonly bool INPUT_DEBUG = true;

	// Use this for initialization
	void Start () {
		tapLengthWatcher = new Timer();
		for(int i = 0; i < 9; i++){
			altWatchers[i] = new Timer();
		}
	}
	private Timer tapLengthWatcher;
	private bool isdown = false;
	private TouchPhase lastPhase = TouchPhase.Ended;
	
	private Timer[] altWatchers = {null,null,null,null,null,null,null,null,null};
	private TouchPhase[] altPhases = {TouchPhase.Ended,TouchPhase.Ended,TouchPhase.Ended,TouchPhase.Ended,
		TouchPhase.Ended,TouchPhase.Ended,TouchPhase.Ended,TouchPhase.Ended,TouchPhase.Ended};
		// Update is called once per frame
	void Update () {
		if (INPUT_DEBUG) {
				if (isdown) {
					bool down = Input.GetMouseButton (0);
					if (!down) {
						//Debug.Log ("released!!!");
						isdown = false;
						GameEvent releaseEvent = new GameEvent ("mouse_release");
						releaseEvent.addArgument (GetInputPosition ());
						EventManager.Instance ().RaiseEvent (releaseEvent);
						if(tapLengthWatcher.TimeElapsedMillis() < 200){
							GameEvent tapEvent = new GameEvent("tap");
							tapEvent.addArgument(GetInputPosition());
							EventManager.Instance().RaiseEvent(tapEvent);
						}
					}
				} else {
					bool down = Input.GetMouseButton (0);
					if (down) {
						isdown = true;
						GameEvent clickEvent = new GameEvent ("mouse_click");
						clickEvent.addArgument (GetInputPosition ());
						EventManager.Instance ().RaiseEvent (clickEvent);
						tapLengthWatcher.Restart();
					}
				}
		} else {
			//first handle the main touch
			if(Input.touchCount < 1){
				return;
			}
			Touch main = Input.GetTouch(0);
			switch(main.phase){
				case TouchPhase.Began:
				if(lastPhase.Equals(TouchPhase.Began))
					break; //avoid double touch events
				GameEvent clickEvent = new GameEvent ("mouse_click");
				clickEvent.addArgument (GetInputPosition ());
				EventManager.Instance ().RaiseEvent (clickEvent);
				lastPhase = TouchPhase.Began;
				//start timer for taps
				tapLengthWatcher.Restart();
				break;
				case TouchPhase.Ended:
				if(lastPhase.Equals(TouchPhase.Ended) || lastPhase.Equals (TouchPhase.Canceled))
					break; //avoid double touch events
				GameEvent clickEvent2 = new GameEvent ("mouse_release");
				clickEvent2.addArgument (GetInputPosition ());
				EventManager.Instance ().RaiseEvent (clickEvent2);
				lastPhase = TouchPhase.Ended;
				if(tapLengthWatcher.TimeElapsedMillis() < 200){
					GameEvent tapEvent = new GameEvent("tap");
					tapEvent.addArgument(GetInputPosition());
					EventManager.Instance().RaiseEvent(tapEvent);
				}
				break;
				case TouchPhase.Canceled:
				if(lastPhase.Equals(TouchPhase.Ended) || lastPhase.Equals (TouchPhase.Canceled))
					break; //avoid double touch events
				GameEvent clickEvent3 = new GameEvent ("mouse_release");
				clickEvent3.addArgument (GetInputPosition ());
				EventManager.Instance ().RaiseEvent (clickEvent3);
				lastPhase = TouchPhase.Canceled;
				if(tapLengthWatcher.TimeElapsedMillis() < 200){
					GameEvent tapEvent = new GameEvent("tap");
					tapEvent.addArgument(GetInputPosition());
					EventManager.Instance().RaiseEvent(tapEvent);
				}
				break;
				default:
				break;
			}
			//next handle further touches
			for(int i = 2; i <= Input.touchCount; i++){
				Touch current = Input.GetTouch(i-1);
				switch(current.phase){
					case TouchPhase.Began:
						if(altPhases[i-2].Equals(TouchPhase.Began))
							break; //avoid double touch events
						GameEvent clickEvent = new GameEvent ("alt_click");
						clickEvent.addArgument (GetInputPosition (i-1));
						clickEvent.addArgument(i-1);
						EventManager.Instance ().RaiseEvent (clickEvent);
						altPhases[i-2] = TouchPhase.Began;
						//start timer for taps
						altWatchers[i-2].Restart();
						break;
					case TouchPhase.Ended:
						if(altPhases[i-2].Equals(TouchPhase.Ended) || altPhases[i-2].Equals (TouchPhase.Canceled))
							break; //avoid double touch events
						GameEvent clickEvent2 = new GameEvent ("alt_release");
						clickEvent2.addArgument (GetInputPosition (i-1));
						clickEvent2.addArgument(i-1);
						EventManager.Instance ().RaiseEvent (clickEvent2);
						altPhases[i-2] = TouchPhase.Ended;
						if(altWatchers[i-2].TimeElapsedMillis() < 200){
							GameEvent tapEvent = new GameEvent("alt_tap");
							tapEvent.addArgument(GetInputPosition(i-1));
							tapEvent.addArgument(i-1);
							EventManager.Instance().RaiseEvent(tapEvent);
						}
						break;
					case TouchPhase.Canceled:
						if(altPhases[i-2].Equals(TouchPhase.Ended) || altPhases[i-2].Equals (TouchPhase.Canceled))
							break; //avoid double touch events
						GameEvent clickEvent3 = new GameEvent ("alt_release");
						clickEvent3.addArgument (GetInputPosition (i-1));
						clickEvent3.addArgument(i-1);
						EventManager.Instance ().RaiseEvent (clickEvent3);
						altPhases[i-2] = TouchPhase.Canceled;
						if(altWatchers[i-2].TimeElapsedMillis() < 200){
							GameEvent tapEvent = new GameEvent("alt_tap");
							tapEvent.addArgument(GetInputPosition(i-1));
							tapEvent.addArgument(i-1);
							EventManager.Instance().RaiseEvent(tapEvent);
						}
						break;
					default:
						break;
				}
			}
		}
	}
	static Vector3 lastPos = new Vector3(0f,0f,0f);
	public static Vector3 GetInputPosition(){
		if (INPUT_DEBUG) { //return mouse position in world
			return Camera.main.ScreenToWorldPoint (Input.mousePosition);
		} else { //return first finger position in world, or 0,0,0 if no touches exist
			if(Input.touchCount < 1){
				return lastPos;
			}
			Touch t = Input.GetTouch(0); 
			//apparently there's no null touch or null vector3, which is inconvenient
			/*if(t == null)
				return null;
			else*/
			lastPos = Camera.main.ScreenToWorldPoint (Input.GetTouch(0).position);
			return lastPos;
		}
	}
	static Vector3 lastTouch = new Vector3(0f,0f,0f);
	public static Vector3 GetTouchPosition(){ //??? what's this secondary thing doing here? it doesn't convert the thing?
		if (INPUT_DEBUG) { //return mouse position in world
			return Input.mousePosition;
		} else { //return first finger position in world, or 0,0,0 if no touches exist
			if(Input.touchCount < 1)
				return lastTouch;
			Touch t = Input.GetTouch(0); 
			//apparently there's no null touch or null vector3, which is inconvenient
			/*if(t == null)
				return null;
			else*/
			lastTouch = t.position;
			return t.position;
		}
	}
	static Vector3[] lastPositions = {new Vector3(0f,0f,0f),
									new Vector3(0f,0f,0f),
									new Vector3(0f,0f,0f),
									new Vector3(0f,0f,0f),
									new Vector3(0f,0f,0f),
									new Vector3(0f,0f,0f),
									new Vector3(0f,0f,0f),
									new Vector3(0f,0f,0f),
									new Vector3(0f,0f,0f)};
	public static Vector3 GetInputPosition(int idx){
		if (idx == 0)
			return GetInputPosition();
		if (INPUT_DEBUG) { //this won't be true if we're calling this,
			return Camera.main.ScreenToWorldPoint (Input.mousePosition);
		} else { //return first finger position in world, or 0,0,0 if no touches exist
			if(Input.touchCount < 2)
				return lastPositions[idx-1];
			Touch t = Input.GetTouch(idx);
			if(t.phase != TouchPhase.Ended)
				lastPositions[idx-1] = Camera.main.ScreenToWorldPoint (t.position);
			return lastPositions[idx-1];
		}
	}
	
	public static bool IsInputDown(){
		if(INPUT_DEBUG){
			return Input.GetMouseButtonDown(0);
		}else{
			if(Input.touchCount == 0){
				return false;
			}
			return true;
		}
	}
}
