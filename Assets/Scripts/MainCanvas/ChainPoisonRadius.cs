using UnityEngine;

public class ChainPoisonRadius : MonoBehaviour{

	float pstrength = 0f;
	float pduration = 0f;

	public void SetRadius(float r){
		GetComponent<CircleCollider2D>().radius = r;
	}
	public void SetStrengthAndDuration(float strength,float duration){
		pstrength = strength;
		pduration = duration;
	}

	void OnTriggerEnter2D(Collider2D coll){
		if(coll.tag == "Enemy"){
			Enemy e = coll.GetComponent<Enemy>();
			e.GetChainPoisoned(pstrength,pduration);
		}
	}
}
