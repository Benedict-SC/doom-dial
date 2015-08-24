using UnityEngine;
using UnityEngine.UI;
using MiniJSON;
using System.IO;
using System.Collections.Generic;

public class InventoryWindowController : MonoBehaviour{

	private class InventoryRecord{
		public GameObject frame;
		public int count;
		public string pieceFileName;
		public GameObject template;
		
		public InventoryRecord(GameObject f,int c, string pfn){
			frame = f;
			count = c;
			pieceFileName = pfn;
			if(count > 0){
				template =  Instantiate (Resources.Load ("Prefabs/PieceTemplate")) as GameObject;
				template.transform.SetParent(frame.transform,true);
				template.transform.localPosition = new Vector3(98f,0f,0.01f);
				template.GetComponent<PieceTemplateController>().ConfigureFromJSON(pfn);
			}
		}
	}

	static GameObject canvas;
	List<InventoryRecord> list;
	
	float pieceHeight = 1f;
	
	public void Start(){
		canvas = GameObject.Find("Canvas");
		list = new List<InventoryRecord>();
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
		pieceHeight = calibrationPiece.GetComponent<RectTransform>().rect.size.y;
		int nonzeroEntries = 0;
		for(int i = 0; i < pieces.Count; i++){
			Dictionary<string,System.Object> pObj = pieces[i] as Dictionary<string,System.Object>;
			int count = (int)(long)pObj["owned"];
			if(count != 0)
				nonzeroEntries++;
		}
		float height = nonzeroEntries * pieceHeight;
		Destroy (calibrationPiece);
		RectTransform ownRect = (RectTransform)transform;
		ownRect.sizeDelta = new Vector2(ownRect.rect.size.x,height);
		Vector3 topPoint = ownRect.TransformPoint(new Vector3(ownRect.rect.center.x,ownRect.rect.max.y,transform.position.z));
		
		int added = 0;
		for(int i = 0; i < pieces.Count; i++){
			Dictionary<string,System.Object> pObj = pieces[i] as Dictionary<string,System.Object>;
			string piecefile = (string)pObj["filename"];
			int count = (int)(long)pObj["owned"];
			//do something with those later
			GameObject go = null;
			if(count > 0){
				go = Instantiate (Resources.Load ("Prefabs/InventoryRow")) as GameObject;
				go.transform.SetParent(this.transform,false);
				RectTransform rt = (RectTransform)go.transform;
				Vector3 worldDimensions = rt.TransformVector(rt.rect.size);
				go.transform.position = new Vector3(transform.position.x,
			                                    topPoint.y - (worldDimensions.y/2) - added*worldDimensions.y,
			                                    transform.position.z);
			    added++;
			}
			InventoryRecord ir = new InventoryRecord(go,count,piecefile);
		}
	}
}
