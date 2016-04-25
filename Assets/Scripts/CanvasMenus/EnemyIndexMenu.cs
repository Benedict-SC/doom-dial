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
        Debug.Log("dial null? : " + md == null);
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
        List<string> enemyTypes = new List<string>();
        foreach (System.Object s in enemies)
        {
            enemyTypes.Add((string)s);
        }

        //for each enemy listing in ENEMY_LIST...
        foreach (string enFilename in enemyTypes)
        {
            GameObject optionobj = GameObject.Instantiate(Resources.Load("Prefabs/Menus/MenuOption")) as GameObject;
            MenuOption option = optionobj.GetComponent<MenuOption>();
            GameObject.Destroy(optionobj.transform.FindChild("Image").gameObject);
            option.enemyFilename = enFilename;

            //set the display name of the enemy.  Make sure the player's encountered him!
            long enTimesSeen = 0;

            //here, we get the enemy's specific file so we can access its display name.
            FileLoader enemyfl = new FileLoader("JSONData" + Path.DirectorySeparatorChar + "Bestiary", enFilename);
            string enemyjson = enemyfl.Read();
            Dictionary<string, System.Object> enemyStats = (Dictionary<string, System.Object>)Json.Deserialize(enemyjson);

            //load up stats from the enemy log save file
            FileLoader logfl = FileLoader.GetSaveDataLoader("Bestiary", "bestiary_logging");
            string logjson = logfl.Read();
            Dictionary<string, System.Object> logDict = (Dictionary<string, System.Object>)Json.Deserialize(logjson);
            List<System.Object> logList = logDict["enemyLogs"] as List<System.Object>;
            foreach (System.Object enemy in logList)
            {
                Dictionary<string, System.Object> edata = enemy as Dictionary<string, System.Object>;
                string filename = edata["name"] as string;
                if (filename.Equals(enFilename)) //find the correct enemy corresponding to enFilename
                {
                    enTimesSeen = (long)edata["timesSeen"]; //now we've finally got the timesSeen value :P
                    break;
                }
            }

            if (enTimesSeen > 0) //if not, the default for this is "???"
            {
                option.SetDialText((string)enemyStats["name"]);
            }

            optionobj.transform.SetParent(md.transform, false);
            md.AddOption(option);
        }
    }
}
