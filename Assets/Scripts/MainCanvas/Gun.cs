/*Some Thom, some Duncan*/

using UnityEngine;
using UnityEngine.UI;
using MiniJSON;
using System;
using System.Collections.Generic;

public class Gun : MonoBehaviour,EventHandler{

	Canvas canvas;
	public bool decalSet = false;
	public bool held = false;

	public readonly float TRACK_LENGTH = 110.8f; //hard coded to avoid querying track size all the time
    public readonly float TRAP_RANGE = 0.6f; //default trap range for now.  halfway down the lane
	public readonly float MAX_CHARGE_RADIUS = 6f;
	public readonly float MAX_CHARGE_TIME = 5f;
	// ^^^ RELATIVE TO CENTER
	
	//Tower type: "Bullet", "Trap", "Shield", "BulletTrap", "BulletShield", "TrapShield"
	string towerType;

    //Tech Abilities

    //Generic tower
    float cooldown; //weapon cooldown
    float energyGain; //each successful use gives additional energy
    float comboKey; //chance for a base weapon type combo to occur (?)

    //Bullet only
    float dmg; //Damage dealt per shot

    //Trap only
    float trapUses; //No. of uses a trap has

    //Shield only
    float shieldDurability; //Health of the shield

    //Bullet and BulletTrap
    float charge; //Size of on-hit explosion
	public float mostRecentChargeTime = 0f;
    int split; //Bullets, no. of split bullets.  BT, no. of radial bullets on hit

    //Bullet and BulletShield
    int penetration; //Bullet, penetration.  BS, shield shred.
    float continuousStrength; //Bullet, laser firing time.  BS, pulse duration.

    //Shield and BulletShield
    float reflect; //reflection (?)
    float frequency; //Shield, regen rate.  BS, slow (?)

    //Shield and TrapShield
    float tempDisplace; //Shield, teleport distance.  TS, cooldown.
    float absorb; //Shield, durability.  TS, lifedrain.

    //Trap and BulletTrap
    float AoE; //Trap, AoE size.  PT, range.
    float attraction; //Trap, pull.  PT, homing.

    //Trap and TrapShield
    int duplicate; //Trap, triggers. (?)  TS, zone range.
    float field; //field time (?)

	
	bool chainsPoison;
	float slowsShields;
	float aoeRadiusBonus = 1.0f;
	bool multiSplits;
	int pierces = 0;
	bool leeches;
	
	float shieldHP; //shield max HP
	float shieldRegen; //shield regen rate
    float shieldRegenAmt; //amt per tick of regen
	float shieldRange = 55f; //just so it's not hardcoded
	//***Skill values end here***
	
	/* Another tower attribute
	 * But not passed to bullets
	 */
	
	float baseMaxcool;
	float maxcool;
    Timer cooltimer;
	
	bool shootingV = true; //for use by spread 3 -- are we shooting in a v this turn or not?
	bool isPaused = false;
	public int gunID; 
	
	Image cooldownImg;
	Image chargeImg;
	
	// Use this for initialization
	void Start () {
		canvas = GameObject.Find("Canvas").GetComponent<Canvas>();
		EventManager.Instance ().RegisterForEventType ("shot_fired", this);
		GameObject overlayObject = transform.Find("CooldownLayer").gameObject;
		cooldownImg = overlayObject.GetComponent<Image>();
		cooldownImg.type = Image.Type.Filled;
		cooldownImg.fillMethod = Image.FillMethod.Radial360;
		cooldownImg.fillClockwise = false;
		cooldownImg.fillAmount = 0f;
        cooltimer = new Timer();

		GameObject chargeObject = transform.Find("ChargeLayer").gameObject;
		chargeImg = chargeObject.GetComponent<Image>();
		chargeImg.type = Image.Type.Filled;
		chargeImg.fillMethod = Image.FillMethod.Vertical;
		chargeImg.fillAmount = 0f;
		
		//defaults
		/*towerType = "Shield";
		shieldRange = 1f;
		dmg = 10;
		speed = defaultBulletSpeed;
		range = defaultBulletRange;
		shieldHP = 100;*/
		
        //split bullet limits
		if (split > 4)
			split = 4;
		if (split < 1)
			split = 1;
	}
	
