using UnityEngine;
using UnityEngine.UI;
using System.IO;
using MiniJSON;
using System.Collections.Generic;

public class EnemyIndexMenu : MonoBehaviour {

    MenuDial md;
    MenuOption selected = null;

    public void Awake()
    {
        md = GameObject.Find("MenuDial").gameObject.GetComponent<MenuDial>();
    }

    public void Start()
    {
        FillEnemyMenu();
    }

    public void Update()
    {

        MenuOption newselected = md.GetSelectedOption();
        if (newselected != selected)
        {
            selected = newselected;
        }
    }

    //rewrite this for the enemy menu
    public void FillEnemyMenu()
    {
        FileLoader fl = new FileLoader("JSONData" + Path.DirectorySeparatorChar + "Bestiary", "ENEMY_LIST");
        string json = fl.Read();
        Dictionary<string, System.Object> enemyData = (Dictionary<string, System.Object>)Json.Deserialize(json);

        List<System.Object> enemies = (List<System.Object>)enemyData["enemies"];

        foreach (System.Object enemyobj in enemies)
        {
            Dictionary<string, System.Object> wdata = (Dictionary<string, System.Object>)enemyobj;

            //Debug.Log (iconlabel + " " + iconfile + " " + buttonlabel);

            GameObject optionobj = GameObject.Instantiate(Resources.Load("Prefabs/Menus/MenuOption")) as GameObject;
            GameObject.Destroy(optionobj.GetComponent<MenuOption>());
            WorldMenuOption option = optionobj.AddComponent<WorldMenuOption>() as WorldMenuOption;
            //option.worldFilename = enemyname;
            optionobj.transform.SetParent(md.transform, false);
            md.AddOption(option);
        }
        GameObject cancelobj = GameObject.Instantiate(Resources.Load("Prefabs/Menus/MenuOption")) as GameObject;
        GameObject.Destroy(cancelobj.GetComponent<MenuOption>());
        LoadSceneMenuOption cancel = cancelobj.AddComponent<LoadSceneMenuOption>() as LoadSceneMenuOption;
        cancel.sceneName = "MenuTest";
        cancel.ConfigureOption("PieceSprites/Piece_Stun_R", "Back to Menu", "Return to the main menu.");
        cancelobj.transform.SetParent(md.transform, false);
        md.AddOption(cancel);
    }
}
