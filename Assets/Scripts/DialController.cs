using UnityEngine;
using System.Collections;

public class DialController : MonoBehaviour,EventHandler {

	float maxHealth = 100;
	float health = 100;

	// Use this for initialization
	void Start () {
		EventManager.Instance ().RegisterForEventType ("enemy_arrived", this);
	}

	// Update is called once per frame
	void Update () {
		GameObject healthbar = transform.FindChild ("Health").gameObject;
		healthbar.transform.localScale = new Vector3 (health / maxHealth, health / maxHealth, 1);
	}

	public void HandleEvent(GameEvent ge){
		EnemyController enemy = ((GameObject)ge.args [0]).GetComponent<EnemyController>();
		health -= enemy.GetDamage ();
		enemy.Die ();
	}
}
