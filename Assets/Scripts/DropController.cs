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
		Vector3 newmousepos = mousepos; //Camera.main.ScreenToWorldPoint (mousepos); //handled in InputWatcher now
		newmousepos.z = 0;
		float distance = (newmousepos - pos).magnitude;
		//calculate radius of buttons
		SpriteRenderer s = this.GetComponent<SpriteRenderer> ();
		float radius = s.bounds.size.x/2;
		
		
		if (distance < radius) { //collect piece
			Destroy (this.gameObject); //temporary! we don't have an inventory yet!
		}
	}
	public void MakeRare(){
		SpriteRenderer img = gameObject.GetComponent<SpriteRenderer> ();
		Texture2D decal = Resources.Load<Texture2D> ("Sprites/" + "PieceRareDrop");
		if (decal == null) {
			Debug.Log("decal is null");
		}
		img.sprite = UnityEngine.Sprite.Create (
			decal,
			new Rect(0,0,decal.width,decal.height),
			new Vector2(0.5f,0.5f),
			img.sprite.rect.width/img.sprite.bounds.size.x);
	}
}
