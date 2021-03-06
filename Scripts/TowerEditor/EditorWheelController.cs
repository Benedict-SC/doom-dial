using UnityEngine;
using UnityEngine.UI;

public class EditorWheelController : MonoBehaviour, EventHandler{

	Canvas c;

	bool spinner = false;
	//Implemented early to allow the player to stop over a button and not accidentally trigger it
	float clickTime = 0;
	bool touchDown = false;
	//Centralized variable for how long the player can hold before it becomes a drag instead of a press
	float clickDelay = 0.1f;
	
	float rotScale = 1.5f; //speeds up or slows down the rotation.
	
	float originalRot = 0.0f; //the angle of the mouse when you start the spin
	float origz = 0.0f; //the angle of the dial when you start the spin
	
	float radius;
	
	// Use this for initialization
	void Start () {
		EventManager em = EventManager.Instance ();
		c = GameObject.Find("Canvas").GetComponent<Canvas>();
		em.RegisterForEventType ("mouse_release", this);
		em.RegisterForEventType ("mouse_click", this);
		
		RectTransform rt = (RectTransform)transform;
		/*
		Vector3 maxCoords = rt.rect.max;// new Vector3(rt.rect.min.x,rt.rect.min.y,0);
		Vector3 worldCoords = Camera.main.ScreenToWorldPoint(maxCoords);
		Vector3 rightCoords = new Vector3(rt.rect.max.x,rt.rect.center.y,0f);
		Vector3 worldCoords2 = Camera.main.ScreenToWorldPoint(rightCoords);
		
		radius = worldCoords.y - worldCoords2.y;*/
		radius = rt.TransformVector(rt.rect.size).x / 2;
		Debug.Log("radius " + radius + " at " + rt.position.x + ", " + rt.position.y);
		
	}
	
	public void HandleEvent(GameEvent ge){
		
		Vector3 mousepos = InputWatcher.GetInputPosition ();
		if (ge.type.Equals ("mouse_release")) {
			//Stops the dial from spinning more
			spinner = false;
			//Only tries to lock if the spinner has a chance of moving
			if(clickTime > clickDelay){
				//Locks position to nearest interval of 60
				float rotation = transform.eulerAngles.z;
				float lockRot = Mathf.Round(rotation /90.0f)*90;
				transform.rotation = Quaternion.Euler(0, 0, lockRot);
				PieceController pc = EditorController.GetFloatingPiece();
				if(pc != null)
					pc.SetRotation(360 - transform.rotation.eulerAngles.z);
			}
			//resets time
			clickTime = 0;
			touchDown = false;
		}else if(ge.type.Equals("mouse_click")){
			//Allows the dial to start spinning
			if(spinner == false){
				float dist = Mathf.Sqrt(((mousepos.x-transform.position.x)*(mousepos.x-transform.position.x))
										+((mousepos.y-transform.position.y)*(mousepos.y-transform.position.y)));
				if(dist > radius)
					return;
				originalRot = Mathf.Atan2(mousepos.y-transform.position.y,mousepos.x-transform.position.x);
				origz = transform.eulerAngles.z;
				//Debug.Log ("new original degrees: " + originalRot);
			}
			spinner = true;
			touchDown = true;
		}
	}
	
	// Update is called once per frame
	void Update () {
		Vector3 mousepos = InputWatcher.GetInputPosition ();
		mousepos = new Vector3(mousepos.x - transform.position.x,mousepos.y-transform.position.y,mousepos.z);
		//Debug.Log (touchDown);
		if(touchDown){
			//Debug.Log ("mouse down");
			clickTime += Time.deltaTime;
			//Only allows the dial to spin if the player has been pressing for over a certain amount of time
			if(spinner && clickTime > clickDelay){
				//Probably not the best for dealing with movement on both axis, 
				//also will change code to touch controls once we start testing the game on mobile
				float angle = Mathf.Atan2(mousepos.y,mousepos.x);// (mousepos.y,mousepos.x);
				float degrees = (Mathf.Rad2Deg * angle);
				float origDegrees = Mathf.Rad2Deg * originalRot;
				transform.rotation = Quaternion.Euler(0,0,(origz + (degrees - origDegrees)*rotScale)%360 );
				//Debug.Log (mousepos.x + ", " + mousepos.y);
				//Debug.Log ("origz: " + origz + " & degrees: " + degrees + " & origDegrees: " + origDegrees + "\n" +mousepos.x + ", " + mousepos.y);
				//Debug.Log ("euler: " + transform.rotation.eulerAngles.z);
				PieceController pc = EditorController.GetFloatingPiece();
				if(pc != null)
					pc.transform.rotation = transform.rotation;
			}
		}
	}
	public bool IsSpinning(){
		return clickTime > clickDelay;
	}

}
