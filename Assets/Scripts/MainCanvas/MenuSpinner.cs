using UnityEngine;
using UnityEngine.UI;

public class MenuSpinner : MonoBehaviour,EventHandler {
	public float anchorX = 0;
	public float anchorY = 0;
	
	Canvas canvas;
	
	bool spinning = false;
	float startingMouseRot;
	float startingDialRot;
	// Use this for initialization
	void Start () {
		canvas = GameObject.Find("MenuCanvas").GetComponent<Canvas>();
		RectTransform rt = (RectTransform)transform;
		EventManager em = EventManager.Instance ();
		em.RegisterForEventType ("mouse_release", this);
		em.RegisterForEventType ("mouse_click", this);
		anchorX = rt.anchoredPosition.x;
		anchorY = rt.anchoredPosition.y;
	}
	public void HandleEvent(GameEvent ge){
		if(!Pause.paused)
			return;
		Vector3 mousepos = InputWatcher.GetCanvasInputPosition((RectTransform)canvas.transform);
		if(ge.type.Equals("mouse_click")){
			if(spinning){
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
			float lockRot = Mathf.Round (rotation / 30) * 30;
			transform.rotation = Quaternion.Euler (0, 0, lockRot);
		}
	}
	// Update is called once per frame
	void Update () {
		if(!Pause.paused)
			return;
		if(!spinning)
			return;
		Vector3 mousepos = InputWatcher.GetCanvasInputPosition((RectTransform)canvas.transform);
		float mouseAngle = Mathf.Atan2 ((mousepos.y - anchorY) - transform.position.y, (mousepos.x-anchorX) - transform.position.x);
		float angleChange = mouseAngle-startingMouseRot;
		transform.eulerAngles = new Vector3(transform.eulerAngles.x,transform.eulerAngles.y,(startingDialRot + angleChange)*Mathf.Rad2Deg);
	}
	public bool IsSpinning(){
		return spinning;
	}
}
