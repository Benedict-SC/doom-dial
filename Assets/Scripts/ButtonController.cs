using UnityEngine;
using System.Collections;

public class ButtonController : MonoBehaviour, EventHandler {

	public int buttonID;

	GunController gc;

	// Use this for initialization
	void Start () {
		EventManager em = EventManager.Instance ();
		em.RegisterForEventType ("mouse_release",this);
		GameObject overlayObject = transform.Find("CooldownLayer").gameObject;
		overlayObject.transform.localScale = new Vector3 (0, 0, 1);
		gc = GameObject.Find ("Gun" + buttonID).GetComponent<GunController> ();
	}
	
	// Update is called once per frame
	void Update () {
		if (gc.GetCooldown () > 0) {
			float ratio = gc.GetCooldownRatio();
			GameObject overlayObject = transform.Find("CooldownLayer").gameObject;
			overlayObject.transform.localScale = new Vector3 (ratio,ratio, 1);
		}
	}

	public void HandleEvent(GameEvent ge){ //REVISE FOR TOUCH LATER
		Vector3 pos = this.transform.position;
		Vector3 mousepos = (Vector3)ge.args [0];
		Vector3 newmousepos = Camera.main.ScreenToWorldPoint (mousepos);
		newmousepos.z = 0;
		float distance = (newmousepos - pos).magnitude;
		//calculate radius of buttons
		SpriteRenderer s = this.GetComponent<SpriteRenderer> ();
		float radius = s.bounds.size.x/2;


		if (distance < radius) {
			//Debug.Log ("button released on button " + buttonID);
			GameEvent nge = new GameEvent("shot_fired");
			nge.addArgument(buttonID);
			EventManager.Instance().RaiseEvent(nge);
		}
	}
}