	// Update is called once per frame
	void Update () {
		if (cooldown > 0) {
            float cooltime = cooltimer.TimeElapsedSecs();
            cooldown = maxcool - cooltime;
            if (cooldown < 0) {
                cooldown = 0;
            }
			cooldownImg.fillAmount = GetCooldownRatio();
		}
		if((charge > 0) && held){
			float maxChargeTime = (MAX_CHARGE_TIME/MAX_CHARGE_RADIUS) * charge;
			float chargePercent = mostRecentChargeTime / maxChargeTime;
			if(chargePercent > 1f){
				chargePercent = 1f;
			}
			chargeImg.fillAmount = chargePercent;
			chargeImg.color = new Color(1f,1f-chargePercent,1f-chargePercent);
		}
	}
	#region Firing (make the gun shoot a thing)
	public void Fire(float heldTime){
		if(GamePause.paused)
			return;
		GameEvent nge = new GameEvent ("shot_fired");
		nge.addArgument(gunID);
		nge.addArgument(heldTime);
		EventManager.Instance ().RaiseEvent (nge);
	}
	public void StartCooldown(float heldTime){
        cooltimer.Restart();
		cooldown = maxcool; //start cooldown
		if(continuousStrength > 0){
			cooldown = baseMaxcool + heldTime;
			maxcool = baseMaxcool + heldTime;
		}
	}
	public void Hold(){
		held = true;
	}
	public void Unhold(float time){
		held = false;
		if(continuousStrength > 0 && cooldown <= 0){
			StartCooldown(time);
			chargeImg.fillAmount = 0f;
			chargeImg.color = new Color(1f,1f,1f);
		}
	}
	public void HandleEvent(GameEvent ge){
		
		//various conditions under which bullet shouldn't fire
		if ((int)ge.args [0] != gunID)
			//Debug.Log ("not tower " + gunID);
			return;
		if (cooldown > 0) {
			//Debug.Log ("cooldown > 0");
			return;
		}
		if (GameObject.Find ("Dial").GetComponent<CanvasSpinner> ().IsSpinning ()) {
			Debug.Log ("dial is spinning");
			return;
		}
		if (transform.gameObject.activeSelf != true) {
			Debug.Log ("we're not active");
			return;
		}

        cooltimer.Restart();
		cooldown = maxcool; //start cooldown
		chargeImg.fillAmount = 0f;
		chargeImg.color = new Color(1f,1f,1f);

        //Decide what to do based on tower type ("Bullet", "Trap", "Shield", "BulletTrap", "BulletShield", "TrapShield")
        switch (towerType)
		{
		case "Bullet":
			if(continuousStrength <= 0){
				if(split == 1){
					SpawnBulletI();
				}else{
					SpawnSplitFirer(split);
				}
			}
			break;
		case "Trap":
			SpawnTrap();
			break;
		case "Shield":
			Dial dialCon = GameObject.Find ("Dial").gameObject.GetComponent<Dial>();
			if (dialCon.IsShielded (GetCurrentLaneID() - 1)) //if there's already a shield there
			{
				dialCon.DestroyShield(GetCurrentLaneID() - 1); //destroy that shield
				Debug.Log ("destroyed previous shield");
			}
			GameObject shield = Instantiate (Resources.Load ("Prefabs/MainCanvas/Shield")) as GameObject; //make a shield
            shield.transform.SetParent(Dial.underLayer, false);
			Shield sc = shield.GetComponent<Shield>();
			//make it the type of shield this thing deploys
			ConfigureShield (sc);
			//find your angle
			float shieldOwnangle = this.transform.eulerAngles.z;
			float shieldAngle = (shieldOwnangle +  90) % 360 ;
			shieldAngle *= (float)Math.PI / 180;
			//find where to spawn the shield
			float shieldSpawnRange = shieldRange;
			shieldSpawnRange += 0.5f;
            RectTransform shieldRt = sc.GetComponent<RectTransform>();
            shieldRt.anchoredPosition = new Vector2(shieldSpawnRange*Mathf.Cos(shieldAngle),shieldSpawnRange*Mathf.Sin (shieldAngle));
            Debug.Log("shield y should be " + shieldSpawnRange * Mathf.Sin(shieldAngle));
			shieldRt.rotation = this.gameObject.transform.rotation;
			dialCon.PlaceShield (GetCurrentLaneID() - 1, shield); //mark current lane as shielded (placed in array)
			break;
        case "BulletTrap":
            //BulletTrap behavior
            SpawnProjectileTrap();
            break;
        case "BulletShield":
            //BulletShield behavior
            break;
        case "TrapShield":
            //TrapShield behavior
            break;
        default:
            print("Uh oh, I didn't receive a valid towerType string value!");
            Debug.Log("value is: " + towerType);
            break;
        }
		
	}
	
