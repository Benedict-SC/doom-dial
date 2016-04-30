using UnityEngine;

public class ReturnToGameOption : MenuOption{
		
	public override void WhenChosen(){
		Camera.main.GetComponent<CameraToggle>().GoToGame();
	}
}


