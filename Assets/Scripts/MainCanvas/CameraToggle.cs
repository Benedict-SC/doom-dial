using UnityEngine;

public class CameraToggle : MonoBehaviour{
	Canvas menucanvas;
	public void Start(){
		menucanvas = GameObject.Find("MenuCanvas").GetComponent<Canvas>();
		menucanvas.gameObject.SetActive(false);
	}
	public void Update(){
	
	}
	public void GoToMenu(){
		transform.position = new Vector3(transform.position.x,transform.position.y,87f);
		menucanvas.gameObject.SetActive(true);
	}
	public void GoToGame(){
		transform.position = new Vector3(transform.position.x,transform.position.y,-10f);
		menucanvas.gameObject.SetActive(false);
	}
}
