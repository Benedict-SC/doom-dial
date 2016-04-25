using UnityEngine;
using System.Collections;

public class EnemyIndexOption : MenuOption {

    public string enemyFilename = "WARNING: ENEMY FILENAME NOT SET";
    public string displayName = "???";

    public override void WhenChosen()
    {
        /*
        WorldData.worldSelected = worldFilename; stuff from example, replace!
        Application.LoadLevel("LevelSelectCanvas");
        */
    }
}
