using UnityEngine;
using UnityEngine.UI;
using System.IO;
using MiniJSON;
using System.Collections.Generic;

public class WorldSelectMenu : MonoBehaviour{

	MenuDial md;
	public Text buttonText;
	MenuOption selected = null;

	public void Awake(){
		md = GameObject.Find("MenuDial").gameObject.GetComponent<MenuDial>();
	}
	public void Start(){
		FillWorldMenu();
	}
	public void Update(){
	
		MenuOption newselected = md.GetSelectedOption();
		if(newselected != selected){
			selected = newselected;
			buttonText.text = selected.GetButtonText();
		}
	}
	public void FillWorldMenu(){
		FileLoader fl = new FileLoader ("JSONData" + Path.DirectorySeparatorChar + "Campaign","WORLD_LIST");
		string json = fl.Read ();
		Dictionary<string,System.Object> data = (Dictionary<string,System.Object>)Json.Deserialize (json);
		
		List<System.Object> worlds = (List<System.Object>)data["worlds"];
		
		foreach(System.Object worldobj in worlds){
			Dictionary<string,System.Object> wdata = (Dictionary<string,System.Object>)worldobj;
			
			string worldfile = (string)wdata["filename"];
			string iconlabel = (string)wdata["iconLabel"];
			string iconfile = (string)wdata["icon"];
			string buttonlabel = (string)wdata["name"];
			
			//Debug.Log (iconlabel + " " + iconfile + " " + buttonlabel);
			
			GameObject optionobj = GameObject.Instantiate (Resources.Load ("Prefabs/Menus/MenuOption")) as GameObject;
			GameObject.Destroy(optionobj.GetComponent<MenuOption>());
			WorldMenuOption option = optionobj.AddComponent<WorldMenuOption>() as WorldMenuOption;
			option.worldFilename = worldfile;
			option.ConfigureOption(iconfile,iconlabel,buttonlabel);
			optionobj.transform.SetParent(md.transform,false);
			md.AddOption(option);
		}
		GameObject cancelobj = GameObject.Instantiate (Resources.Load ("Prefabs/Menus/MenuOption")) as GameObject;
		GameObject.Destroy(cancelobj.GetComponent<MenuOption>());
		LoadSceneMenuOption cancel = cancelobj.AddComponent<LoadSceneMenuOption>() as LoadSceneMenuOption;
		cancel.sceneName = "MainMenu";
		cancel.ConfigureOption("PieceSprites/Piece_Stun_R","Back to Menu","Return to the main menu.");
		cancelobj.transform.SetParent(md.transform,false);
		md.AddOption(cancel);
	}
}