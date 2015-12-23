using UnityEngine;
using UnityEngine.UI;

public class Blob : Enemy{

	private float blastDamage = 0f;

	public override void Update(){
		base.Update();
		if(progress > 0.5f){
			progress = 0.5f;
			DoTheMoving();
		}
	}
	public override void OnTriggerEnter2D(Collider2D coll){
		base.OnTriggerEnter2D(coll);
		if(coll.gameObject.tag.Equals("Enemy")){
			//absorb the enemy
			Enemy e = coll.gameObject.GetComponent<Enemy>();
			e.canDropPiece = false;
			e.Die();
			radius += 0.02f;
			ScaleEnemy();
			blastDamage += 5;
		}
	}

}
