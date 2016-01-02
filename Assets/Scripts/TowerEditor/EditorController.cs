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
	Image typeButtonImg;
	public static Canvas canvas;
	
	string towerName;
	string decalFilename;
	string towerType;
	
	bool gridloaded = false;
	
	public static bool finger1down = false;
	public static bool finger2down = false;
	public static bool waitingForOtherFingerToReset = false;
	float pieceAngle = 0f;
	float twoFingerAngle = 0f;
	
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
		typeButtonImg = canvas.gameObject.transform.FindChild("TypeButton").FindChild("TypeIcon").GetComponent<Image>();
		//grid.editor = this;
		
		EventManager em = EventManager.Instance();
		em.RegisterForEventType("piece_tapped",this);
		em.RegisterForEventType("template_tapped",this);
		em.RegisterForEventType("piece_dropped_on_inventory",this);
		em.RegisterForEventType ("mouse_click", this);		
		em.RegisterForEventType ("mouse_release", this);
		em.RegisterForEventType ("alt_click", this);		
		em.RegisterForEventType ("alt_release", this);
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
		
		if(finger2down && finger1down){
			Vector3 altClickPos = InputWatcher.GetInputPosition(1);
			Vector3 firstClickPos = InputWatcher.GetInputPosition();
			Vector3 direction = altClickPos-firstClickPos;
			float newAngle = Mathf.Atan2 (direction.y,direction.x) *Mathf.Rad2Deg;
			float diff = newAngle - twoFingerAngle;
			floatingPiece.transform.eulerAngles = new Vector3(0,0,pieceAngle+diff);
		}
	}
	public void ButtonRotate(bool clockwise){
		if(floatingPiece != null)
			floatingPiece.RotateClockwise(clockwise);
	}
	public void LoadTower(string jsonfile){
		grid.LoadTower(jsonfile);
		FileLoader fl = new FileLoader (Application.persistentDataPath,"Towers",jsonfile);
		string json = fl.Read ();
		Dictionary<string,System.Object> data = (Dictionary<string,System.Object>)Json.Deserialize (json);
		
		towerName = (string)data["towerName"];
		decalFilename = (string)data["decalFilename"];
		Debug.Log ("decal name: " + decalFilename);
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
		
		//handle tower type
		string typeIconFile = "PieceNormalDrop";
		if(towerType == "Bullet"){
			typeIconFile = "BulletIconTemp";
		}else if(towerType == "Trap"){
			typeIconFile = "TrapIconTemp";
		}else if(towerType == "Shield"){
			typeIconFile = "ShieldIconTemp";
		}
		Texture2D typeIcon = Resources.Load<Texture2D> ("Sprites/" + typeIconFile);
		typeButtonImg.sprite = UnityEngine.Sprite.Create (
			typeIcon,
			new Rect(0,0,typeIcon.width,typeIcon.height),
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
		}else if(ge.type.Equals("alt_click")){
			if(floatingPiece == null)
				return;
			if(!finger1down){
				return;
			}
				
			Vector3 altClickPos = (Vector3)ge.args[0];
			Vector3 firstClickPos = InputWatcher.GetInputPosition();
			Vector3 direction = altClickPos-firstClickPos;
			twoFingerAngle = Mathf.Atan2 (direction.y,direction.x) *Mathf.Rad2Deg;
			pieceAngle = floatingPiece.transform.eulerAngles.z;
			
			floatingPiece.SetTwoFingerMovement(true);
			finger2down = true;
		}else if(ge.type.Equals("alt_release")){
			Debug.Log ("alt release happened");
			if(floatingPiece == null)
				return;
			
			if(waitingForOtherFingerToReset){
				floatingPiece.SetTwoFingerMovement(false);
				waitingForOtherFingerToReset = false;
			}else{
				waitingForOtherFingerToReset = true;
				floatingPiece.LockRotation();
			}
			finger2down = false;	
			
		}else if(ge.type.Equals("mouse_click")){
			Vector3 pos = (Vector3)ge.args[0];
			if(grid.TouchIsOnMe(pos)){
				finger1down = true;
			}
		}else if(ge.type.Equals("mouse_release")){
			if(floatingPiece == null)
				return;
			if(waitingForOtherFingerToReset){
				floatingPiece.SetTwoFingerMovement(false);
				waitingForOtherFingerToReset = false;
			}else{
				waitingForOtherFingerToReset = true;
				floatingPiece.LockRotation();
			}
			finger1down = false;
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
