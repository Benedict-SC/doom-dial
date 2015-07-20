using UnityEngine;
using System.Collections;

public class DropController : MonoBehaviour, EventHandler {

	// Use this for initialization
	void Start () {
		EventManager.Instance ().RegisterForEventType ("mouse_release", this);
	}
	
	// Update is called once per frame
	void Update () {
	
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
		
		
		if (distance < radius) { //collect piece
			Destroy (this.gameObject); //temporary! we don't have an inventory yet!
		}
	}
}
