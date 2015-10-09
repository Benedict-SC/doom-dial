using UnityEngine;
using MiniJSON;
using System.Collections.Generic;
using System.IO;

public class RunOnceOnGameStartup : MonoBehaviour{
	
	readonly bool OVERWRITE_SAVE = true;

	public void Start(){
		//FillStorageWithTempData(); //turned off since TestDataLoader does the same thing- later, replace this with a call to that
		//FileLoader check = new FileLoader (Application.persistentDataPath,"Inventory","inventory");
		if(OVERWRITE_SAVE)
			TestDataLoader.StaticLoadGameData();
	}
	public void Update(){
	
	}
	public void FillStorageWithTempData(){
		/*FileLoader source = new FileLoader ("JSONData" + Path.DirectorySeparatorChar + "MiscData","inventory");
		FileLoader dest = new FileLoader (Application.persistentDataPath,"Inventory","inventory");
		string emptyCheck = dest.Read();
		if(!dest.Equals("ERROR"))
			return;
		string copy = source.Read();
		dest.Write(copy);*/
	}
}
