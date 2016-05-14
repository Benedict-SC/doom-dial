using UnityEngine;
using System.Collections;
using MiniJSON;
using System.IO;
using System.Collections.Generic;

public class StartupChecker : MonoBehaviour {

    public string newestFileFolder; //folder under the save data location
    public string newestFilename; //if this file exists we know we've got the newest version of the project

	// Use this for initialization
	void Start () {
        FileLoader fl = FileLoader.GetSaveDataLoader(newestFileFolder, newestFilename);
        if (fl.Read() != "ERROR") //if the file exists
        {
            //we've got the newest version! yaaaaay
            Application.LoadLevel("MenuTest");
        }
        else
        {
            Debug.Log("This is not the newest version of the game!");
            //idk do other non-newest version stuff
        }
    }
	
	// Update is called once per frame
	void Update () {
	
	}
}
