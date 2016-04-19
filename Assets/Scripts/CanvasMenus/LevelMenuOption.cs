using UnityEngine;

public class LevelMenuOption : MenuOption{
	
	public string levelFilename = "ERROR: LEVEL FILENAME NOT SET";
	public int levelIndex = -1;
	
	public override void WhenChosen(){
		WorldData.levelSelected = levelFilename;
		WorldData.levelIndex = levelIndex;
		Application.LoadLevel("MainGameCanvas");
	}
}
