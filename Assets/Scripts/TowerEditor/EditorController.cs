using UnityEngine;
using UnityEngine.UI;

public class EditorController : MonoBehaviour,EventHandler{

	static PieceController floatingPiece = null; //yeah, making this static is cheating, but whatever
	GridController grid;
	EditorWheelController ewc;
	InventoryWindowController iwc;
	ScrollRect sr;
	Canvas canvas;
	
	public void Start(){
		grid = GameObject.Find("Grid").GetComponent<GridController>();
		ewc = GameObject.Find("SpinWheel").GetComponent<EditorWheelController>();
		sr = GameObject.Find("InvScroll").GetComponent<ScrollRect>();
		iwc = GameObject.Find("InvContent").GetComponent<InventoryWindowController>();
		canvas = GameObject.Find ("Canvas").GetComponent<Canvas>();
		//grid.editor = this;
		
		EventManager em = EventManager.Instance();
		em.RegisterForEventType("piece_tapped",this);
		em.RegisterForEventType("template_tapped",this);
		em.RegisterForEventType("piece_dropped_on_inventory",this);
		
		/*GameObject go = Instantiate (Resources.Load ("Prefabs/ExistingPiece")) as GameObject;
		go.transform.SetParent(canvas.transform,false);
		go.transform.position = new Vector3(go.transform.position.x-2,go.transform.position.y,go.transform.position.z);
		floatingPiece = go.GetComponent<PieceController>();
		floatingPiece.ConfigureFromJSON("penetration_normal");
		floatingPiece.SetRotation(180);*/
		
		//ewc.transform.rotation = floatingPiece.transform.rotation;
		
	}
	public void Update(){
		if(floatingPiece != null && floatingPiece.IsMoving()){
			if(sr.vertical)
				sr.vertical = false;
		}else{
			if(!sr.vertical)
				sr.vertical = true;
		}
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
		grid.RemovePiece(p);
		ewc.transform.rotation = floatingPiece.transform.rotation;
	}
	public void InventoryAdd(PieceController p){
		iwc.AddPiece(p);
		Destroy (p.gameObject);
	}
	
	public static PieceController GetFloatingPiece(){
		return floatingPiece;
	}
}
