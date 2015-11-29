using UnityEngine;

public class CameraToggle : MonoBehaviour{
	public void Start(){
	
	}
	public void Update(){
	
	}
	public void GoToMenu(){
		transform.position = new Vector3(transform.position.x,transform.position.y,20f);
	}
	public void GoToGame(){
		transform.position = new Vector3(transform.position.x,transform.position.y,-10f);
		GamePause.paused = false;
	}
}
