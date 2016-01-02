using UnityEngine;

public class WorldMenuOption : MenuOption{

	public string worldFilename = "ERROR: WORLD FILENAME NOT SET";
	
	public override void WhenChosen(){
		WorldData.worldSelected = worldFilename;
		Application.LoadLevel("LevelSelectCanvas");
	}
}