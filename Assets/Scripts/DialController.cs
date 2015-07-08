using UnityEngine;
using System.Collections;

public class DialController : MonoBehaviour,EventHandler {

	float maxHealth = 100;
	float health = 100;

	GameObject[] shields = new GameObject[6];

	// Use this for initialization
	void Start () {
		EventManager.Instance ().RegisterForEventType ("enemy_arrived", this);
		GameObject.Find ("Gun1").GetComponent<GunController> ().SetValuesFromJSON ("testtower");
	}

	// Update is called once per frame
	void Update () {
		GameObject healthbar = transform.FindChild ("Health").gameObject;
		healthbar.transform.localScale = new Vector3 (health / maxHealth, health / maxHealth, 1);
	}

	public void HandleEvent(GameEvent ge){
		EnemyController enemy = ((GameObject)ge.args [0]).GetComponent<EnemyController>();
		float rawDamage = enemy.GetDamage ();
		//check if the shield for enemy.GetTrackID()-1 isn't null
		//subtract that damage from shield
		//Destroy(shields[???]); to get rid of shield if it's been depleted 
		//(this will make the reference in the array null)
		//calculate the new damage to the dial if the shield was depleted and apply that damage
		//by changing the line below to subtract the modified damage value
		health -= rawDamage;
		enemy.Die ();
	}
	public void PlaceShield(int id, GameObject shield){
		shields [id] = shield;
	}
}