	void SpawnBulletI()
	{
		for (int i = 1; i <= 1; i++)
		{
			//Debug.Log ("called instantiate bullet");
			GameObject bullet = Instantiate (Resources.Load ("Prefabs/MainCanvas/Bullet")) as GameObject; //make a bullet
			bullet.transform.SetParent(Dial.spawnLayer,false);
			RectTransform bulletRect = (RectTransform)bullet.transform;
			RectTransform rt = GetComponent<RectTransform>();
			Bullet bc = bullet.GetComponent<Bullet>();
			//make it the type of bullet this thing fires
			ConfigureBullet (bc);
			//find your angle
			float ownangle = this.transform.eulerAngles.z;
			float angle = (ownangle +  90) % 360 ;
			angle *= (float)Math.PI / 180;
			//Debug.Log ("original angle: " + angle);
			angle = (angle - (float)Math.PI / 6f) + ((((float)Math.PI / 3f) / (2)) * i); //handles spread effect
			//find where to spawn the bullet
			float gunDistFromCenter = (float)Math.Sqrt (rt.anchoredPosition.x*rt.anchoredPosition.x + rt.anchoredPosition.y*rt.anchoredPosition.y);
			gunDistFromCenter += 0.47f;
			bc.spawnx = gunDistFromCenter * (float)Math.Cos (angle);
			bc.spawny = gunDistFromCenter * (float)Math.Sin (angle);
			bc.UpdateSpawnDist();
			//Debug.Log (bc.speed);
			bulletRect.anchoredPosition = new Vector2(bc.spawnx,bc.spawny);
			bc.transform.rotation = transform.rotation;
			bc.vx = bc.speed * (float)Math.Cos(angle);
			bc.vy = bc.speed * (float)Math.Sin(angle);
		}
	}
	void SpawnSplitFirer(int times){
		GameObject firer = Instantiate (Resources.Load ("Prefabs/RectTransform")) as GameObject;
		firer.transform.SetParent(Dial.spawnLayer,false);
		SplitFirer sf = firer.AddComponent<SplitFirer>();
		Vector2 pos = GetComponent<RectTransform>().anchoredPosition;
		firer.GetComponent<RectTransform>().anchoredPosition = pos;

		//create bullet template
		GameObject bullet = Instantiate (Resources.Load ("Prefabs/MainCanvas/Bullet")) as GameObject; //make a bullet
		Bullet bc = bullet.GetComponent<Bullet>();
		ConfigureBullet (bc);
		bullet.SetActive(false);

		sf.transform.rotation = transform.rotation;
		sf.Configure(bc,split,this.transform.eulerAngles.z);
	}
	public GameObject SpawnLasers(){
		GameObject allLasersBase = new GameObject();
		allLasersBase.transform.SetParent(Dial.spawnLayer,false);

		//do once for each split laser
		for(int i = 0; i < split; i++){
			float increment = 60f/(split+1);
			float angle = -30f + (increment*(i+1));
			GameObject laserBase = Instantiate (Resources.Load ("Prefabs/MainCanvas/Laser")) as GameObject;
			GameObject laser = laserBase.transform.Find("Laser").gameObject;
			Laser l = laser.GetComponent<Laser>();
			ConfigureLaser(l);
			laserBase.transform.SetParent(allLasersBase.transform,false);
			laserBase.transform.eulerAngles = new Vector3(0f,0f,angle);
		}

		allLasersBase.transform.rotation = transform.rotation;

		return allLasersBase;
	}
	void SpawnTrap(){
		//Debug.Log ("range is " + range);
		float spawnRadius = Dial.TRACK_LENGTH * TRAP_RANGE;
		float longRadius = Dial.TRACK_LENGTH * (TRAP_RANGE + 0.2f);
		float shortRadius = Dial.TRACK_LENGTH * (TRAP_RANGE - 0.2f);
		//find your angle
		float ownangle = this.transform.eulerAngles.z;
		float angle = (ownangle +  90) % 360 ;
		
		float leftAngle = angle + 15f;
		float rightAngle = angle - 15f;
		angle *= Mathf.Deg2Rad;
		leftAngle *= Mathf.Deg2Rad;
		rightAngle *= Mathf.Deg2Rad;
		
		GameObject trap = Instantiate (Resources.Load ("Prefabs/MainCanvas/Trap")) as GameObject;
		trap.transform.SetParent(Dial.spawnLayer,false);
		RectTransform trapRect = (RectTransform)trap.transform;
		RectTransform rt = (RectTransform)transform;
		Trap tc = trap.GetComponent<Trap>();
		//make it the type of trap this thing fires
		ConfigureTrap (tc);

		//find where to spawn the trap
		Vector2[] spawnPoints = new Vector2[5]; //set up potential trap locations
		float gunDistFromCenter = Dial.DIAL_RADIUS;
		spawnPoints[0] = new Vector2((gunDistFromCenter + spawnRadius) * (float)Math.Cos (angle),(gunDistFromCenter + spawnRadius) * (float)Math.Sin (angle));
		spawnPoints[1] = new Vector2((gunDistFromCenter + shortRadius) * (float)Math.Cos (leftAngle),(gunDistFromCenter + shortRadius) * (float)Math.Sin (leftAngle));
		spawnPoints[2] = new Vector2((gunDistFromCenter + shortRadius) * (float)Math.Cos (rightAngle),(gunDistFromCenter + shortRadius) * (float)Math.Sin (rightAngle));
		spawnPoints[3] = new Vector2((gunDistFromCenter + longRadius) * (float)Math.Cos (leftAngle),(gunDistFromCenter + longRadius) * (float)Math.Sin (leftAngle));
		spawnPoints[4] = new Vector2((gunDistFromCenter + longRadius) * (float)Math.Cos (rightAngle),(gunDistFromCenter + longRadius) * (float)Math.Sin (rightAngle));

		//create a collision checker
		GameObject spawnScanner = Instantiate (Resources.Load ("Prefabs/RectTransform")) as GameObject;
		spawnScanner.transform.SetParent(Dial.spawnLayer,false);
		CircleCollider2D focus = spawnScanner.AddComponent<CircleCollider2D>() as CircleCollider2D;
		RectTransform scanRect = spawnScanner.GetComponent<RectTransform>();
		focus.radius = 3f;
		//move it around and see if there's traps in place
		int spot = 0;
		for(int i = 0; i < 5; i++){
			Vector2 loc = spawnPoints[i];
			scanRect.anchoredPosition = loc;
			//check if there's an opening there			
			Collider2D[] stuffHit = new Collider2D[10];
            ContactFilter2D filter = new ContactFilter2D();
            filter.NoFilter();
            focus.OverlapCollider(filter,stuffHit); //fill array with all colliders intersecting scanner
            List<Collider2D> fieldHit = new List<Collider2D>();
            for(int j = 0; j < stuffHit.Length; j++){ //filter out anything that doesn't get damaged by the field
                Collider2D coll = stuffHit[j];
                if(coll != null && ((coll.gameObject.tag == "Trap" ) || (coll.gameObject.tag == "ProjectileTrap"))){
                    fieldHit.Add(coll);
                }
            }
			if(fieldHit.Count <= 0){
				spot = i;
				break;
			}
		}
		Destroy(spawnScanner);

		trapRect.anchoredPosition = spawnPoints[spot];
		tc.transform.rotation = transform.rotation;
			
	}

