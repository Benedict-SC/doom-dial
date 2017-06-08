using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using MiniJSON;

public class TowerMenu : MonoBehaviour,EventHandler{
	
	public float anchorX = 0;
	public float anchorY = 0;
	
	Canvas canvas;
	TowerLoad loader;
	GameObject loadingMessage;
	
	bool spinning = false;
	float startingMouseRot;
	float startingDialRot;
	
	int selectedIndex = 0;
	string[] towerFiles = new string[6];
	
	public void Start(){
		
		canvas = GameObject.Find("Canvas").GetComponent<Canvas>();
		loader = GameObject.Find("NameHolder").GetComponent<TowerLoad> ();
		loadingMessage = GameObject.Find ("Loading").gameObject;
		loadingMessage.SetActive(false);
		
		RectTransform rt = (RectTransform)transform;
		EventManager em = EventManager.Instance ();
		em.RegisterForEventType ("mouse_release", this);
		em.RegisterForEventType ("mouse_click", this);
		anchorX = rt.anchoredPosition.x;
		anchorY = rt.anchoredPosition.y;
		
		LoadDial ();
		selectedIndex = 0;
	}
	public void LoadDial(){
		FileLoader fl = new FileLoader (Application.persistentDataPath,"Dials",WorldData.dialSelected);
		//FileLoader fl = new FileLoader ("JSONData" + Path.DirectorySeparatorChar + "DialConfigs",filename);
		//Debug.Log("yeah it's this dial");
		string json = fl.Read ();
		Dictionary<string,System.Object> data = (Dictionary<string,System.Object>)Json.Deserialize (json);
		
		List<System.Object> entries = data ["towers"] as List<System.Object>;
		Transform towers = transform.Find("Towers");
		for(int i = 0; i < 6; i++) {
			Dictionary<string,System.Object> entry = entries[i] as Dictionary<string,System.Object>;
			string towerfile = entry["filename"] as string;
            bool active = (bool)entry["active"];
			towerFiles[i] = towerfile;
			Debug.Log (towerfile);
			
			FileLoader tfl = new FileLoader (Application.persistentDataPath,"Towers",towerfile);
			string tjson = tfl.Read ();
			Dictionary<string,System.Object> tdata = (Dictionary<string,System.Object>)Json.Deserialize (tjson);
			
			string imgfilename = tdata ["decalFilename"] as string;
			Image img = towers.Find ("T" + (i+1)).Find("Decal").gameObject.GetComponent<Image> ();
			//Debug.Log ("Sprites" + Path.DirectorySeparatorChar + imgfilename);
			Texture2D decal = Resources.Load<Texture2D> ("Sprites/" + imgfilename);
			if (decal == null) {
				Debug.Log("decal is null");
			}
			img.sprite = UnityEngine.Sprite.Create (
				decal,
				new Rect(0,0,decal.width,decal.height),
				new Vector2(0.5f,0.5f),
				img.sprite.rect.width/img.sprite.bounds.size.x);
            if (!active) {
                img.color = Color.gray;
            }
		}
	}
	public void HandleEvent(GameEvent ge){
		if(Pause.paused)
			return;
		Vector3 mousepos = InputWatcher.GetCanvasInputPosition((RectTransform)canvas.transform);
		if(ge.type.Equals("mouse_click")){
			if(spinning)
				return;
			
			spinning = true;
			startingDialRot = transform.eulerAngles.z * Mathf.Deg2Rad;
			startingMouseRot = Mathf.Atan2(mousepos.y-anchorY,mousepos.x-anchorX);
		}else if(ge.type.Equals("mouse_release")){
			if(!spinning)
				return;
			spinning = false;
			float mouseAngle = Mathf.Atan2 ((mousepos.y - anchorY) - transform.position.y, (mousepos.x-anchorX) - transform.position.x);
			float angleChange = mouseAngle-startingMouseRot;
			transform.eulerAngles = new Vector3(transform.eulerAngles.x,transform.eulerAngles.y,(startingDialRot + angleChange)*Mathf.Rad2Deg);
			float rotation = transform.eulerAngles.z;
			float lockRot = Mathf.Round (rotation / 60) * 60;
			UpdateSelection(lockRot);
			transform.rotation = Quaternion.Euler (0, 0, lockRot);
		}
	}
	void UpdateSelection(float lockRot){
		float twixtThreeSixty = Rotations.ClipDegrees(lockRot);
		int intDegrees = Convert.ToInt32(twixtThreeSixty); //don't really trust Mathf.Round to cast to an int properly
		intDegrees /= 60; 
		if(intDegrees == 6){intDegrees--;}//is now 0 through 5
		selectedIndex = intDegrees; //don't think i need to offset it?
	}
	public void LoadTower(){
		loadingMessage.SetActive(true);
		loader.towerName = towerFiles[selectedIndex];
		Application.LoadLevel("TowerEditor");
	}
	public void ReturnToMenu(){
		Application.LoadLevel("MainMenu");
	}
	public void Update(){
		if(Pause.paused)
			return;
		if(!spinning)
			return;
		Vector3 mousepos = InputWatcher.GetCanvasInputPosition((RectTransform)canvas.transform);
		float mouseAngle = Mathf.Atan2 ((mousepos.y - anchorY) - transform.position.y, (mousepos.x-anchorX) - transform.position.x);
		float angleChange = mouseAngle-startingMouseRot;
		transform.eulerAngles = new Vector3(transform.eulerAngles.x,transform.eulerAngles.y,(startingDialRot + angleChange)*Mathf.Rad2Deg);
	}
	public bool IsSpinning(){
		return spinning;
	}
	
}

