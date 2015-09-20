using UnityEngine;
using MiniJSON;
using System.Collections.Generic;
using System.IO;

public class RunOnceOnGameStartup : MonoBehaviour{
	public void Start(){
		FillStorageWithTempData();
	}
	public void Update(){
	
	}
	public void FillStorageWithTempData(){
		FileLoader source = new FileLoader ("JSONData" + Path.DirectorySeparatorChar + "MiscData","inventory");
		FileLoader dest = new FileLoader (Application.persistentDataPath,"Inventory","inventory");
		string emptyCheck = dest.Read();
		if(!dest.Equals("ERROR"))
			return;
		string copy = source.Read();
		dest.Write(copy);
	}
}