    //spawns a single projectiletrap in the center of the lane
    void SpawnProjectileTrap()
    {
        //Debug.Log ("range is " + range);
        float spawnRadius = Dial.TRACK_LENGTH * TRAP_RANGE;
        float longRadius = Dial.TRACK_LENGTH * (TRAP_RANGE + 0.2f);
        float shortRadius = Dial.TRACK_LENGTH * (TRAP_RANGE - 0.2f);
        //find your angle
        float ownangle = this.transform.eulerAngles.z;
        float angle = (ownangle + 90) % 360;

        float leftAngle = angle + 15f;
        float rightAngle = angle - 15f;
        angle *= Mathf.Deg2Rad;
        leftAngle *= Mathf.Deg2Rad;
        rightAngle *= Mathf.Deg2Rad;

        GameObject trap = Instantiate(Resources.Load("Prefabs/MainCanvas/ProjectileTrap")) as GameObject;
        trap.transform.SetParent(Dial.spawnLayer, false);
        RectTransform trapRect = (RectTransform)trap.transform;
        RectTransform rt = (RectTransform)transform;
        ProjectileTrap tc = trap.GetComponent<ProjectileTrap>();
        //make it the type of trap this thing fires
        ConfigureProjectileTrap(tc);

        //find where to spawn the trap
        Vector2[] spawnPoints = new Vector2[1]; //set up potential trap locations
        float gunDistFromCenter = Dial.DIAL_RADIUS;
        spawnPoints[0] = new Vector2((gunDistFromCenter + spawnRadius) * (float)Math.Cos(angle), (gunDistFromCenter + spawnRadius) * (float)Math.Sin(angle));

        //create a collision checker
        GameObject spawnScanner = Instantiate(Resources.Load("Prefabs/RectTransform")) as GameObject;
        spawnScanner.transform.SetParent(Dial.spawnLayer, false);
        CircleCollider2D focus = spawnScanner.AddComponent<CircleCollider2D>() as CircleCollider2D;
        RectTransform scanRect = spawnScanner.GetComponent<RectTransform>();
        focus.radius = 3f;
        //move it around and see if there's traps in place
        int spot = 0;
        for (int i = 0; i < 1; i++)
        {
            Vector2 loc = spawnPoints[i];
            scanRect.anchoredPosition = loc;
            //check if there's an opening there			
            Collider2D[] stuffHit = new Collider2D[10];
            ContactFilter2D filter = new ContactFilter2D();
            filter.NoFilter();
            focus.OverlapCollider(filter, stuffHit); //fill array with all colliders intersecting scanner
            List<Collider2D> fieldHit = new List<Collider2D>();
            for (int j = 0; j < stuffHit.Length; j++)
            { //filter out anything that doesn't get damaged by the field
                Collider2D coll = stuffHit[j];
                if (coll != null && ((coll.gameObject.tag == "Trap") || (coll.gameObject.tag == "ProjectileTrap")))
                {
                    fieldHit.Add(coll);
                }
            }
            if (fieldHit.Count <= 0)
            {
                spot = i;
                break;
            }
        }
        Destroy(spawnScanner);

        trapRect.anchoredPosition = spawnPoints[spot];
        tc.transform.rotation = transform.rotation;
    }
	
