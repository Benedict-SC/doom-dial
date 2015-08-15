using UnityEngine;

public class EditorController : MonoBehaviour,EventHandler{

	PieceController floatingPiece = null;
	GridController grid;
	
	public void Start(){
		grid = GameObject.Find("Grid").GetComponent<GridController>();
		grid.editor = this;
		
		EventManager em = EventManager.Instance();
		em.RegisterForEventType("piece_tapped",this);
		
		GameObject go = Instantiate (Resources.Load ("Prefabs/ExistingPiece")) as GameObject;
		floatingPiece = go.GetComponent<PieceController>();
		floatingPiece.ConfigureFromJSON("damage_super");
		floatingPiece.SetRotation(180);
		
	}
	public void Update(){
	
	}
	public void HandleEvent(GameEvent ge){
		if(ge.type.Equals("piece_tapped")){
			PieceController tappedPiece = (PieceController)ge.args[0];
			if(tappedPiece.GetGridLock()){
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
		floatingPiece = p;
		p.SetGridLock(false);
		grid.RemovePiece(p);
	}
	
	public PieceController GetFloatingPiece(){
		return floatingPiece;
	}
}
