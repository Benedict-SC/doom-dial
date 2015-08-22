using UnityEngine;
using UnityEngine.UI;
using MiniJSON;
using System.IO;
using System.Collections.Generic;

public class InventoryWindowController : MonoBehaviour{

	static GameObject canvas;

	//control where the individual panes appear relative to the bottom left corner of...
	//whatever this is attached to
	Vector3 offset;
	
	//float scrollPosition = 0f; //between 0 and 1, percentage of the scroll progress down the inner thing
	
	public void Start(){
		canvas = GameObject.Find("Canvas");
		PopulateInventoryFromJSON();
	}
	public void Update(){
	
	}
	
	public void PopulateInventoryFromJSON(){
		FileLoader fl = new FileLoader ("JSONData" + Path.DirectorySeparatorChar + "MiscData","inventory");
		string json = fl.Read ();
		Dictionary<string,System.Object> data = (Dictionary<string,System.Object>)Json.Deserialize (json);
		
		List<System.Object> pieces = data["pieces"] as List<System.Object>;
		
		GameObject calibrationPiece = Instantiate (Resources.Load ("Prefabs/InventoryRow")) as GameObject;
		float pieceHeight = calibrationPiece.GetComponent<RectTransform>().rect.size.y;
		float height = pieces.Count * pieceHeight;
		Destroy (calibrationPiece);
		RectTransform ownRect = (RectTransform)transform;
		ownRect.sizeDelta = new Vector2(ownRect.rect.size.x,height);
		Vector3 topPoint = ownRect.TransformPoint(new Vector3(ownRect.rect.center.x,ownRect.rect.max.y,transform.position.z));
		
		for(int i = 0; i < pieces.Count; i++){
			Dictionary<string,System.Object> pObj = pieces[i] as Dictionary<string,System.Object>;
			string piecefile = (string)pObj["filename"];
			int count = (int)(long)pObj["owned"];
			//do something with those later
			GameObject go = Instantiate (Resources.Load ("Prefabs/InventoryRow")) as GameObject;
			go.transform.SetParent(this.transform,false);
			RectTransform rt = (RectTransform)go.transform;
			Vector3 worldDimensions = rt.TransformVector(rt.rect.size);
			go.transform.position = new Vector3(transform.position.x,
			                                    topPoint.y - (worldDimensions.y/2) - i*worldDimensions.y,
			                                    transform.position.z);
						
		}
	}
}
