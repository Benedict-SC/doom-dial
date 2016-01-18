using UnityEngine;
using System.Collections.Generic;
using MiniJSON;

public class Dial : MonoBehaviour,EventHandler {

	public static readonly float FULL_LENGTH = 162.9f;
	public static readonly float DIAL_RADIUS = 52.1f;
	public static readonly float TRACK_LENGTH = 110.8f;
	public static readonly float ENEMY_SPAWN_LENGTH = 187f;
	public static readonly float ENEMY_TRACK_LENGTH = 134.9f;
	
	public float maxHealth = 100.0f;
	public float health = 100.0f;
	
	public static float inner_radius = 80f; //inexact - set this value from function for changing ring sizes
	public static float middle_radius = 126f; //inexact
	
	GameObject zoneLines;
	GameObject[] superBars = new GameObject[3];
	public float superPercentage = 0.0f; //percentage between 0 and 1
	float goodLifeBonus = 0.05f;
	float halfLifeFullLifeConsequences = 0.08f;
	float quarterLifeHalfwayToDestruction = 0.1f;
	float tenthLifeBonus = 0.2f;
	
	GameObject[] shields = new GameObject[6];
	public static RectTransform canvasTransform;
	public static RectTransform spawnLayer;
	public static RectTransform underLayer;
	public static RectTransform unmaskedLayer;
	static Dial thisDial;
	
	Dictionary<string,System.Object> bonusWaveDictionary;
	int bonusCapacity = 30;
	
	void Awake(){
		GamePause.paused = false;
		canvasTransform = GameObject.Find("Canvas").GetComponent<RectTransform>();
		spawnLayer = GameObject.Find("SpawnOverDialLayer").GetComponent<RectTransform>();
		underLayer = GameObject.Find("SpawnUnderDialLayer").GetComponent<RectTransform>();
		unmaskedLayer = GameObject.Find ("UnmaskedSpawns").GetComponent<RectTransform>();
		thisDial = this;
	}
	void Start () {
		
		EventManager.Instance ().RegisterForEventType ("enemy_arrived", this);
		EventManager.Instance ().RegisterForEventType ("dial_damaged", this);
		LoadDialConfigFromJSON (WorldData.dialSelected);
		
		bonusWaveDictionary = new Dictionary<string,System.Object>();
		bonusWaveDictionary.Add("levelID",0L);
		bonusWaveDictionary.Add("waveID",404L);
		bonusWaveDictionary.Add("maxMilliseconds",(long)(bonusCapacity * 1000));
		bonusWaveDictionary.Add("minimumInterval",1000L);
		bonusWaveDictionary.Add("enemies",new List<System.Object>());
		
		zoneLines = GameObject.Find("ZoneLines").gameObject;
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
		
		if (health < 0)
		{
			health = 0.0f;
			Debug.Log ("health is negative@");
		}
		GameObject healthbar = transform.FindChild ("Health").gameObject;
		healthbar.transform.localScale = new Vector3 (health / maxHealth, health / maxHealth, 1);
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
		float multiplier = 1-baseWidth;
		
		for(int i = 0; i < 3; i++){
			GameObject barObj = superBars[i];
			Transform bar = barObj.transform;
			bar.localScale = new Vector3(baseWidth + (multiplier*superPercentage), bar.localScale.y,bar.localScale.z);
		}
	}
	
	public void HandleEvent(GameEvent ge){
		if (ge.type.Equals("enemy_arrived")) {
						GameObject eh = (GameObject)ge.args [0];
						if (eh == null || eh.Equals (null)) {
								return;
						}
						Enemy enemy = eh.GetComponent<Enemy> ();
						enemy.AddToBonus ((List<System.Object>)bonusWaveDictionary ["enemies"]);
						float rawDamage = enemy.GetDamage ();
						int trackID = enemy.GetCurrentTrackID ();
						if (shields [trackID - 1] != null) { //if this enemy's lane is shielded
								int arrayInd = trackID - 1; //index of shield array to reference
								GameObject shield = shields [trackID - 1];
								Shield sc = shield.GetComponent<Shield> ();
								float oldHP = sc.hp; //the shield's hp pre-absorbing damage
								Debug.Log ("old shield HP = " + oldHP);
								sc.hp -= rawDamage;
								sc.UpdateHPMeter ();
								sc.PrintHP (); //debug
								if (sc.hp <= 0.0f) { //if the shield's now dead
										float dialDamage = (oldHP - rawDamage); //this should be a negative value or 0
										health += dialDamage; //dial takes damage (adds the negative value)
										Destroy (shields [arrayInd]); //destroy the shield
										Debug.Log ("shield destroyed");
								}
						} else { //if there's no shield
								health -= rawDamage;
								//Debug.Log ("damage taken, new health is " + health);
						}
						enemy.Die ();
		}else if(ge.type.Equals("dial_damaged")){
			GameObject damageSource = (GameObject)ge.args[0];
			float damageAmount = (float)ge.args[1];
			
			health -= damageAmount;
		}
	}
	
