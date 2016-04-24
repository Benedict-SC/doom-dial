using UnityEngine;
using System.Collections;

public class EnemyIndexOption : MenuOption {

    public string enemyFilename = "WARNING: ENEMY FILENAME NOT SET";

    //enemy stats
    public float enHealth;
    public float enAttack;

    public override void WhenChosen()
    {
        /*
        WorldData.worldSelected = worldFilename; stuff from example, replace!
        Application.LoadLevel("LevelSelectCanvas");
        */
    }
}
