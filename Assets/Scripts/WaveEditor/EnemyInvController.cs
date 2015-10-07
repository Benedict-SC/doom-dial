using UnityEngine;
using UnityEngine.UI;
using MiniJSON;
using System.IO;
using System.Collections.Generic;

public class EnemyInvController : MonoBehaviour{
	public void Start(){
		BuildInventory();
	}
	public void Update(){
	
	}
	public void BuildInventory(){
		FileLoader fl = new FileLoader ("JSONData" + Path.DirectorySeparatorChar + "Bestiary","ENEMY_LIST");
		string json = fl.Read ();
		Dictionary<string,System.Object> data = (Dictionary<string,System.Object>)Json.Deserialize (json);
		
		//figure out how wide the panels are
		GameObject calibrationPanel = Instantiate (Resources.Load ("Prefabs/EnemyPanel")) as GameObject;
		float width = ((RectTransform)calibrationPanel.transform).rect.width;
		width += 3;
		Destroy(calibrationPanel);
		
		//get list of enemies
		List<System.Object> enemies = data["enemies"] as List<System.Object>;
		//set content object to appropriate width
		int count = enemies.Count;
		RectTransform ownRect = (RectTransform)transform;
		if(width * count > ownRect.rect.width)
		ownRect.sizeDelta = new Vector2(width*count + 2,ownRect.rect.height);
		
		//add a panel for each object
		int added = 0;
		foreach (System.Object e in enemies){
			string efilename = (string)e;
			GameObject go = Instantiate (Resources.Load ("Prefabs/EnemyPanel")) as GameObject;
			RectTransform rt = (RectTransform)go.transform;
			go.transform.SetParent(this.transform,false);
			//add enemy template stuff
			GameObject template = Instantiate (Resources.Load ("Prefabs/EnemyTemplate")) as GameObject;
			template.transform.SetParent(go.transform,false);
			template.GetComponent<EnemyTemplateController>().ConfigureFromJSON(efilename);
			RectTransform trt = (RectTransform)template.transform;
			trt.anchoredPosition = new Vector2(0f,-25f);
			
			//set position of panel
			rt.anchoredPosition = new Vector2(ownRect.rect.x + 1 + (width/2) + (added*width),rt.anchoredPosition.y);
			added++;
		}
	}
}
