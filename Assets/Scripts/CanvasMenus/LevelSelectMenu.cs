using UnityEngine;
using UnityEngine.UI;
using System.IO;
using MiniJSON;
using System.Collections.Generic;

public class LevelSelectMenu : MonoBehaviour{
	
	MenuDial md;
	public Text buttonText;
	MenuOption selected = null;
	
	public void Awake(){
		md = GameObject.Find("MenuDial").gameObject.GetComponent<MenuDial>();
	}
	public void Start(){
		FillLevelMenu();
	}
	public void Update(){
		
		MenuOption newselected = md.GetSelectedOption();
		if(newselected != selected){
			selected = newselected;
			buttonText.text = selected.GetButtonText();
		}
	}
	public void FillLevelMenu(){
		Debug.Log("JSONData" + Path.DirectorySeparatorChar + "Campaign" + Path.DirectorySeparatorChar + "Worlds/" + WorldData.worldSelected);
		FileLoader fl = new FileLoader ("JSONData" + Path.DirectorySeparatorChar + "Campaign" + Path.DirectorySeparatorChar + "Worlds",WorldData.worldSelected);
		string json = fl.Read ();
		Dictionary<string,System.Object> data = (Dictionary<string,System.Object>)Json.Deserialize (json);
		
		List<System.Object> levels = (List<System.Object>)data["levels"];
		
		foreach(System.Object levelobj in levels){
			Dictionary<string,System.Object> ldata = (Dictionary<string,System.Object>)levelobj;
			
			string levelfile = (string)ldata["filename"];
			string iconlabel = (string)ldata["iconLabel"];
			string iconfile = (string)ldata["icon"];
			string buttonlabel = (string)ldata["button"];
			
			//Debug.Log (iconlabel + " " + iconfile + " " + buttonlabel);
			
			GameObject optionobj = GameObject.Instantiate (Resources.Load ("Prefabs/Menus/MenuOption")) as GameObject;
			GameObject.Destroy(optionobj.GetComponent<MenuOption>());
			LevelMenuOption option = optionobj.AddComponent<LevelMenuOption>() as LevelMenuOption;
			option.levelFilename = levelfile;
			option.ConfigureOption(iconfile,iconlabel,buttonlabel);
			optionobj.transform.SetParent(md.transform,false);
			md.AddOption(option);
		}
		GameObject cancelobj = GameObject.Instantiate (Resources.Load ("Prefabs/Menus/MenuOption")) as GameObject;
		GameObject.Destroy(cancelobj.GetComponent<MenuOption>());
		LoadSceneMenuOption cancel = cancelobj.AddComponent<LoadSceneMenuOption>() as LoadSceneMenuOption;
		cancel.sceneName = "WorldSelectCanvas";
		cancel.ConfigureOption("HPGuageGreen","World Select","Return to world select.");
		cancelobj.transform.SetParent(md.transform,false);
		md.AddOption(cancel);
	}
}
