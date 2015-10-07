using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class WaveEditorController : MonoBehaviour,EventHandler{

	public static WaveEditorController singleton;
	GameObject canvas;
	ScrollRect sr;
	ZonePanelController[] zonepanels;
	List<EnemyListEntryController>[] zonelists;
	
	bool moving = false;
	EnemyDraggableController floatingEnemy = null;
	EnemyListEntryController floatingEntry = null;
	
	public void Start(){
		singleton = this;
		canvas = GameObject.Find("Canvas");
		sr = GameObject.Find("EnemyScroll").GetComponent<ScrollRect>();
		EventManager.Instance().RegisterForEventType("mouse_release",this);
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
		if(floatingEnemy != null)
			DropEnemy();
		if(floatingEntry != null)
			DropEntry();
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
}