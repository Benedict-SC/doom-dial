using UnityEngine;
using UnityEngine.UI;

public class CanvasSpinner : MonoBehaviour,EventHandler{

	public float anchorX = 0;
	public float anchorY = 0;
	
	Canvas canvas;
	
	bool spinning = false;
	float startingMouseRot;
	float startingDialRot;
	GunButton[] gunButtons = new GunButton[6];
	
	public void Start(){
		canvas = GameObject.Find("Canvas").GetComponent<Canvas>();
		RectTransform rt = (RectTransform)transform;
		EventManager em = EventManager.Instance ();
		em.RegisterForEventType ("mouse_release", this);
		em.RegisterForEventType ("mouse_click", this);
		anchorX = rt.anchoredPosition.x;
		anchorY = rt.anchoredPosition.y;
		
		for(int i = 1; i < 7; i++){
			gunButtons[i-1] = GameObject.Find("Button"+i).GetComponent<GunButton>();
		}
	}
	public void HandleEvent(GameEvent ge){
		if(Pause.paused)
			return;
		Vector3 mousepos = InputWatcher.GetCanvasInputPosition((RectTransform)canvas.transform);
		if(ge.type.Equals("mouse_click")){
			if(spinning)
				return;
			if(TouchIsOnGunButtons()){
				return;
			}else if(mousepos.magnitude < Dial.DIAL_RADIUS - 28f){
				return;
			}else{
				spinning = true;
				startingDialRot = transform.eulerAngles.z * Mathf.Deg2Rad;
				startingMouseRot = Mathf.Atan2(mousepos.y-anchorY,mousepos.x-anchorX);
			}
		}else if(ge.type.Equals("mouse_release")){
			if(!spinning)
				return;
			spinning = false;
			float mouseAngle = Mathf.Atan2 ((mousepos.y - anchorY) - transform.position.y, (mousepos.x-anchorX) - transform.position.x);
			float angleChange = mouseAngle-startingMouseRot;
			transform.eulerAngles = new Vector3(transform.eulerAngles.x,transform.eulerAngles.y,(startingDialRot + angleChange)*Mathf.Rad2Deg);
			float rotation = transform.eulerAngles.z;
			float lockRot = Mathf.Round (rotation / 60) * 60;
			transform.rotation = Quaternion.Euler (0, 0, lockRot);
            GameEvent lockEvent = new GameEvent("dial_locked");
            lockEvent.addArgument(lockRot);
            EventManager.Instance().RaiseEvent(lockEvent);
            Debug.Log("lockRot: " + lockRot);
		}
	}
	public void Update(){
		if(Pause.paused)
			return;
		if(!spinning)
			return;
		Vector3 mousepos = InputWatcher.GetCanvasInputPosition((RectTransform)canvas.transform);
		float mouseAngle = Mathf.Atan2 ((mousepos.y - anchorY) - transform.position.y, (mousepos.x-anchorX) - transform.position.x);
		float angleChange = mouseAngle-startingMouseRot;
		transform.eulerAngles = new Vector3(transform.eulerAngles.x,transform.eulerAngles.y,(startingDialRot + angleChange)*Mathf.Rad2Deg);
	}
	public bool TouchIsOnGunButtons(){
		foreach (GunButton gb in gunButtons){
			if(gb.TouchIsOnMe(InputWatcher.GetCanvasInputPosition((RectTransform)canvas.transform)))
				return true;
		}
		return false;
	}
	public bool IsSpinning(){
		return spinning;
	}
	
}
