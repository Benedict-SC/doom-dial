using UnityEngine;
using UnityEngine.UI;

public class BuildDebugConsole : MonoBehaviour{
	
	static Text debugText = null;
	static Timer expiration;
	
	public void Start(){
		debugText = gameObject.GetComponent<Text>();
		expiration = new Timer();
	}
	public void Update(){
		if(expiration.TimeElapsedSecs() >= 3f){
			debugText.text = "";
		}
	}
	
	public static void Flash(string text){
		if(debugText = null)
			return;
		debugText.text = text;
		expiration.Restart();
	}
	

}
