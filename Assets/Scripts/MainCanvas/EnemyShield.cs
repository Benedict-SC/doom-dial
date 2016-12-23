using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class EnemyShield : MonoBehaviour{

	Vector3 rot = new Vector3(0f,0f,2f);
	List<ShieldFragment> fragments = new List<ShieldFragment>();

	float referenceCapacity = 2; //true max hp, without shred applied
	public float capacity = 2; //max hp
	public float power = 2; //hp
	float regenRate = 0.02f;
	
	bool charged = false;
	Timer brokenTimer;
	float breakWait = 2f;
	bool waitingToRecharge = false;
	bool growing = false;
	Timer growTimer;
	float growDur = 2f;
	
	public bool bulked = false;
	public float slowedAmount = 0f;
	public bool leeched = false;
	
	public int frameLastHit = 0;
	public bool hitThisFrame = false;
	
	Enemy parent;
	
	
	public void SetAllShieldHP(float hp){
		referenceCapacity = hp;
		capacity = hp;
		power = hp;
	}
	public void IncreaseAllShieldHP(float hp){
		referenceCapacity += hp;
		capacity += hp;
		power += hp;
	}
	public float GetBaseHP(){
		return referenceCapacity;
	}
	public float GetCurrentHP(){
		return power;
	}

	public void ConfigureShield(float maxHP,float hp, float regen, float speed, List<System.Object> fragments){
		parent = transform.parent.GetComponent<Enemy>();
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
		if(growing){
			float percent = growTimer.TimeElapsedSecs()/growDur;
			if(percent > 1){
				growing = false;
				charged = true;
			}else{
				foreach(ShieldFragment sf in fragments){
					sf.transform.localScale = new Vector3(percent,percent,1f);
				}
			}
		}
		if(waitingToRecharge){
			if(brokenTimer.TimeElapsedSecs() >= breakWait){
				waitingToRecharge = false;
				BeginRegrowth();
			}
		}
	
		transform.Rotate(rot);
		if(power < 0 && charged){
			GetBroken ();
		}else{
			Regenerate ();
		}
	}
	public void GrowShields(){
		growing = true;
		growTimer = new Timer();
	}
	public void MakeStuffRealTinyInPreparationForGrowing(){
		foreach(ShieldFragment sf in fragments){
			sf.transform.localScale = new Vector3(0.01f,0.01f,1f);
		}
	}
	public void Regenerate(){
		if(!leeched){
			if(power >= capacity)
				return;
			float regen = regenRate;
			if(bulked)
				regen *= 2;
			if(slowedAmount > 0)
				regen *= (1f-slowedAmount);
			power += regen;
			if(power > capacity)
				power = capacity;
			RefreshShieldColors();
		}else{//leeched
			float regen = regenRate;
			if(bulked)
				regen *= 2;
			if(slowedAmount > 0)
				regen *= (1f-slowedAmount);
			GameEvent ge = new GameEvent("health_leeched");
			ge.addArgument(regen);
			EventManager.Instance().RaiseEvent(ge);
		}
	}
	public void RefreshShieldColors(){
		float percent = power/referenceCapacity;
		foreach(ShieldFragment sf in fragments){
			Image i = sf.gameObject.GetComponent<Image>();
			i.color = new Color(percent,percent,1f);
		}
	}
	public void AddFragment(ShieldFragment sf){
		fragments.Add(sf);
	}
	public void GetHitBy(Collider2D collider){
		frameLastHit = Time.frameCount;
		hitThisFrame = true;
		//Debug.Log ("shield hit!");
		if(collider.gameObject.tag == "Bullet"){
			Bullet b = collider.gameObject.GetComponent<Bullet>();
			power -= b.dmg;
			//Debug.Log("shield power: " + power + "/" + capacity);
			b.Collide();
			parent.GetStatused(b);
			if(power <= 0){
				GetBroken();
			}
			if(b.penetration > 0){
				float penDamage = b.penetration*b.dmg;
				parent.TakeDamage(penDamage);
			}
			/*if(b.shieldShred > 0){
				float shredDamage = b.shieldShred*b.dmg;
				capacity -= shredDamage;
				if(power > capacity){
					power = capacity;
				}
			}
			if(b.slowsShields != 0){
				SlowRegen(b.slowsShields);
			}*/
		}else if(collider.gameObject.tag == "AoE"){
		
		}else if(collider.gameObject.tag == "Trap"){
		
		}
		RefreshShieldColors();
	}
	public void TakeDamage(float hp){
		power -= hp;
		if(power <= 0){
			power = 0f;
			GetBroken();
		}
	}
	public void GetBroken(){
		charged = false;
		MakeStuffRealTinyInPreparationForGrowing();
		brokenTimer = new Timer();
		waitingToRecharge = true;
	}
	public void BeginRegrowth(){
		GrowShields();
	}
	public float Drain(float hp){ //takes hp and returns how much was successfully taken
		power -= hp;
		if(power < 0){
			hp += power;
		}
		RefreshShieldColors();
		return hp;
	}
	public void SlowRegen(float percent){
		slowedAmount += percent;
		if(slowedAmount > 1)
			slowedAmount = 1f;
	}
	
}
