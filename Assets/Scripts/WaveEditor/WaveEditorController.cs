using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using MiniJSON;

public class WaveEditorController : MonoBehaviour,EventHandler{

	public static WaveEditorController singleton;
	GameObject canvas;
	ScrollRect sr;
	ZonePanelController[] zonepanels;
	List<EnemyListEntryController>[] zonelists;
	
	bool moving = false;
	EnemyDraggableController floatingEnemy = null;
	EnemyListEntryController floatingEntry = null;
	
	int points = 0;
	Text pointText;
	
	public void Start(){
		singleton = this;
		canvas = GameObject.Find("Canvas");
		sr = GameObject.Find("EnemyScroll").GetComponent<ScrollRect>();
		pointText = GameObject.Find ("Points").transform.FindChild("Text").GetComponent<Text>();
		EventManager.Instance().RegisterForEventType("mouse_release",this);
		EventManager.Instance().RegisterForEventType("wave_editor_changed",this);
		zonepanels = new ZonePanelController[6];
		zonelists = new List<EnemyListEntryController>[6];
		for(int i = 0; i < 6; i++){
			string id = "ZonePanel" + i;
			zonepanels[i] = GameObject.Find(id).GetComponent<ZonePanelController>();
			zonelists[i] = new List<EnemyListEntryController>();
			zonepanels[i].SetList(zonelists[i]);
		}
	}
	public void Update(){
		if(moving){
			if(sr.horizontal)
				sr.horizontal = false;
		}else{
			if(!sr.horizontal)
				sr.horizontal = true;
		}
	}
	public void HandleEvent(GameEvent ge){
		if(ge.type.Equals("mouse_release")){
			if(floatingEnemy != null)
				DropEnemy();
			if(floatingEntry != null)
				DropEntry();
		}else if(ge.type.Equals("wave_editor_changed")){
			int pointcount = 0;
			for(int i = 0; i < 6; i++){
				List<EnemyListEntryController> enemies = zonelists[i];
				foreach(EnemyListEntryController elec in enemies){
					pointcount += elec.GetEnemyTemplate().GetPointValue();
				}
			}
			points = pointcount;
			pointText.text = "" + points;
		}
	}
	
	public EnemyDraggableController GetFloatingEnemy(){
		return floatingEnemy;
	}
	public void AttachEnemy(EnemyTemplateController etc){
		moving = true;
		GameObject go = Instantiate (Resources.Load ("Prefabs/DraggableEnemy")) as GameObject;
		go.transform.SetParent(canvas.transform,false);
		EnemyDraggableController edc = go.GetComponent<EnemyDraggableController>();
		edc.ConfigureFromTemplate(etc);
		floatingEnemy = edc;
		//make draggable controller based on etc stats
		//make floatingEnemy that controller and dispose of the old one if applicable
	}
	public void DropEnemy(){
		//figure out which zone panel to drop it on
		for(int i = 0; i < 6; i++){
			if(zonepanels[i].GlowIsOn()){
				zonepanels[i].AddNewEntry(floatingEnemy); 
				break;
			}
		}
	
		moving = false;
		if(floatingEnemy != null)
			Destroy (floatingEnemy.gameObject);
		floatingEnemy = null;
	}
	
	public void AttachEntry(EnemyListEntryController elec){
		moving = true;
		GameObject go = Instantiate (Resources.Load ("Prefabs/EnemyListEntry")) as GameObject;
		go.transform.SetParent(canvas.transform,false);
		EnemyListEntryController nelec = go.GetComponent<EnemyListEntryController>();
		nelec.ConfigureFromTemplate(elec.GetEnemyTemplate());
		floatingEntry = nelec;
		floatingEntry.floating = true;
		Vector3 inputPos = InputWatcher.GetInputPosition();
		floatingEntry.transform.position = new Vector3(inputPos.x,inputPos.y,-1.0f);
	}
	public void DropEntry(){
		for(int i = 0; i < 6; i++){
			if(zonepanels[i].GlowIsOn()){
				zonepanels[i].AddNewEntry(floatingEntry); 
				break;
			}
		}		
		moving = false;
		if(floatingEntry != null)
			Destroy (floatingEntry.gameObject);
		floatingEntry = null;
	}
	public bool IsMoving(){
		return moving;
	}
	
	public string GetWaveJSON(){
		Dictionary<string,System.Object> serialdict = new Dictionary<string,System.Object>();
		serialdict.Add("levelID",1000);
		serialdict.Add("waveID",1000);
		serialdict.Add("maxMilliseconds",30000);
		serialdict.Add("minimumInterval",1000);
		List<System.Object> elist = new List<System.Object>();
		for(int i = 0; i < 6; i++){
			List<EnemyListEntryController> enemies = zonelists[i];
			foreach(EnemyListEntryController elec in enemies){
				Dictionary<string,System.Object> enemy = new Dictionary<string,System.Object>();
				enemy.Add("enemyID",elec.GetEnemyTemplate().GetSrcFileName());
				enemy.Add("trackID",i+1);
				enemy.Add("trackpos",0);
				elist.Add(enemy);
			}
		}
		serialdict.Add("enemies",elist);
		return Json.Serialize(serialdict);
	}
	public void PrintWaveJSON(){
		Debug.Log (GetWaveJSON());
	}
	public void PlayLevel(){
		//write wave to wave file
		FileLoader wavedata = new FileLoader (Application.persistentDataPath,"UserLevels","userwave");
		wavedata.Write(GetWaveJSON());
		//create level dictionary
		Dictionary<string,System.Object> leveldict = new Dictionary<string,System.Object>();
		List<System.Object> waves = new List<System.Object>();
		Dictionary<string,System.Object> waveobj = new Dictionary<string,System.Object>();
		waveobj.Add("wavename","userwave");
		waves.Add(waveobj);
		leveldict.Add("waves",waves);
		//set loader for level
		FileLoader leveldata = new FileLoader (Application.persistentDataPath,"UserLevels","userlevel");
		//write dictionary to loader
		leveldata.Write(Json.Serialize(leveldict));
		//switch scene to MainGame while somehow calling the wavemanager reset thing
		GameObject worlddata = GameObject.Find("WorldData");
		worlddata.GetComponent<WorldData>().loadUserLevel = true;
		Application.LoadLevel("MainGame");
	}
	public void Return(){
		Application.LoadLevel ("MenuTest");
	}
}