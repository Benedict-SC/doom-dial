using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using MiniJSON;

public class DialController : MonoBehaviour,EventHandler {

	public static readonly float FULL_LENGTH = 4.52f;
	public static readonly float DIAL_RADIUS = 1.5f;
	public static readonly float TRACK_LENGTH = 3.02f;

	public float maxHealth = 100.0f;
	public float health = 100.0f;

	public static float inner_radius = 2.2f; //inexact - set this value from function for changing ring sizes
	public static float middle_radius = 3.5f; //inexact
	
	GameObject[] superBars = new GameObject[3];
	float superPercentage = 0.0f; //percentage between 0 and 1
	float goodLifeBonus = 0.05f;
	float halfLifeFullLifeConsequences = 0.08f;
	float quarterLifeHalfwayToDestruction = 0.1f;
	float tenthLifeBonus = 0.2f;

	GameObject[] shields = new GameObject[6];

	// Use this for initialization
	void Start () {
		EventManager.Instance ().RegisterForEventType ("enemy_arrived", this);
		LoadDialConfigFromJSON ("devdial");
		
		GameObject zoneLines = GameObject.Find("ZoneLines").gameObject;
		superBars[0] = zoneLines.transform.FindChild("Super1").gameObject;
		superBars[1] = zoneLines.transform.FindChild("Super2").gameObject;
		superBars[2] = zoneLines.transform.FindChild("Super3").gameObject;
		for(int i = 0; i < 3; i++){
			Transform bar = superBars[i].transform;
			bar.localScale = new Vector3(0.0f, bar.localScale.y,bar.localScale.z);
		}
	}
	
	// Update is called once per frame
	void Update () {
		if (health > maxHealth)
		{
			health = maxHealth;
		}
		GameObject healthbar = transform.FindChild ("Health").gameObject;
		healthbar.transform.localScale = new Vector3 (health / maxHealth, health / maxHealth, 1);
		if (health < 0)
		{
			health = 0.0f;
			Debug.Log ("health is negative@");
		}
	}
	
	public void IncreaseSuperPercent(){
		float healthPercent = health/maxHealth;
		if(healthPercent > 0.5f){
			superPercentage += goodLifeBonus;
		}else if(healthPercent > 0.25f){
			superPercentage += halfLifeFullLifeConsequences;
		}else if(healthPercent > 0.1f){
			superPercentage += quarterLifeHalfwayToDestruction;
		}else{
			superPercentage += tenthLifeBonus;
		}
		if(superPercentage > 1.0f){
			superPercentage = 1.0f;
		}
		
		float baseWidth = DIAL_RADIUS / FULL_LENGTH;
		float multiplier = 1-baseWidth; //TRACK_LENGTH / FULL_LENGTH;
		
		for(int i = 0; i < 3; i++){
			GameObject barObj = superBars[i];
			Transform bar = barObj.transform;
			bar.localScale = new Vector3(baseWidth + (multiplier*superPercentage), bar.localScale.y,bar.localScale.z);
		}
	}

	public void HandleEvent(GameEvent ge){
		GameObject eh = (GameObject)ge.args[0];
		if(eh == null || eh.Equals(null) ){
			return;
		}
		EnemyController enemy = eh.GetComponent<EnemyController>();
		float rawDamage = enemy.GetDamage ();
		int trackID = enemy.GetCurrentTrackID();
		if (shields[trackID - 1] != null) //if this enemy's lane is shielded
		{
			int arrayInd = trackID - 1; //index of shield array to reference
			GameObject shield = shields[trackID - 1];
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
			//Debug.Log ("damage taken, new health is " + health);
		}
		enemy.Die ();
	}

	public void LoadDialConfigFromJSON(string filename){
		//FileLoader fl = new FileLoader (Application.persistentDataPath,"Dials",filename);
		FileLoader fl = new FileLoader ("JSONData" + Path.DirectorySeparatorChar + "DialConfigs",filename);
		//Debug.Log("yeah it's this dial");
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

	public void ChangeHealth(float amt)
	{
		health += amt;
		if (health > maxHealth)
		{
			health = maxHealth;
		}
		Debug.Log ("Dial health += " + amt);
		Debug.Log ("health is " + health);
	}

}
