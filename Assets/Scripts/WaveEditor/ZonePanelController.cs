using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class ZonePanelController : MonoBehaviour{
	
	GameObject glow;
	bool glowIsOn = false;
	float spacing = 20f;
	
	float initialpos = 65f;
	
	List<EnemyListEntryController> enemies;
	
	public void Start(){
		glow = transform.FindChild("Glow").gameObject;
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
		elec.transform.SetParent(this.transform,false);
		elec.ConfigureFromTemplate(entry.GetEnemyTemplate());
		RectTransform rt = (RectTransform)elec.transform;
		rt.anchoredPosition = new Vector2(rt.anchoredPosition.x,rt.anchoredPosition.y - spacing*enemies.Count);
		if(enemies.Count == 0){
			initialpos = rt.anchoredPosition.y;
		}
		enemies.Add(elec);
		elec.parentZonePanel = this;
	}
	public void AddNewEntry(EnemyListEntryController entry){
		GameObject go = Instantiate (Resources.Load ("Prefabs/EnemyListEntry")) as GameObject;
		EnemyListEntryController elec = go.GetComponent<EnemyListEntryController>();
		elec.transform.SetParent(this.transform,false);
		elec.ConfigureFromTemplate(entry.GetEnemyTemplate());
		RectTransform rt = (RectTransform)elec.transform;
		rt.anchoredPosition = new Vector2(rt.anchoredPosition.x,rt.anchoredPosition.y - spacing*enemies.Count);
		if(enemies.Count == 0){
			initialpos = rt.anchoredPosition.y;
		}
		enemies.Add(elec);
		elec.parentZonePanel = this;
	}
	public void RemoveEntry(EnemyListEntryController entry){
		enemies.Remove(entry);
		for(int i = 0; i < enemies.Count; i++){
			EnemyListEntryController elec = enemies[i];
			RectTransform rt = (RectTransform)elec.transform;
			rt.anchoredPosition = new Vector2(rt.anchoredPosition.x,initialpos - spacing*i); 
		}
	}
}
