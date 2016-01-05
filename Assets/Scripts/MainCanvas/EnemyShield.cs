using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class EnemyShield : MonoBehaviour{

	Vector3 rot = new Vector3(0f,0f,2f);
	List<ShieldFragment> fragments = new List<ShieldFragment>();

	float capacity = 2; //max hp
	float power = 2; //hp

	public void Start(){
		GameObject shield1 = Instantiate (Resources.Load ("Prefabs/MainCanvas/ShieldFragment")) as GameObject;
		GameObject shield2 = Instantiate (Resources.Load ("Prefabs/MainCanvas/ShieldFragment")) as GameObject;
		shield1.GetComponent<ShieldFragment>().SetManager(this);
		shield2.GetComponent<ShieldFragment>().SetManager(this);
		
		fragments[0].transform.localEulerAngles = new Vector3(0f,0f,60f);
		fragments[1].transform.localEulerAngles = new Vector3(0f,0f,240f);
		fragments[0].SetArcDegrees(90f);
		
	}
	public void Update(){
		transform.Rotate(rot);
	}
	public void AddFragment(ShieldFragment sf){
		fragments.Add(sf);
	}
	public void GetHitBy(Collider2D collider){
		Debug.Log ("shield hit!");
		if(collider.gameObject.tag == "Bullet"){
			Bullet b = collider.gameObject.GetComponent<Bullet>();
			power -= b.dmg;
			b.Collide();
			if(power <= 0){
				GetBroken();
			}
		}else if(collider.gameObject.tag == "AoE"){
		
		}else if(collider.gameObject.tag == "Trap"){
		
		}
	}
	
	public void GetBroken(){
		Destroy (gameObject);
	}
	
}
