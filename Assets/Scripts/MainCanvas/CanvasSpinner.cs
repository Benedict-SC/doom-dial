using UnityEngine;
using UnityEngine.UI;

public class CanvasSpinner : MonoBehaviour,EventHandler{

	public float anchorX = 0;
	public float anchorY = 0;
	
	Canvas canvas;
	
	bool spinning = false;
	float startingRot;
	GunButton[] gunButtons = new GunButton[6];
	
	public void Start(){
		canvas = GameObject.Find("Canvas").GetComponent<Canvas>();
		EventManager em = EventManager.Instance ();
		em.RegisterForEventType ("mouse_release", this);
		em.RegisterForEventType ("mouse_click", this);
		
		for(int i = 1; i < 7; i++){
			gunButtons[i-1] = GameObject.Find("Button"+i).GetComponent<GunButton>();
		}
	}
	public void HandleEvent(GameEvent ge){
		if(ge.type.Equals("mouse_click")){
			if(TouchIsOnGunButtons()){
				Debug.Log("you touched a gun button");
			}else{
				Debug.Log ("you touched not a gun button");
			}
		}else if(ge.type.Equals("mouse_release")){
		
		}
	}
	public void Update(){
		
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
