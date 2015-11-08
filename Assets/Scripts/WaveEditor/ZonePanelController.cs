using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class ZonePanelController : MonoBehaviour{
	
	GameObject glow;
	GameObject scroll; 
	bool glowIsOn = false;
	float spacing = 20f;
	
	float initialpos = 65f;
	
	List<EnemyListEntryController> enemies;
	
	public void Start(){
		glow = transform.FindChild("Glow").gameObject;
		scroll = transform.FindChild("ScrollField").FindChild("ScrollContent").gameObject;
		glow.SetActive(glowIsOn);
	}
	public void Update(){
		if(WaveEditorController.singleton.IsMoving ()){
			if(!glowIsOn && TouchIsOnMe(InputWatcher.GetInputPosition())){
				glowIsOn = true;
				glow.SetActive(glowIsOn);
			}
			if(glowIsOn && !TouchIsOnMe(InputWatcher.GetInputPosition())){
				glowIsOn = false;
				glow.SetActive(glowIsOn);
			}
		}else{
			glowIsOn = false;
			glow.SetActive(glowIsOn);
		}
	}
	public bool TouchIsOnMe(Vector3 touchpos){
		RectTransform rt = (RectTransform)transform;
		Vector3 newpoint = rt.InverseTransformPoint(new Vector2(touchpos.x,touchpos.y));
		bool rectangleOverlap = rt.rect.Contains(newpoint);
		return rectangleOverlap;
	}
	public bool GlowIsOn(){
		return glowIsOn;
	}
	public void SetList(List<EnemyListEntryController> list){
		enemies = list;
	}
	public void AddNewEntry(EnemyDraggableController entry){
		GameObject go = Instantiate (Resources.Load ("Prefabs/EnemyListEntry")) as GameObject;
		EnemyListEntryController elec = go.GetComponent<EnemyListEntryController>();
		elec.transform.SetParent(scroll.transform,false);
		elec.ConfigureFromTemplate(entry.GetEnemyTemplate());
		RectTransform rt = (RectTransform)elec.transform;
		rt.anchoredPosition = new Vector2(rt.anchoredPosition.x,rt.anchoredPosition.y - spacing*enemies.Count);
		if(enemies.Count == 0){
			initialpos = rt.anchoredPosition.y;
		}
		enemies.Add(elec);
		elec.parentZonePanel = this;
		ResizeScroll();
		GameEvent ge = new GameEvent("wave_editor_changed");
		EventManager.Instance().RaiseEvent(ge);
	}
	public void AddNewEntry(EnemyListEntryController entry){
		GameObject go = Instantiate (Resources.Load ("Prefabs/EnemyListEntry")) as GameObject;
		EnemyListEntryController elec = go.GetComponent<EnemyListEntryController>();
		elec.transform.SetParent(scroll.transform,false);
		elec.ConfigureFromTemplate(entry.GetEnemyTemplate());
		RectTransform rt = (RectTransform)elec.transform;
		rt.anchoredPosition = new Vector2(rt.anchoredPosition.x,rt.anchoredPosition.y - spacing*enemies.Count);
		if(enemies.Count == 0){
			initialpos = rt.anchoredPosition.y;
		}
		enemies.Add(elec);
		elec.parentZonePanel = this;
		ResizeScroll();
		GameEvent ge = new GameEvent("wave_editor_changed");
		EventManager.Instance().RaiseEvent(ge);
	}
	public void RemoveEntry(EnemyListEntryController entry){
		enemies.Remove(entry);
		ResizeScroll();
		/*for(int i = 0; i < enemies.Count; i++){
			EnemyListEntryController elec = enemies[i];
			RectTransform rt = (RectTransform)elec.transform;
			rt.anchoredPosition = new Vector2(rt.anchoredPosition.x,initialpos - spacing*i); 
		}*/
		
		GameEvent ge = new GameEvent("wave_editor_changed");
		EventManager.Instance().RaiseEvent(ge);
	}
	void ResizeScroll(){//position assumes you just added one enemy
		Vector2 sizedata = new Vector2(0,0);
		int enemycount = enemies.Count;
		enemycount -= 5;
		if(enemycount <= 0){
			sizedata = new Vector2(160f,0f);
		}else{
			float size = 160f + (enemycount * spacing);
			float position = ((RectTransform)scroll.transform).anchoredPosition.y - (spacing/2);
			sizedata = new Vector2(size,position);
		}
		
		RectTransform scrollRect = (RectTransform)scroll.transform;
		scrollRect.sizeDelta = new Vector2(scrollRect.rect.width,sizedata.x);
		scrollRect.anchoredPosition = new Vector2(scrollRect.anchoredPosition.x,sizedata.y);
		//shift the entries
		float basePosition = (sizedata.x/2)-15;
		for(int i = 0; i < enemies.Count; i++){
			float y = basePosition - spacing*i;
			EnemyListEntryController elec = enemies[i];
			RectTransform elecrt = (RectTransform)elec.gameObject.transform;
			elecrt.anchoredPosition = new Vector2(elecrt.anchoredPosition.x,y);
		}
	}
}