	public void LoadDialConfigFromJSON(string filename){
		FileLoader fl = new FileLoader (Application.persistentDataPath,"Dials",filename);
		//FileLoader fl = new FileLoader ("JSONData" + Path.DirectorySeparatorChar + "DialConfigs",filename);
		//Debug.Log("yeah it's this dial");
		string json = fl.Read ();
		Dictionary<string,System.Object> data = (Dictionary<string,System.Object>)Json.Deserialize (json);
		
		List<System.Object> entries = data ["towers"] as List<System.Object>;
		for(int i = 0; i < 6; i++) {
			Dictionary<string,System.Object> entry = entries[i] as Dictionary<string,System.Object>;
			string towerfile = entry["filename"] as string;
			bool active = (bool)entry["active"];
			
			GameObject gun = GameObject.Find ("Gun" + (i+1)).gameObject;
			Gun gc = gun.GetComponent<Gun>();
			//Debug.Log(towerfile);
			gc.SetValuesFromJSON(towerfile);
			gun.SetActive(active);
		}
	}
	public Dictionary<string,System.Object> GetBonusJSON(){
		return bonusWaveDictionary;
	}
	public void ClearBonusJSON(){
		bonusWaveDictionary = new Dictionary<string,System.Object>();
		bonusWaveDictionary.Add("levelID",0L);
		bonusWaveDictionary.Add("waveID",404L);
		bonusWaveDictionary.Add("maxMilliseconds",(long)(bonusCapacity * 1000));
		bonusWaveDictionary.Add("minimumInterval",1000L);
		bonusWaveDictionary.Add("enemies",new List<System.Object>());
	}
	public void TractorBeam(){
		GameObject[] drops = GameObject.FindGameObjectsWithTag("DroppedPiece");
		if(drops.Length <= 0){
			return;
		}
		//don't forget to lower the super percent
		
		//get the subset of super rares
		List<GameObject> superRares = new List<GameObject>();
		foreach(GameObject drop in drops){
			Drop d = drop.GetComponent<Drop>();
			if(d.GetRarity() == 2){
				superRares.Add(drop);
			}
		}
		//subset of rares
		List<GameObject> rares = new List<GameObject>();
		foreach(GameObject drop in drops){
			Drop d = drop.GetComponent<Drop>();
			if(d.GetRarity() == 1){
				rares.Add(drop);
			}
		}
		
		System.Random r = new System.Random ();
		int index = 0;
		GameObject dropTarget = null;
		
		if(superRares.Count > 0){
			double dindex = r.NextDouble() * superRares.Count;
			index = (int)dindex;
			dropTarget = superRares[index];
		}else if(rares.Count > 0){
			double dindex = r.NextDouble() * rares.Count;
			index = (int)dindex;
			dropTarget = rares[index];
		}else{
			double dindex = r.NextDouble() * drops.Length;
			index = (int)dindex;
			dropTarget = drops[index];
		}
		dropTarget.tag = "Untagged";
		
		GameObject tractorBeam = Instantiate (Resources.Load ("Prefabs/MainCanvas/TractorBeam")) as GameObject;
		tractorBeam.transform.SetParent(Dial.underLayer,false);
		tractorBeam.GetComponent<TractorBeam>().SetTarget(dropTarget);
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
	public static List<Enemy> GetAllEnemiesInZone(int zoneID){
		GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
		List<Enemy> zoneOccupants = new List<Enemy>();
		foreach(GameObject go in enemies){
			Enemy e = go.GetComponent<Enemy>();
			if(e.GetCurrentTrackID() == zoneID){
				zoneOccupants.Add(e);
			}
		}
		return zoneOccupants;
	}
	public static List<Enemy> GetAllEnemies(){
		GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
		List<Enemy> zoneOccupants = new List<Enemy>();
		foreach(GameObject go in enemies){
			Enemy e = go.GetComponent<Enemy>();
			zoneOccupants.Add(e);
		}
		return zoneOccupants;
	}
	public static List<Enemy> GetAllShieldedEnemies(){
		GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
		List<Enemy> zoneOccupants = new List<Enemy>();
		foreach(GameObject go in enemies){
			Enemy e = go.GetComponent<Enemy>();
			if(e.GetShield() != null){
				if(!e.IsBeingBulkDrained())
					zoneOccupants.Add(e);
			}
		}
		return zoneOccupants;
	}
	public Dictionary<string,System.Object> GetBonusDict(){
		return bonusWaveDictionary;
	}
	public static void CallEnemyAddBonus(Enemy e){
		e.AddToBonus((List<System.Object>)thisDial.GetBonusDict()["enemies"]);
	}
}
