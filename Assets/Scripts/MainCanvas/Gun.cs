/*Some Thom, some Duncan*/

using UnityEngine;
using UnityEngine.UI;
using MiniJSON;
using System;
using System.Collections.Generic;

public class Gun : MonoBehaviour,EventHandler{

	Canvas canvas;
	public bool decalSet = false;

	public readonly float TRACK_LENGTH = 110.8f; //hard coded to avoid querying track size all the time
    public readonly float TRAP_RANGE = 0.5f; //default trap range for now.  halfway down the lane
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
    int split; //Bullets, no. of split bullets.  BT, no. of radial bullets on hit

    //Bullet and BulletShield
    float penetration; //Bullet, penetration.  BS, shield shred.
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
    float duplicate; //Trap, triggers. (?)  TS, zone range.
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
	
	float maxcool;
    Timer cooltimer;
	
	bool shootingV = true; //for use by spread 3 -- are we shooting in a v this turn or not?
	bool isPaused = false;
	public int gunID; 
	
	Image cooldownImg;
	
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
		
	}
	#region Firing (make the gun shoot a thing)
	public void Fire(){
		if(GamePause.paused)
			return;
		GameEvent nge = new GameEvent ("shot_fired");
		nge.addArgument (gunID);
		EventManager.Instance ().RaiseEvent (nge);
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

        //Decide what to do based on tower type ("Bullet", "Trap", "Shield", "BulletTrap", "BulletShield", "TrapShield")
        switch (towerType)
		{
		case "Bullet":
			switch (split)
			{
			case 1:
				SpawnBulletI ();
				break;
			case 2:
				SpawnBulletWideV ();
				break;
			case 3:
				if (shootingV)
				{
					SpawnBulletWideV ();
					shootingV = false;
				}
				else if (!shootingV)
				{
					SpawnBulletI ();
					shootingV = true;
				}
				break;
			case 4:
				SpawnBulletI ();
				SpawnBulletWideV ();
				break;
			default:
				Debug.Log ("Spread error in Gun Bullet firing");
				break;
			}
			break;
		case "Trap":
			for (int i = 1; i <= split; i++)
			{
				//Debug.Log ("range is " + range);
				float spawnRadius = Dial.TRACK_LENGTH * TRAP_RANGE;
				GameObject trap = Instantiate (Resources.Load ("Prefabs/MainCanvas/Trap")) as GameObject; //make a bullet
				trap.transform.SetParent(Dial.spawnLayer,false);
				RectTransform bulletRect = (RectTransform)trap.transform;
				RectTransform rt = (RectTransform)transform;
				Trap tc = trap.GetComponent<Trap>();
				//make it the type of bullet this thing fires
				ConfigureTrap (tc);
				//find your angle
				float ownangle = this.transform.eulerAngles.z;
				float angle = (ownangle +  90) % 360 ;
				angle *= (float)Math.PI / 180;
				//Debug.Log ("original angle: " + angle);
				angle = (angle - (float)Math.PI / 6f) + ((((float)Math.PI / 3f) / (2)) * i); //handles spread effect
				//find where to spawn the bullet
				float gunDistFromCenter = Dial.DIAL_RADIUS;
                Debug.Log("spawnRadius is " + spawnRadius);
				tc.spawnx = (gunDistFromCenter + spawnRadius) * (float)Math.Cos (angle);
				tc.spawny = (gunDistFromCenter + spawnRadius) * (float)Math.Sin (angle);
				//Debug.Log (bc.speed);
				bulletRect.anchoredPosition = new Vector2(tc.spawnx,tc.spawny);
				tc.transform.rotation = transform.rotation;
			}
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
			RectTransform rt = (RectTransform)transform;
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
			if(split == 4){
				bc.split = 0; //don't split if you're the middle split on a 3-way thing
			}
		}
		
	}
	
	void SpawnBulletV()
	{
		for (int i = 1; i <= 2; i++)
		{
			//Debug.Log ("called instantiate bullet");
			GameObject bullet = Instantiate (Resources.Load ("Prefabs/MainCanvas/Bullet")) as GameObject; //make a bullet
			bullet.transform.SetParent(Dial.spawnLayer,false);
			Bullet bc = bullet.GetComponent<Bullet>();
			RectTransform bulletRect = (RectTransform)bullet.transform;
			RectTransform rt = (RectTransform)transform;
			//make it the type of bullet this thing fires
			ConfigureBullet (bc);
			//find your angle
			float ownangle = this.transform.eulerAngles.z;
			float angle = (ownangle +  90) % 360 ;
			angle *= (float)Math.PI / 180;
			//Debug.Log ("original angle: " + angle);
			angle = (angle - (float)Math.PI / 6f) + ((((float)Math.PI / 3f) / (3)) * i); //handles spread effect
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
			if(i == 1){
				bc.splitCode = -1;
			}else{
				bc.splitCode = 1;
			}
		}
		
	}
	
	void SpawnBulletWideV()
	{
		for (int i = 1; i <= 2; i++)
		{
			//Debug.Log ("called instantiate bullet");
			GameObject bullet = Instantiate (Resources.Load ("Prefabs/MainCanvas/Bullet")) as GameObject; //make a bullet
			bullet.transform.SetParent(Dial.spawnLayer,false);
			Bullet bc = bullet.GetComponent<Bullet>();
			RectTransform bulletRect = (RectTransform)bullet.transform;
			RectTransform rt = (RectTransform)transform;
			//make it the type of bullet this thing fires
			ConfigureBullet (bc);
			//find your angle
			float ownangle = this.transform.eulerAngles.z;
			float angle = (ownangle +  90) % 360 ;
			angle *= (float)Math.PI / 180;
			//Debug.Log ("original angle: " + angle);
			float mult = 1f;
			if (i == 2)
			{
				mult = 1.5f; //so it skips the middle and spawns the second one at the 3/4 angle
			}
			angle = (angle - (float)Math.PI / 6f) + ((((float)Math.PI / 3f) / (4 /*wideness const*/)) * (i * mult)); //handles spread effect
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
			if(i == 1){
				bc.splitCode = -1;
			}else{
				bc.splitCode = 1;
			}
		}
	}
	#endregion
	
	#region Setup (get the gun ready to do stuff)
	public void SetValuesFromJSON(string filename){
		//FileLoader fl = new FileLoader ("JSONData" + Path.DirectorySeparatorChar + "Towers",filename);
		FileLoader fl = new FileLoader (Application.persistentDataPath,"Towers",filename);
		string json = fl.Read ();
		Dictionary<string,System.Object> data = (Dictionary<string,System.Object>)Json.Deserialize (json);
		
		string imgfilename = data ["decalFilename"] as string;
		Image img = transform.FindChild("Label").gameObject.GetComponent<Image> ();
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
        bc.charge = charge;
        bc.split = split;
        bc.penetration = penetration;
        bc.continuousStrength = continuousStrength;
	}
	
	//Assigns skill values to traps
	private void ConfigureTrap(Trap tc)
	{
        tc.trapUses = (int)trapUses;
        tc.aoe = AoE;
        tc.attraction = attraction;
        tc.duplicate = duplicate;
        tc.field = field;
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
		if (angle > 28.0 && angle < 32.0)
			return 1;
		else if (angle > 88.0 && angle < 92.0)
			return 6;
		else if (angle > 148.0 && angle < 152.0)
			return 5;
		else if (angle > 208.0 && angle < 212.0)
			return 4;
		else if (angle > 268.0 && angle < 272.0)
			return 3;
		else if (angle > 328.0 && angle < 332.0)
			return 2;
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
    public void SetPenetration(float ppenetration)
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
    public void SetDuplicate(float pduplicate)
    {
        duplicate = pduplicate;
    }
    public void SetField(float pfield)
    {
        field = pfield;
    }
    #endregion
}
