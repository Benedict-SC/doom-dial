using UnityEngine;

public class EditorController : MonoBehaviour,EventHandler{

	static PieceController floatingPiece = null; //yeah, making this static is cheating, but whatever
	GridController grid;
	EditorWheelController ewc;
	
	public void Start(){
		grid = GameObject.Find("Grid").GetComponent<GridController>();
		ewc = GameObject.Find("SpinWheel").GetComponent<EditorWheelController>();
		//grid.editor = this;
		
		EventManager em = EventManager.Instance();
		em.RegisterForEventType("piece_tapped",this);
		
		GameObject go = Instantiate (Resources.Load ("Prefabs/ExistingPiece")) as GameObject;
		floatingPiece = go.GetComponent<PieceController>();
		floatingPiece.ConfigureFromJSON("penetration_normal");
		floatingPiece.SetRotation(180);
		
		ewc.transform.rotation = floatingPiece.transform.rotation;
		
	}
	public void Update(){
	
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
		}
	}
	public void Activate(PieceController p){
		if(floatingPiece != null){
			Destroy (floatingPiece.gameObject); //ACTUALLY RETURN TO INVENTORY, DON'T DESTROY
		}
		//p.transform.position = new Vector3(p.transform.position.x,p.transform.position.y,-1f);
		floatingPiece = p;
		p.SetGridLock(false);
		grid.RemovePiece(p);
	}
	
	public static PieceController GetFloatingPiece(){
		return floatingPiece;
	}
}
