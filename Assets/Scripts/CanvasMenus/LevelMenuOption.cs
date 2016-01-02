using UnityEngine;

public class LevelMenuOption : MenuOption{
	
	public string levelFilename = "ERROR: LEVEL FILENAME NOT SET";
	
	public override void WhenChosen(){
		WorldData.levelSelected = levelFilename;
		Application.LoadLevel("MainGameCanvas");
	}
}
