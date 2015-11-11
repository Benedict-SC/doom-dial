using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class WaveFrameController : MonoBehaviour, EventHandler{

	static float trackbounds = 2062.5f;
	static float[] thresholds = {1625f,800f,-25f,-850f,-1675f};
	
	public ZonePanelController[] zonepanels;
	public List<EnemyListEntryController>[] zonelists;
	static bool dragging = false;
	static bool snapping = false;
	static int snapIndex = 0;
	static float snapTarget = 2062.5f;
	GameObject leveltrack;
	static float initialTrackPos = trackbounds;
	static float initialTouchPos = 0f;
	RectTransform canvas;
	
	public void Start(){
		leveltrack = GameObject.Find ("WaveTrack").gameObject;
		//canvas = (RectTransform)WaveEditorController.singleton.canvas.transform;
		EventManager.Instance().RegisterForEventType("mouse_click",this);
		EventManager.Instance().RegisterForEventType("mouse_release",this);
		zonepanels = new ZonePanelController[6];
		zonelists = new List<EnemyListEntryController>[6];
		for(int i = 0; i < 6; i++){ //associate lists and panels with this frame
			string id = "ZonePanel" + i;
			zonepanels[i] = transform.FindChild(id).gameObject.GetComponent<ZonePanelController>();
			zonelists[i] = new List<EnemyListEntryController>();
			zonepanels[i].SetList(zonelists[i]);
		}
	}
	public void Update(){
		if(eventWaiting){ //this waiting stuff is to make sure nothing else is grabbing the player's input before starting on the drag
			clickUpdateCycled = true;
			eventWaiting = false;
		}else if(clickUpdateCycled){ //now we can handle the event
			if(!WaveEditorController.singleton.IsMoving()){
				if(BossTabController.open){
					return;
				}
				//go into drag mode
				dragging = true;
				RectTransform rt = (RectTransform)leveltrack.transform;
				initialTrackPos = rt.anchoredPosition.x;
				Vector3 touchpos = (Vector3)heldEvent.args[0];
				//RectTransform canvas = (RectTransform)WaveEditorController.singleton.canvas.transform;
				Vector3 newpoint = canvas.InverseTransformPoint(new Vector2(touchpos.x,touchpos.y));
				initialTouchPos = newpoint.x;
			}
			clickUpdateCycled = false;	
		}
		if(WaveEditorController.singleton.activeWaveFrame != this)
			return; //don't do it all over again
		if(dragging && snapping){
			Debug.Log ("NO DRAGGING AND SNAPPING AT THE SAME TIME");
		}
		if(dragging){
			Debug.Log ("dragging");
			RectTransform rt = (RectTransform)leveltrack.transform;
			Vector3 touchpos = InputWatcher.GetInputPosition();
			//RectTransform canvas = (RectTransform)WaveEditorController.singleton.canvas.transform;
			Vector3 newpoint = canvas.InverseTransformPoint(new Vector2(touchpos.x,touchpos.y));
			float x = newpoint.x-initialTouchPos;
			if(initialTrackPos+x >trackbounds)
				rt.anchoredPosition = new Vector2 (trackbounds,rt.anchoredPosition.y);
			else if(initialTrackPos + x < -trackbounds)
				rt.anchoredPosition = new Vector2 (-trackbounds,rt.anchoredPosition.y);
			else
				rt.anchoredPosition = new Vector2 (initialTrackPos + x,rt.anchoredPosition.y);
		}
		if(snapping){
			Debug.Log ("snapping");
			RectTransform rt = (RectTransform)leveltrack.transform;
			float trackPos = rt.anchoredPosition.x;
			float distance = snapTarget - trackPos; //distance should always be 412.5 > x > -412.5
			//Debug.Log(distance);
			if(distance > 0){
				if(Mathf.Abs(distance) > 200)
					rt.anchoredPosition = new Vector2(rt.anchoredPosition.x + 10,rt.anchoredPosition.y);
				else if(Mathf.Abs(distance) > 80)
					rt.anchoredPosition = new Vector2(rt.anchoredPosition.x + 9,rt.anchoredPosition.y);
				else if(Mathf.Abs(distance) > 40)
					rt.anchoredPosition = new Vector2(rt.anchoredPosition.x + 8,rt.anchoredPosition.y);
				else if(Mathf.Abs(distance) > 20)
					rt.anchoredPosition = new Vector2(rt.anchoredPosition.x + 6,rt.anchoredPosition.y);
				else if(Mathf.Abs(distance) > 2.1)
					rt.anchoredPosition = new Vector2(rt.anchoredPosition.x + 4,rt.anchoredPosition.y);
				else if(Mathf.Abs(distance) < 2.1){
					rt.anchoredPosition = new Vector2(snapTarget,rt.anchoredPosition.y);
					snapping = false;
				}
			}else{
				if(Mathf.Abs(distance) > 200)
					rt.anchoredPosition = new Vector2(rt.anchoredPosition.x - 10,rt.anchoredPosition.y);
				else if(Mathf.Abs(distance) > 80)
					rt.anchoredPosition = new Vector2(rt.anchoredPosition.x - 9,rt.anchoredPosition.y);
				else if(Mathf.Abs(distance) > 40)
					rt.anchoredPosition = new Vector2(rt.anchoredPosition.x - 8,rt.anchoredPosition.y);
				else if(Mathf.Abs(distance) > 20)
					rt.anchoredPosition = new Vector2(rt.anchoredPosition.x - 6,rt.anchoredPosition.y);
				else if(Mathf.Abs(distance) > 2.1)
					rt.anchoredPosition = new Vector2(rt.anchoredPosition.x - 4,rt.anchoredPosition.y);
				else if(Mathf.Abs(distance) < 2.1){
					rt.anchoredPosition = new Vector2(snapTarget,rt.anchoredPosition.y);
					snapping = false;
				}
			}
		}
	}
	public void Reset(){ //if something else needs to take input focus from the dragging, this sets the wave track to its resting state
		Debug.Log ("reset");
		dragging = false;
		snapping = false;
		clickUpdateCycled = false;
		eventWaiting = false;
		if(snapIndex == 0)
			snapTarget = trackbounds;
		else{
			snapTarget = thresholds[snapIndex-1] - 387.5f;
		}
		RectTransform rt = (RectTransform)leveltrack.transform;
		rt.anchoredPosition = new Vector2(snapTarget,rt.anchoredPosition.y);
	}
	
	static bool clickUpdateCycled = false; //on mouse click event, wait for everyone to update before processing
	static bool eventWaiting = false;
	static GameEvent heldEvent = null;
	public void HandleEvent(GameEvent ge){
		if(ge.type.Equals("mouse_click")){
			if(snapping || dragging)
				return;
			canvas = (RectTransform)WaveEditorController.singleton.canvas.transform;
			RectTransform rt = (RectTransform)transform;
			Vector3 touchpos = (Vector3)ge.args[0];
			Vector3 newpoint = rt.InverseTransformPoint(new Vector2(touchpos.x,touchpos.y));
			if(!rt.rect.Contains(new Vector2(newpoint.x,newpoint.y))){
				//Debug.Log ("click not on thing");
				return;
			}else{
				//Debug.Log ("a frame thought it was being clicked");
			}
			eventWaiting = true;
			heldEvent = ge;
		}else{ //it's a mouse release event
			if(!dragging)
				return;
			dragging = false;
			snapping = true;
			//decide which to snap to
			
			//swipe distance based
			float movementThreshold = 200f;
			Vector3 touchpos = InputWatcher.GetInputPosition();
			Vector3 newpoint = canvas.InverseTransformPoint(new Vector2(touchpos.x,touchpos.y));
			float distance = newpoint.x - initialTouchPos;
			if(distance > movementThreshold){
				Debug.Log ("old snap index: " + snapIndex);
				snapIndex--;
				Debug.Log ("new snap index: " + snapIndex);
				if(snapIndex < 0){
					Debug.Log ("snap index is " + snapIndex + " which is under 0. setting to " + 0);	
					snapIndex = 0;
				}
			}else if(distance < -movementThreshold){
				Debug.Log ("old snap index: " + snapIndex);
				snapIndex++;
				Debug.Log ("new snap index: " + snapIndex);
				if(snapIndex >= thresholds.Length + 1){
					Debug.Log ("snap index is " + snapIndex + " which is over five. setting to " + thresholds.Length);	
					snapIndex = thresholds.Length;
				}
			}
			
			
			//absolute position based
			/*RectTransform rt = (RectTransform)leveltrack.transform;
			float trackPos = rt.anchoredPosition.x;
			snapIndex = 0;
			for(int i = 0; i < thresholds.Length; i++){
				if(trackPos < thresholds[i]){
					snapIndex++;
				}else{
					break;
				}
			}*/
			
			//snapIndex is now the index of the frame to snap to
			WaveEditorController.singleton.SetActiveFrame(snapIndex);
			if(snapIndex == 0)
				snapTarget = trackbounds;
			else{
				snapTarget = thresholds[snapIndex-1] - 387.5f;
			}
		}
	}
	public bool IsEmpty(){
		foreach(List<EnemyListEntryController> column in zonelists){
			if(column.Count != 0)
				return false;
		}
		return true;
	}
}
