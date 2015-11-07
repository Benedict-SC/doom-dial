using UnityEngine;
using UnityEngine.UI;

public class EnemyListEntryController : MonoBehaviour,EventHandler{
	
	public ZonePanelController parentZonePanel = null;
	EnemyTemplateController etc = null;
	string label = "/";
	
	public bool floating = false;
	
	Timer holdTimer;
	bool beingHeld = false;
	
	public void Start(){
		holdTimer = new Timer();
		
		EventManager.Instance().RegisterForEventType("mouse_click",this);
		EventManager.Instance().RegisterForEventType("mouse_release",this);
	}
	public void Update(){
		if(floating){
			Vector3 inputPos = InputWatcher.GetInputPosition();
			transform.position = new Vector3(inputPos.x,inputPos.y,-1.0f);
			return;
		}
		if(beingHeld){
			if(!TouchIsOnMe(InputWatcher.GetInputPosition())){
				beingHeld = false;
				return;
			}
			if(holdTimer.TimeElapsedMillis() > 80){
				beingHeld = false;
				WaveEditorController.singleton.AttachEntry(this);
				parentZonePanel.RemoveEntry(this);
				Destroy (this.gameObject);
			}
		}
	}
	public void HandleEvent(GameEvent ge){
		Vector3 pos = (Vector3)ge.args[0];
		if(ge.type.Equals("mouse_click")){
			if(TouchIsOnMe(pos)){
				beingHeld = true;
				holdTimer.Restart();
				WaveEditorController.singleton.activeWaveFrame.Reset();
			}
		}
		if(ge.type.Equals("mouse_release")){
			beingHeld = false;
		}
	}
	public bool TouchIsOnMe(Vector3 touchpos){
		if(WaveEditorController.singleton.IsMoving ()){
			return false;
		}
		RectTransform rt = (RectTransform)transform;
		Vector3 newpoint = rt.InverseTransformPoint(new Vector2(touchpos.x,touchpos.y));
		/*Debug.Log ( "old point is " + touchpos.x + ", " + touchpos.y + "\n" +
					"newpoint is " + newpoint.x + ", " + newpoint.y + "\n" +
					"rect is from " + rt.rect.x + ", " + rt.rect.y + " to " + 
					(rt.rect.x+rt.rect.width) + ", " + (rt.rect.y + rt.rect.height));*/
		bool rectangleOverlap = rt.rect.Contains(newpoint);
		return rectangleOverlap;
	}
	public void ConfigureFromTemplate(EnemyTemplateController etc){
		this.etc = etc;
		label = etc.GetName() + " (" + etc.GetPointValue() +")";
		//set name
		Transform nTransform = transform.FindChild("Text");
		Text nameText = nTransform.gameObject.GetComponent<Text>();
		nameText.text = label;
	}
	
	public EnemyTemplateController GetEnemyTemplate(){
		return etc;
	}
}