	#endregion
	
	#region Setup (get the gun ready to do stuff)
	public void SetValuesFromJSON(string filename){
		//FileLoader fl = new FileLoader ("JSONData" + Path.DirectorySeparatorChar + "Towers",filename);
		FileLoader fl = new FileLoader (Application.persistentDataPath,"Towers",filename);
		string json = fl.Read ();
		Dictionary<string,System.Object> data = (Dictionary<string,System.Object>)Json.Deserialize (json);
		
		string imgfilename = data ["decalFilename"] as string;
		Image img = transform.Find("Label").gameObject.GetComponent<Image> ();
		//Debug.Log ("Sprites" + Path.DirectorySeparatorChar + imgfilename);
		Texture2D decal = Resources.Load<Texture2D> ("Sprites/" + imgfilename);
		if (decal == null) {
			Debug.Log("decal is null");
		}
		img.sprite = UnityEngine.Sprite.Create (
			decal,
			new Rect(0,0,decal.width,decal.height),
			new Vector2(0.5f,0.5f),
			img.sprite.rect.width/img.sprite.bounds.size.x);
		decalSet = true;
		
		towerType = data ["towerType"] as string;
		//Debug.Log (filename + " tower type is " + towerType);
		
		//if(filename.Equals("piecetower")){
		//	PieceParser.FillController(this,filename);
		//	return;
		//}
		PieceParser.FillController(this,filename);
		/*
		cooldownFactor = (float)(double)data["cooldownFactor"];
		maxcool = DEF_COOLDOWN * cooldownFactor;
		dmg = (float)(double)data ["dmg"];
		speed = (float)(double)data ["speed"];
		range = (float)(double)data ["range"];
		knockback = (float)(double)data ["knockback"];
		lifeDrain = (float)(double)data ["lifeDrain"];
		poison = (float)(double)data ["poison"];
		poisonDur = (float)(double)data ["poisonDur"];
		splash = (float)(double)data ["splash"];
		stun = (float)(double)data ["stun"];
		slowdown = (float)(double)data ["slowdown"];
		slowDur = (float)(double)data ["slowDur"];
		penetration = (float)(double)data ["penetration"];
		shieldShred = (float)(double)data ["shieldShred"];
		trapArmTime = (float)(double)data ["trapArmTime"];
		spread = (int)(long)data ["spread"];
		splitCount = (int)(double)data ["doesSplit"];
		homingStrength = (float)(double)data ["isHoming"];
		arcDmg = (float)(double)data ["doesArc"];
		shieldHP = (float)(double)data ["shieldHP"];*/
	}
	
