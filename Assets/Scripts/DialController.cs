using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using MiniJSON;

public class DialController : MonoBehaviour,EventHandler {

	public static readonly float FULL_LENGTH = 4.52f;
	public static readonly float DIAL_RADIUS = 1.5f;
	public static readonly float TRACK_LENGTH = 3.02f;

	float maxHealth = 100;
	float health = 100;

	GameObject[] shields = new GameObject[6];

	// Use this for initialization
	void Start () {
		EventManager.Instance ().RegisterForEventType ("enemy_arrived", this);
		LoadDialConfigFromJSON ("testdial");
	}

	// Update is called once per frame
	void Update () {
		GameObject healthbar = transform.FindChild ("Health").gameObject;
		healthbar.transform.localScale = new Vector3 (health / maxHealth, health / maxHealth, 1);
	}

	public void HandleEvent(GameEvent ge){
		EnemyController enemy = ((GameObject)ge.args [0]).GetComponent<EnemyController>();
		float rawDamage = enemy.GetDamage ();
		if (shields[enemy.GetTrackID () - 1] != null) //if this enemy's lane is shielded
		{
			int arrayInd = enemy.GetTrackID () - 1; //index of shield array to reference
			GameObject shield = shields[enemy.GetTrackID () - 1];
			ShieldController sc = shield.GetComponent<ShieldController>();
			float oldHP = sc.hp; //the shield's hp pre-absorbing damage
			Debug.Log ("old shield HP = " + oldHP);
			sc.hp -= rawDamage;
			sc.UpdateHPMeter();
			sc.PrintHP(); //debug
			if (sc.hp <= 0.0f) //if the shield's now dead
			{
				float dialDamage = (oldHP - rawDamage); //this should be a negative value or 0
				health += dialDamage; //dial takes damage (adds the negative value)
				Destroy (shields[arrayInd]); //destroy the shield
				Debug.Log ("shield destroyed");
			}
		}
		else //if there's no shield
		{
			health -= rawDamage;
		}
		enemy.Die ();
	}

	public void LoadDialConfigFromJSON(string filename){
		FileLoader fl = new FileLoader ("JSONData" + Path.DirectorySeparatorChar + "DialConfigs",filename);
		string json = fl.Read ();
		Dictionary<string,System.Object> data = (Dictionary<string,System.Object>)Json.Deserialize (json);

		List<System.Object> entries = data ["towers"] as List<System.Object>;
		for(int i = 0; i < 6; i++) {
			Dictionary<string,System.Object> entry = entries[i] as Dictionary<string,System.Object>;
			string towerfile = entry["filename"] as string;
			bool active = (bool)entry["active"];

			GameObject gun = GameObject.Find ("Gun" + (i+1)).gameObject;
			GunController gc = gun.GetComponent<GunController>();
			gc.SetValuesFromJSON(towerfile);
			gun.SetActive(active);
		}
	}


	public void PlaceShield(int id, GameObject shield){
		shields [id] = shield;
	}
	public bool IsShielded(int ind)
	{
		if (shields[ind] != null)
			return true;
		return false;
	}
	public void DestroyShield(int ind)
	{
		Destroy (shields[ind]);
	}
}
