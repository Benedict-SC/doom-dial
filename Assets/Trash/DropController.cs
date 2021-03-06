﻿using UnityEngine;
using System.Collections;
using MiniJSON;
using System.Collections.Generic;
using System.IO;

public class DropController : MonoBehaviour, EventHandler {

	int rarity = 0;
	public float superRareChance = 0.1f;
	List<string> pieceTypes;

	// Use this for initialization
	void Start () {
		EventManager.Instance ().RegisterForEventType ("mouse_release", this);
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void HandleEvent(GameEvent ge){ //REVISE FOR TOUCH LATER
		Vector3 pos = this.transform.position;
		Vector3 mousepos = (Vector3)ge.args [0];
		Vector3 newmousepos = mousepos; //Camera.main.ScreenToWorldPoint (mousepos); //handled in InputWatcher now
		newmousepos.z = 0;
		float distance = (newmousepos - pos).magnitude;
		//calculate radius of buttons
		SpriteRenderer s = this.GetComponent<SpriteRenderer> ();
		float radius = s.bounds.size.x/2;
		
		
		if (distance < radius) { //collect piece
			AddPieceToInventory();
			Destroy (this.gameObject); //temporary! we don't have an inventory yet!
		}
	}
	public void SetTypes(string enemyfile){
		FileLoader fl = new FileLoader ("JSONData" + Path.DirectorySeparatorChar + "Bestiary",enemyfile);
		string json = fl.Read ();
		Dictionary<string,System.Object> data = (Dictionary<string,System.Object>)Json.Deserialize (json);
		List<System.Object> castable = data["pieceTypes"] as List<System.Object>;
		pieceTypes = new List<string>();
		foreach(System.Object s in castable){
			pieceTypes.Add((string)s);
		}
	}
	public void MakeRare(){
		System.Random r = new System.Random ();
		if(r.NextDouble() > superRareChance){
			rarity = 1;
			SpriteRenderer img = gameObject.GetComponent<SpriteRenderer> ();
			Texture2D decal = Resources.Load<Texture2D> ("Sprites/" + "PieceRareDrop");
			if (decal == null) {
				Debug.Log("decal is null");
			}
			img.sprite = UnityEngine.Sprite.Create (
				decal,
				new Rect(0,0,decal.width,decal.height),
				new Vector2(0.5f,0.5f),
				img.sprite.rect.width/img.sprite.bounds.size.x);
		}else{
			rarity = 2;
			SpriteRenderer img = gameObject.GetComponent<SpriteRenderer> ();
			Texture2D decal = Resources.Load<Texture2D> ("Sprites/" + "PieceSuperRareDrop");
			if (decal == null) {
				Debug.Log("decal is null");
			}
			img.sprite = UnityEngine.Sprite.Create (
				decal,
				new Rect(0,0,decal.width,decal.height),
				new Vector2(0.5f,0.5f),
				img.sprite.rect.width/img.sprite.bounds.size.x);
		}
	}
	public void AddPieceToInventory(){
		//decide on a piece to get
		string piecetype = "damage"; //placeholder
		string piecerarity = "_normal";
		if(rarity == 0){
			piecerarity = "_normal";
		}else if(rarity == 1){
			piecerarity = "_rare";
		}else if(rarity == 2){
			piecerarity = "_super";
		}
		System.Random r = new System.Random ();
		double dindex = r.NextDouble() * pieceTypes.Count;
		int index = (int)dindex;
		if(index == pieceTypes.Count){
			index--;
		}
		piecetype = pieceTypes[index];
		Debug.Log("type is " + piecetype + piecerarity);
		
		FileLoader fl = new FileLoader (Application.persistentDataPath,"Inventory","inventory");
		string json = fl.Read ();
		Dictionary<string,System.Object> data = (Dictionary<string,System.Object>)Json.Deserialize (json);
		List<System.Object> pieces = data["pieces"] as List<System.Object>;
		foreach(System.Object piece in pieces){
			Dictionary<string,System.Object> pdata = piece as Dictionary<string,System.Object>;
			string filename = pdata["filename"] as string;
			if(filename.Equals(piecetype + piecerarity)){
				int count = (int)(long)pdata["owned"];
				count++;
				pdata["owned"] = (long)count;
				break;
			}
		}
		
		string filedata = Json.Serialize(data);
		fl.Write(filedata);
	}
}