	//Assigns skill values to bullets
	private void ConfigureBullet(Bullet bc)
	{
        bc.dmg = dmg;
		bc.dmg /= split;
        bc.charge = charge;
		bc.chargeDamage = bc.dmg;
		if(bc.charge != 0){
			bc.dmg = 0;
		}
		float maxChargeTime = (MAX_CHARGE_TIME/MAX_CHARGE_RADIUS) * charge;
		if(maxChargeTime == 0)
			maxChargeTime = 0.001f;
		float chargePercent = mostRecentChargeTime / maxChargeTime;
		if(chargePercent > 1f)
			chargePercent = 1f;
		bc.chargePercent = chargePercent;
        bc.split = split;
        bc.penetration = penetration;
		bc.penetrationsLeft = penetration;
        bc.continuousStrength = continuousStrength;
	}
	//Assigns skill values to continuous fire Towers
	private void ConfigureLaser(Laser bc){
		bc.dmg = dmg;
		bc.dmg /= split;
        bc.charge = charge;
		float maxChargeTime = (MAX_CHARGE_TIME/MAX_CHARGE_RADIUS) * charge;
		if(maxChargeTime == 0)
			maxChargeTime = 0.001f;
		float chargePercent = mostRecentChargeTime / maxChargeTime;
		if(chargePercent > 1f)
			chargePercent = 1f;
		bc.chargePercent = chargePercent;
        bc.split = split;
        bc.penetration = penetration;
		bc.penetrationsLeft = penetration;
        bc.continuousStrength = continuousStrength;
	}
	//Assigns skill values to traps
	private void ConfigureTrap(Trap tc)
	{
		tc.dmg = tc.baseDamage;
        tc.trapUses = (int)trapUses;
		tc.usesLeft = (int)trapUses;
        tc.aoe = AoE;
        tc.attraction = attraction;
        tc.duplicate = duplicate;
        tc.field = field;
		tc.zone = GetCurrentLaneID();
	}

