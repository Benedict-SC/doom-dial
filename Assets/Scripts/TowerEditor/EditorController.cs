using UnityEngine;
using UnityEngine.UI;
using MiniJSON;
using System.IO;
using System.Collections.Generic;

public class EditorController : MonoBehaviour,EventHandler{

	static PieceController floatingPiece = null; //yeah, making this static is cheating, but whatever
	GridController grid;
	//EditorWheelController ewc;
	InventoryWindowController iwc;
	ScrollRect sr;
	InputField nameEntry;
	Image decalButtonImg;
	public static Canvas canvas;
	
	string towerName;
	string decalFilename;
	string towerType;
	
	bool gridloaded = false;
	
	public static RectTransform piecesLayer;
	public static RectTransform overlaysLayer;
	
	void Awake(){
		grid = GameObject.Find("Grid").GetComponent<GridController>();
		//ewc = GameObject.Find("SpinWheel").GetComponent<EditorWheelController>();
		sr = GameObject.Find("InvScroll").GetComponent<ScrollRect>();
		iwc = GameObject.Find("InvContent").GetComponent<InventoryWindowController>();
		canvas = GameObject.Find ("Canvas").GetComponent<Canvas>();
		overlaysLayer = GameObject.Find("Overlays").GetComponent<RectTransform>();
		piecesLayer = GameObject.Find("Pieces").GetComponent<RectTransform>();
	}
	
	public void Start(){
		
		nameEntry = canvas.gameObject.transform.FindChild("NameEntry").GetComponent<InputField>();
		decalButtonImg = canvas.gameObject.transform.FindChild("DecalButton").FindChild("Decal").GetComponent<Image>();
		//grid.editor = this;
		
		EventManager em = EventManager.Instance();
		em.RegisterForEventType("piece_tapped",this);
		em.RegisterForEventType("template_tapped",this);
		em.RegisterForEventType("piece_dropped_on_inventory",this);
		GamePause.paused = false;
		
		//LoadTower("drainpunch");
	}
	public void Update(){
		if(!gridloaded){
			gridloaded = true;
			GameObject loader = GameObject.Find ("NameHolder");
			LoadTower (loader.GetComponent<TowerLoad> ().towerName);
			Destroy (loader);
		}
	
		if(floatingPiece != null && floatingPiece.IsMoving()){
			if(sr.vertical)
				sr.vertical = false;
		}else{
			if(!sr.vertical)
				sr.vertical = true;
		}
	}
	public void LoadTower(string jsonfile){
		grid.LoadTower(jsonfile);
		FileLoader fl = new FileLoader ("JSONData" + Path.DirectorySeparatorChar + "Towers",jsonfile);
		string json = fl.Read ();
		Dictionary<string,System.Object> data = (Dictionary<string,System.Object>)Json.Deserialize (json);
		
		towerName = (string)data["towerName"];
		decalFilename = (string)data["decalFilename"];
		towerType = (string)data["towerType"];
		
		nameEntry.text = towerName;
		Texture2D decal = Resources.Load<Texture2D> ("Sprites/" + decalFilename);
		if (decal == null)
			Debug.Log("decal is null");
		decalButtonImg.sprite = UnityEngine.Sprite.Create (
			decal,
			new Rect(0,0,decal.width,decal.height),
			new Vector2(0.5f,0.5f),
			100f);
	}
	public void HandleEvent(GameEvent ge){
		if(ge.type.Equals("piece_tapped")){
			PieceController tappedPiece = (PieceController)ge.args[0];
			if(tappedPiece.GetGridLock()){
				if(floatingPiece != null){
					bool success = grid.TryAddPiece(floatingPiece);
					if(success){
						floatingPiece.SetGridLock(true);
						floatingPiece = null;
					}
				}
				Activate (tappedPiece);
			}else{
				bool success = grid.TryAddPiece(tappedPiece);
				if(success){
					tappedPiece.SetGridLock(true);
					floatingPiece = null;
				}
			}
		}else if(ge.type.Equals("piece_dropped_on_inventory")){
			PieceController piece = (PieceController)ge.args[0];
			InventoryAdd(piece);
		}else if(ge.type.Equals("template_tapped")){
			PieceTemplateController tappedPiece = (PieceTemplateController)ge.args[0];
			Vector3 loc = (Vector3)ge.args[1];
			if(floatingPiece != null){
				bool success = grid.TryAddPiece(floatingPiece);
				if(success){
					floatingPiece.SetGridLock(true);
					floatingPiece = null;
				}
			}
			
			GameObject go = Instantiate (Resources.Load ("Prefabs/ExistingPiece")) as GameObject;
			go.transform.SetParent(canvas.transform,false);
			go.transform.position = new Vector3(loc.x,loc.y,go.transform.position.z);
			PieceController newPiece = go.GetComponent<PieceController>();
			newPiece.ConfigureFromJSON(tappedPiece.GetFilename());
			newPiece.SetRotation(0);
			newPiece.SetMoving(true);
			Activate(newPiece);
			iwc.RemovePiece(newPiece);
			//tappedPiece.SetCount(tappedPiece.GetCount()-1);
		}	
	}
	public void Activate(PieceController p){
		if(floatingPiece != null){
			InventoryAdd(floatingPiece);
		}
		//p.transform.position = new Vector3(p.transform.position.x,p.transform.position.y,-1f);
		floatingPiece = p;
		p.SetGridLock(false);
		p.transform.SetParent(canvas.transform,false);
		grid.RemovePiece(p);
		//ewc.transform.rotation = floatingPiece.transform.rotation;
	}
	public void InventoryAdd(PieceController p){
		iwc.AddPiece(p);
		Destroy (p.gameObject);
	}
	
	public static PieceController GetFloatingPiece(){
		return floatingPiece;
	}
}
