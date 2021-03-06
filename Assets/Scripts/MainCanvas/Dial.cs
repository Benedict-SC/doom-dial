﻿using UnityEngine;
using System.Collections.Generic;
using MiniJSON;

public class Dial : MonoBehaviour,EventHandler {

	public static readonly float FULL_LENGTH = 162.9f;
	public static readonly float DIAL_RADIUS = 52.1f;
	public static readonly float TRACK_LENGTH = 110.8f;
	public static readonly float ENEMY_SPAWN_LENGTH = 187f;
	public static readonly float ENEMY_TRACK_LENGTH = 110.8f;//134.9f; extra space to spawn off dial no longer a concern
	
	public float maxHealth = 100.0f;
	public float health = 100.0f;
    GameObject healthBar;
	
	public static float inner_radius = 80f; //inexact - set this value from function for changing ring sizes
	public static float middle_radius = 126f; //inexact
	
	GameObject zoneLines;
	GameObject[] superBars = new GameObject[3];
	public float superPercentage = 0.0f; //percentage between 0 and 1
	float goodLifeBonus = 0.05f;
	float halfLifeFullLifeConsequences = 0.08f;
	float quarterLifeHalfwayToDestruction = 0.1f;
	float tenthLifeBonus = 0.2f;
	float tractorBeamCost = 0.05f;
	
	GameObject[] shields = new GameObject[6];
	public static RectTransform canvasTransform;
	public static RectTransform spawnLayer;
	public static RectTransform underLayer;
	public static RectTransform unmaskedLayer;
	static Dial thisDial;
	
	Dictionary<string,System.Object> bonusWaveDictionary;
    public int escapedEnemyCount = 0;
	int bonusCapacity = 30;
	string mostRecentAttackerFilename = "testenemy";

    List<Gun> guns = new List<Gun>();

    //Vampire Risk stuff
    bool vampireIsOn = false;
    float vampireDrainPerTick = .01f;
	
	void Awake(){
		GamePause.paused = false;
		Pause.paused = false;
		canvasTransform = GameObject.Find("Canvas").GetComponent<RectTransform>();
		spawnLayer = GameObject.Find("SpawnOverDialLayer").GetComponent<RectTransform>();
		underLayer = GameObject.Find("SpawnUnderDialLayer").GetComponent<RectTransform>();
		unmaskedLayer = GameObject.Find ("UnmaskedSpawns").GetComponent<RectTransform>();
        healthBar = transform.Find("Health").gameObject;
        thisDial = this;
	}
	void Start () {
        healthBar = transform.Find("Health").gameObject;

        EventManager.Instance ().RegisterForEventType ("enemy_arrived", this);
		EventManager.Instance ().RegisterForEventType ("dial_damaged", this);
		EventManager.Instance ().RegisterForEventType ("health_leeched",this);
		LoadDialConfigFromJSON (WorldData.dialSelected);
		
		bonusWaveDictionary = new Dictionary<string,System.Object>();
		bonusWaveDictionary.Add("levelID",0L);
		bonusWaveDictionary.Add("waveID",404L);
		bonusWaveDictionary.Add("maxMilliseconds",(long)(bonusCapacity * 1000));
		bonusWaveDictionary.Add("minimumInterval",1000L);
		bonusWaveDictionary.Add("enemies",new List<System.Object>());
		
		zoneLines = GameObject.Find("ZoneLines").gameObject;
		superBars[0] = zoneLines.transform.Find("Super1").gameObject;
		superBars[1] = zoneLines.transform.Find("Super2").gameObject;
		superBars[2] = zoneLines.transform.Find("Super3").gameObject;
		for(int i = 0; i < 3; i++){
			Transform bar = superBars[i].transform;
			bar.localScale = new Vector3(0.0f, bar.localScale.y,bar.localScale.z);
		}

        guns = GetAllGuns();

        if (PlayerPrefsInfo.Int2Bool(PlayerPrefs.GetInt(PlayerPrefsInfo.s_vampire)))
        {
            vampireIsOn = true;
        }
	}
	
