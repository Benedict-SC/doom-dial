using UnityEngine;
using MiniJSON;
using System.IO;
using System.Collections.Generic;

public class InventoryWindowController : MonoBehaviour{

	static GameObject canvas;

	//control where the individual panes appear relative to the bottom left corner of...
	//whatever this is attached to
	Vector3 offset;
	
	float scrollPosition = 0f; //between 0 and 1, percentage of the scroll progress down the inner thing
	
	public void Start(){
		canvas = GameObject.Find("Canvas");
		offset = new Vector3(0.0f,0.0f,-1.0f);
		PopulateInventoryFromJSON();
	}
	public void Update(){
	
	}
	
	public void PopulateInventoryFromJSON(){
		FileLoader fl = new FileLoader ("JSONData" + Path.DirectorySeparatorChar + "MiscData","inventory");
		string json = fl.Read ();
		Dictionary<string,System.Object> data = (Dictionary<string,System.Object>)Json.Deserialize (json);
		
		List<System.Object> pieces = data["pieces"] as List<System.Object>;
		for(int i = 0; i < pieces.Count; i++){
			Dictionary<string,System.Object> pObj = pieces[i] as Dictionary<string,System.Object>;
			string piecefile = (string)pObj["filename"];
			int count = (int)(long)pObj["owned"];
			//do something with those later
			GameObject go = Instantiate (Resources.Load ("Prefabs/InventoryRow")) as GameObject;
			go.transform.parent = this.transform;
			RectTransform rt = (RectTransform)transform;
			SpriteRenderer rowBox = go.GetComponent<SpriteRenderer>();
			go.transform.position = new Vector3(transform.position.x + offset.x,
			                                    rt.offsetMin.y + i*rowBox.bounds.size.y,
			                                    transform.position.z + offset.z);
						
		}
	}
}
