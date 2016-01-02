using UnityEngine;

public class LoadSceneMenuOption : MenuOption{
	
	public string sceneName = "DebugLoader";
	
	public override void WhenChosen(){
		Application.LoadLevel(sceneName);
	}
}