	// Update is called once per frame
	void Update () {
		if (health > maxHealth)
		{
			health = maxHealth;
		}

        if (vampireIsOn)
        {
            ChangeHealth(-vampireDrainPerTick);
        }
		
		if (health < 0)
		{
			health = 0.0f;
			Debug.Log ("health is negative@");
			Die();
		}
		healthBar.transform.localScale = new Vector3 (health / maxHealth, health / maxHealth, 1);
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
	public void SpendSuperPercent(float percent){
		superPercentage -= percent;
		if(superPercentage > 1)
			superPercentage = 1.0f;
		else if(superPercentage < 0){
			superPercentage = 0.0f;
		}
		float baseWidth = DIAL_RADIUS / FULL_LENGTH;
		float multiplier = 1-baseWidth;
		for(int i = 0; i < 3; i++){
			GameObject barObj = superBars[i];
			Transform bar = barObj.transform;
			bar.localScale = new Vector3(baseWidth + (multiplier*superPercentage), bar.localScale.y,bar.localScale.z);
		}
	}

	public void OnTriggerEnter2D(Collider2D coll){
        Debug.Log("dial called ontriggerenter2d");
		if(coll.gameObject.tag == "DialCollider"){
			GameEvent ge = new GameEvent("enemy_arrived");
			ge.addArgument(coll.transform.parent.gameObject);
			EventManager.Instance().RaiseEvent(ge);
		}
		if(coll.gameObject.tag == "Boss"){
			Debug.Log ("dial hit boss");
			//check if it's a Skizzard, which deals contact damage
			Skizzard skizz = coll.gameObject.GetComponent<Skizzard>();
			if(skizz != null){
				skizz.HitDial();
			}
		}
        else if (coll.gameObject.tag == "BulletRadial")
        {
            BulletRadial bc = coll.gameObject.GetComponent<BulletRadial>();
            bc.Collide();
        }
	}

	public void HandleEvent(GameEvent ge){
		if (ge.type.Equals("enemy_arrived")) {
						GameObject eh = (GameObject)ge.args [0];
						if (eh == null || eh.Equals (null)) {
								return;
						}
						Enemy enemy = eh.GetComponent<Enemy> ();
						mostRecentAttackerFilename = enemy.GetSrcFileName();
						enemy.AddToBonus ((List<System.Object>)bonusWaveDictionary ["enemies"]);
                        escapedEnemyCount++;
							float edamage = enemy.GetDamage();
							health -= edamage;
							GameEvent hitlog = new GameEvent("enemy_finished");
                            hitlog.addArgument(enemy.GetSrcFileName());
                            hitlog.addArgument(edamage);
                            EventManager.Instance().RaiseEvent(hitlog);
							//Debug.Log ("damage taken, new health is " + health);
						
						enemy.Die ();
		}else if(ge.type.Equals("dial_damaged")){
			GameObject damageSource = (GameObject)ge.args[0];
			float damageAmount = (float)ge.args[1];
			
			health -= damageAmount;
		}else if(ge.type.Equals ("health_leeched")){
			float healAmount = (float)ge.args[0];
			health += healAmount;
			if(health > maxHealth){
				health = maxHealth;
			}
		}
	}
	public void Die(){
		EnemyIndexManager.LogPlayerDeath(mostRecentAttackerFilename);
		GameEvent death = new GameEvent("game_over");
		EventManager.Instance().RaiseEvent(death);
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
        List<System.Object> bonusList = bonusWaveDictionary["enemies"] as List<System.Object>;
        int overflow = bonusList.Count - 30;
        if(overflow > 0) {
            for(int i = 0; i < overflow; i++) {
                bonusList.RemoveAt(0);
            }
        }
		return bonusWaveDictionary;
	}
	public void ClearBonusJSON(){
		bonusWaveDictionary = new Dictionary<string,System.Object>();
		bonusWaveDictionary.Add("levelID",0L);
		bonusWaveDictionary.Add("waveID",404L);
		bonusWaveDictionary.Add("maxMilliseconds",(long)(bonusCapacity * 1000));
		bonusWaveDictionary.Add("minimumInterval",1000L);
		bonusWaveDictionary.Add("enemies",new List<System.Object>());
        escapedEnemyCount = 0;
	}
	public void TractorBeam(){
		
		GameObject[] drops = GameObject.FindGameObjectsWithTag("DroppedPiece");
		if(drops.Length <= 0){
			return;
		}
		
		if(superPercentage < tractorBeamCost){
			Debug.Log ("not enough super!");
			return;
		}else{
			SpendSuperPercent(tractorBeamCost);
		}
		
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
		if (amt > 0) Debug.Log ("Dial health += " + amt);
		//Debug.Log ("health is " + health);
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

    public static List<Gun> GetAllGuns()
    {
        List<Gun> result = new List<Gun>();
        for (int i = 1; i <= 6; i++)
        {
            GameObject g = GameObject.Find("Gun" + i);
            if (g != null)
            {
                result.Add(g.GetComponent<Gun>());
            }
        }
        return result;
    }

    //returns true if all guns HAVE fired
    public bool UseLockGunsHaveAllFired()
    {
        foreach (Gun g in GetAllGuns())
        {
            if (!g.useLockHasFired)
            {
                return false;
            }
        }
        return true;
    }

    public void UseLockResetAllGuns()
    {
        foreach (Gun g in GetAllGuns())
        {
            g.waitingForOtherGunsToFire = false;
            g.useLockHasFired = false;
        }
    }

	public static int LaneFromPosition(GameObject go){
		RectTransform gort = go.GetComponent<RectTransform>();
		float degrees = ((360-Mathf.Atan2(gort.anchoredPosition.y,gort.anchoredPosition.x) * Mathf.Rad2Deg)+90 + 360)%360;
		//Debug.Log(degrees);
		if(degrees >= 60.0 && degrees < 120.0){
			return 2;
		}else if(degrees >= 120.0 && degrees < 180.0){
			return 3;
		}else if(degrees >= 180.0 && degrees < 240.0){
			return 4;
		}else if(degrees >= 240.0 && degrees < 300.0){
			return 5;
		}else if(degrees >= 300.0 && degrees < 360.0){
			return 6;
		}else if(degrees >= 360.0 || degrees < 60.0){
			return 1;
		}else{
			Debug.Log ("What the heck, this shouldn't happen");
			return 0;
		}
	}
	public Dictionary<string,System.Object> GetBonusDict(){
		return bonusWaveDictionary;
	}
	public static void CallEnemyAddBonus(Enemy e){
		e.AddToBonus((List<System.Object>)thisDial.GetBonusDict()["enemies"]);
	}
}
