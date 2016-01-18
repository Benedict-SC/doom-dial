using UnityEngine;
using UnityEngine.UI;

public class BigBulkCollisionPasser : MonoBehaviour{

	public bool shield = false;
	BigBulk parent = null;
	
	void Start(){
		parent = transform.parent.GetComponent<BigBulk>();
	}
	
	void OnTriggerEnter2D(Collider2D coll){
		parent.ReceiveCollisionFromChildCollider(coll,shield);
	}

}