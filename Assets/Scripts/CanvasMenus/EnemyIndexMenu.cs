﻿/*Thom*/

using UnityEngine;
using UnityEngine.UI;
using System.IO;
using MiniJSON;
using System.Collections.Generic;

public class EnemyIndexMenu : MonoBehaviour {

    MenuDial md;
    MenuOption selected = null;

    public bool debugAllEnemiesAvailable = false;

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
            GameObject.Destroy(optionobj.transform.Find("Image").gameObject);
            GameObject.Destroy(optionobj.transform.Find("Text").gameObject);
            #region TextMaking (creating text from script is apparently pretty complicated actually)
            GameObject newtextobj = new GameObject("EnemyName");
            newtextobj.transform.SetParent(optionobj.transform,false);
            Text newtext = newtextobj.AddComponent<Text>();
            newtext.font = (Font)Resources.GetBuiltinResource(typeof(Font),"Arial.ttf");
            newtext.fontSize = 20;
            newtext.fontStyle = FontStyle.Bold;
            newtext.color = new Color(1f,.906f,.153f);
            newtext.alignment = TextAnchor.MiddleCenter;
            #endregion

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
            option.enemyName = (string)enemyStats["name"];
            if (enTimesSeen > 0 || debugAllEnemiesAvailable) //if not, the default for this is "???"
            {
                newtext.text = (string)enemyStats["name"];
                if (enTimesSeen > 0) option.enemySeen = true;
                else option.enemySeen = false;
            }
            else
            {
                newtext.text = "???";
                option.enemySeen = false;
            }

            optionobj.transform.SetParent(md.transform, false);
            md.AddOption(option);
        }
    }
    public void BackToMainMenu()
    {
        Application.LoadLevel("MainMenu");
    }

    public void SetDebugUnlock(Toggle t)
    {
        debugAllEnemiesAvailable = t.isOn;
        foreach (MenuOption mo in md.options)
        {
            Text optionText = mo.transform.GetComponentInChildren<Text>();
            if (t.isOn)
            {
                optionText.text = mo.enemyName;
            }
            else
            {
                optionText.text = "???";
                if (mo.enemySeen)
                {
                    optionText.text = mo.enemyName;
                }
            }
        }
    }
}