    //Assigns skill values to projectile traps
    private void ConfigureProjectileTrap(ProjectileTrap tc)
    {
        tc.dmg = tc.baseDamage;
        tc.trapUses = (int)trapUses;
        tc.usesLeft = (int)trapUses;
        tc.aoe = AoE;
        tc.attraction = attraction;
        tc.charge = charge;
        tc.split = split;
        tc.zone = GetCurrentLaneID();
    }
	
	//Assigns skill values to shields
	private void ConfigureShield(Shield sc)
	{
        sc.shieldDurability = shieldDurability;
        sc.reflect = reflect;
        sc.frequency = frequency;
        sc.tempDisplace = tempDisplace;
        sc.absorb = absorb;
	}
	#endregion
	
	#region GettersAndSetters (accessing properties)
	public float GetCooldownRatio(){
		return cooldown / maxcool;
	}
	public float GetCooldown(){
		return cooldown;
	}
	public int GetCurrentLaneID(){
		float angle = transform.eulerAngles.z;
		angle = (360f-angle)%360f;
		if (angle > 28.0 && angle < 32.0)
			return 1;
		else if (angle > 88.0 && angle < 92.0)
			return 2;
		else if (angle > 148.0 && angle < 152.0)
			return 3;
		else if (angle > 208.0 && angle < 212.0)
			return 4;
		else if (angle > 268.0 && angle < 272.0)
			return 5;
		else if (angle > 328.0 && angle < 332.0)
			return 6;
		else{
			Debug.Log ("somehow a gun has a very very wrong angle");
			return -1;
		}
	}
	
	public string GetTowerType(){
		return towerType;
	}
	//put getters/setters here?
	
	public void SetCooldown(float pCooldown)
	{
		baseMaxcool = pCooldown;
		maxcool = pCooldown;
	}
    public void SetEnergyGain(float pEnergyGain)
    {
        energyGain = pEnergyGain;
    }
    public void SetComboKey(float pComboKey)
    {
        comboKey = pComboKey;
    }
    public void SetDmg(float pdmg)
    {
        dmg = pdmg;
        //Debug.Log ("Set tower damage to " + dmg);
    }
    public void SetTrapUses(float ptrapUses)
    {
        trapUses = ptrapUses;
    }
    public void SetShieldDurability(float pShieldDur)
    {
        shieldDurability = pShieldDur;
    }
    public void SetCharge(float pcharge)
    {
        charge = pcharge;
    }
    public void SetSplit(float psplit)
    {
        split = (int)psplit;
    }
    public void SetPenetration(int ppenetration)
    {
        penetration = ppenetration;
    }
    public void SetContinuousStrength(float pcontinuous)
    {
        continuousStrength = pcontinuous;
    }
    public void SetReflect(float preflect)
    {
        reflect = preflect;
    }
    public void SetFrequency(float pfrequency)
    {
        frequency = pfrequency;
    }
    public void SetTempDisplace(float ptemp)
    {
        tempDisplace = ptemp;
    }
    public void SetAbsorb(float pabsorb)
    {
        absorb = pabsorb;
    }
    public void SetAoE(float paoe)
    {
        AoE = paoe;
    }
    public void SetAttraction(float pattraction)
    {
        attraction = pattraction;
    }
    public void SetDuplicate(int pduplicate)
    {
        duplicate = pduplicate;
    }
    public void SetField(float pfield)
    {
        field = pfield;
    }
	public float GetContinuousStrength(){
		return continuousStrength;
	}
    #endregion
}
