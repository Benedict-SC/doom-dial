using UnityEngine;
using UnityEngine.UI;
using MiniJSON;
using System.IO;
using System.Collections.Generic;

public class InventoryWindowController : MonoBehaviour{

	private class InventoryRecord{
		public GameObject frame;
		public string pieceFileName;
		public PieceTemplateController template = null;
		public InventoryWindowController parent = null;
		
		readonly float templateX = 135f;
		
		public InventoryRecord(GameObject f,int c, string pfn, InventoryWindowController par){
			parent = par;
			frame = f;
			pieceFileName = pfn;
			GameObject t = Instantiate (Resources.Load ("Prefabs/PieceTemplate")) as GameObject;
			template = t.GetComponent<PieceTemplateController>();
			if(c > 0){
				template.transform.SetParent(frame.transform,true);
				t.transform.SetAsFirstSibling();
				template.transform.localPosition = new Vector3(templateX,0f,0.01f);
				template.ConfigureFromJSON(pfn);
				UpdateDescriptiveText();
				template.SetCount(c);
			}else{
				template = null;
				Destroy (t);
			}
		}
		public void Reconstruct(GameObject f,int c, string pfn){
			if(template.gameObject != null)
				Destroy (template.gameObject);
			frame = f;
			pieceFileName = pfn;
			GameObject t = Instantiate (Resources.Load ("Prefabs/PieceTemplate")) as GameObject;
			template = t.GetComponent<PieceTemplateController>();
			if(c > 0){
				template.transform.SetParent(frame.transform,true);
				t.transform.SetAsFirstSibling();
				template.transform.localPosition = new Vector3(templateX,0f,0.01f);
				template.ConfigureFromJSON(pfn);
				UpdateDescriptiveText();
				template.SetCount(c);
			}else{
				template = null;
				Destroy (t);
			}
			if(f != null && GetCount() > 0){
				SetCount (c);
			}
		}
		public int GetCount(){
			if(template == null)
				return 0;
			else
				return template.GetCount();
		}
		public void SetCount(int count){
			if(template != null){
				template.SetCount(count);
				//Debug.Log(pieceFileName + " count is " + count);
			}else if(count > 0){
				//create a temporary template to hold the thing
				GameObject t = Instantiate (Resources.Load ("Prefabs/PieceTemplate")) as GameObject;
				t.transform.SetAsFirstSibling();
				template = t.GetComponent<PieceTemplateController>();
				template.SetCount(count);
			}
			//don't forget to update the frame's text element for tracking count
			if(frame != null)
				UpdateCountText (count);
		}
		public void UpdateCountText(int count){
			Transform cTransform = frame.transform.FindChild("CountText");
			Text countText = cTransform.gameObject.GetComponent<Text>();
			countText.text = "" + count;
		}
		public void UpdateDescriptiveText(){
			if(parent.towerType.Equals("Bullet")){
				FileLoader fl = new FileLoader ("JSONData" + Path.DirectorySeparatorChar + "Pieces",pieceFileName);
				string json = fl.Read ();
				Dictionary<string,System.Object> data = (Dictionary<string,System.Object>)Json.Deserialize (json);
				
				string bulletText = (string)data["bulletText"];
				Text text = frame.transform.FindChild("StatsText").gameObject.GetComponent<Text>();
				text.text = bulletText;				
			}else{
				Debug.Log ("it ain't bullet");
			}
		}
	}

	static GameObject canvas;
	List<InventoryRecord> fullList;
	public string towerType;
	
	float pieceHeight = 1f;
	
	public void Start(){
		towerType = "Bullet";
		canvas = GameObject.Find("Canvas");
		fullList = new List<InventoryRecord>();
		PopulateInventoryFromJSON();
		
	}
	public void Update(){
	}
	
	public void PopulateInventoryFromJSON(){
		FileLoader fl = new FileLoader (Application.persistentDataPath,"Inventory","inventory");
		//FileLoader fl = new FileLoader ("JSONData" + Path.DirectorySeparatorChar + "MiscData","inventory");
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
			InventoryRecord ir = new InventoryRecord(go,count,piecefile,this);
			if(ir.frame != null){
				ir.SetCount(count);
			}
			
			fullList.Add(ir);
		}
	}
	public void SaveInventory(){
		FileLoader fl = new FileLoader (Application.persistentDataPath,"Inventory","inventory");
		
		Dictionary<string,System.Object> data = new Dictionary<string,System.Object>();
		List<System.Object> pieces = new List<System.Object>();
		for(int i = 0; i < fullList.Count; i++){
			InventoryRecord ir = fullList[i];
			Dictionary<string,System.Object> invObject = new Dictionary<string,System.Object>();
			string filename = ir.pieceFileName;
			int count = ir.GetCount();
			invObject.Add("filename",filename);
			invObject.Add("owned",count);
			pieces.Add(invObject);
		}
		data.Add("pieces",pieces);		
		
		string filedata = Json.Serialize(data);
		fl.Write(filedata);
		Application.LoadLevel("TowerSelect"); //oh man don't put this here, make the loadlevel call another function on the button
	}
	public void AddPiece(PieceController p){
		string fname = p.GetFilename();
		for(int i = 0; i < fullList.Count; i++){
			InventoryRecord ir = fullList[i];
			if(fname.Equals(ir.pieceFileName)){
				//Debug.Log("old count is " + ir.GetCount()); 
				ir.SetCount(ir.GetCount()+1);
				//Debug.Log ("new count is " + ir.GetCount());
				if(ir.GetCount() == 1){
					//Debug.Log ("we just tried refreshing");
					RefreshList();
				}
				break;
			}else{
				//Debug.Log(fname + " doesn't equal " + ir.pieceFileName);
			}
		}
	}
	public void RemovePiece(PieceController p){
		string fname = p.GetFilename();
		for(int i = 0; i < fullList.Count; i++){
			InventoryRecord ir = fullList[i];
			if(fname.Equals(ir.pieceFileName)){
				int oldcount = ir.GetCount();
				if(oldcount <= 0){
					return;
				}else if(oldcount == 1){
					ir.SetCount(ir.GetCount()-1);
					RefreshList();
				}else{
					ir.SetCount(ir.GetCount()-1);
				}
				break;
			}
		}
	}
	private void RefreshList(){
		int nonzeroEntries = 0;
		for(int i = 0; i < fullList.Count; i++){
			int count = fullList[i].GetCount();
			if(count != 0)
				nonzeroEntries++;
		}
		float height = nonzeroEntries * pieceHeight;
	
		RectTransform ownRect = (RectTransform)transform;
		ownRect.sizeDelta = new Vector2(ownRect.rect.size.x,height);
		Vector3 topPoint = ownRect.TransformPoint(new Vector3(ownRect.rect.center.x,ownRect.rect.max.y,transform.position.z));
		
		//clear the existing entries' frames
		foreach(InventoryRecord ir in fullList){
			Destroy (ir.frame);
		}
		
		//add the new ones
		int added = 0;
		for(int i = 0; i < fullList.Count; i++){
			InventoryRecord ir = fullList[i];
			string piecefile = ir.pieceFileName;
			int count = ir.GetCount();
			
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
				ir.Reconstruct(go,count,piecefile);
			}
		}
	}
}
