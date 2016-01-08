using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class EnemyShield : MonoBehaviour{

	Vector3 rot = new Vector3(0f,0f,2f);
	List<ShieldFragment> fragments = new List<ShieldFragment>();

	float referenceCapacity = 2; //true max hp, without shred applied
	float capacity = 2; //max hp
	float power = 2; //hp
	float regenRate = 0.02f;
	
	public int frameLastHit = 0;
	public bool hitThisFrame = false;

	public void ConfigureShield(float maxHP,float hp, float regen, float speed, List<System.Object> fragments){
		capacity = maxHP;
		referenceCapacity = maxHP;
		power = hp;
		regenRate = regen;
		rot = new Vector3(0f,0f,speed);
		foreach(System.Object obj in fragments){
			Dictionary<string,System.Object> fragDict = obj as Dictionary<string,System.Object>;
			float fragAngle = (float)(double)fragDict["fragAngle"];
			//fragAngle is the angle in degrees clockwise from the front of the enemy where the center of this fragment is located
			//do NOT use fragAngle to directly set the fragment's angle, which is the leftmost edge of the fragment, not the center
			float fragArc = (float)(double)fragDict["fragArc"]; //width of fragment in degrees
			
			//make it
			GameObject frag = Instantiate (Resources.Load ("Prefabs/MainCanvas/ShieldFragment")) as GameObject;
			ShieldFragment sf = frag.GetComponent<ShieldFragment>();
			sf.SetManager(this);
			frag.transform.localEulerAngles = new Vector3(0f,0f,fragAngle-(fragArc/2f));
			sf.SetArcDegrees(fragArc);
		}
	}
	public void Update(){
		transform.Rotate(rot);
	}
	public void AddFragment(ShieldFragment sf){
		fragments.Add(sf);
	}
	public void GetHitBy(Collider2D collider){
		frameLastHit = Time.frameCount;
		hitThisFrame = true;
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
