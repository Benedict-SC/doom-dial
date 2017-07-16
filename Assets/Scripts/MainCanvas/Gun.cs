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
    public readonly float TRAP_RANGE = 0.6f; //default trap range for now.  a little over halfway down the lane
    public readonly float P_TRAP_RANGE = 0.45f; //default projectile trap range. about halfway down the lane
	public readonly float MAX_CHARGE_RADIUS = 6f;
	public readonly float MAX_CHARGE_TIME = 5f;
    // ^^^ RELATIVE TO CENTER

    LinkedLaneList laneList; //a linked list representing the 6 lanes, used for ShieldTrap spawning
	
	//Tower type: "Bullet", "Trap", "Shield", "BulletTrap", "BulletShield", "TrapShield"
	string towerType;

    //Tech Abilities

    //Generic tower
    float cooldown; //weapon cooldown
    float energyGain; //each successful use gives additional energy
    float comboKey; //chance for a base weapon type combo to occur (?)
    float selfRepairRate; //chance for self-repair to occur on use of this gun
    float selfRepairAmt; //hp gained by one self-repair use

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
    public int duplicate; //Trap, triggers. (?)  TS, zone range.
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
	float shieldRange = 42f; //just so it's not hardcoded
	//***Skill values end here***
	
	/* Another tower attribute
	 * But not passed to bullets
	 */
	
	float baseMaxcool;
	public float maxcool;
	float lastCooltime;
    Timer cooltimer;
	
	bool shootingV = true; //for use by spread 3 -- are we shooting in a v this turn or not?
	bool isPaused = false;
	public int gunID; 
	
	Image cooldownImg;
	Image chargeImg;

    //use-lock risk stuff
    bool useLockIsOn = false; //use-lock Risk
    public bool waitingForOtherGunsToFire = false; //also for UseLock
    public bool useLockHasFired = false;

    //vampire risk stuff
    public float defaultVampDrain = .2f;
    float vampDrain = 0f;

    Dial dial;
    GameObject dialObj;
	
	// Use this for initialization
	void Start () {
        dialObj = GameObject.Find("Dial");
        dial = dialObj.GetComponent<Dial>();

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

        laneList = new LinkedLaneList();
        laneList.SetUpList();

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

        //if useLock risk is on
        if (PlayerPrefsInfo.Int2Bool(PlayerPrefs.GetInt(PlayerPrefsInfo.s_useLock)))
        {
            useLockIsOn = true;
        }

        //if vampire risk is on
        if (PlayerPrefsInfo.Int2Bool(PlayerPrefs.GetInt(PlayerPrefsInfo.s_vampire)))
        {
            vampDrain = defaultVampDrain;
        }
	}
	
	// Update is called once per frame
	void Update () {
        if (useLockIsOn)
        {
            //set waitingforothergunstofire
            //based on useLockHasFire and other guns' states
            if(waitingForOtherGunsToFire)
            {
                //if all other guns have fired
                if (dial.UseLockGunsHaveAllFired())
                {
                    //these lines are all the guns resetting
                    dial.UseLockResetAllGuns();
                }
            }
        }
		if (cooldown > 0) {
            float cooltime = cooltimer.TimeElapsedSecs();
            float elapsed = cooltime - lastCooltime;
            cooldown -= elapsed;
            if (waitingForOtherGunsToFire)
            {
                cooldown = maxcool;
            }
            lastCooltime = cooltime;
            if (cooldown < 0)
            {
                lastCooltime = 0;
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
		lastCooltime = 0f;
		if(continuousStrength > 0){
			cooldown = baseMaxcool + heldTime;
			maxcool = baseMaxcool + heldTime;
		}
	}

    //if this tower's cooldown is > 0, reduce it by sec seconds
    public void ReduceCooldownInstant(float sec)
    {
        //Debug.Log("calling ReduceCooldownInstant by " + sec + " on gun in lane " + (GetCurrentLaneID() - 1));
        //Debug.Log("cooldown is " + cooldown);
        //Debug.Log("new cooldown is " + cooldown);
        cooldown -= sec;
        if (cooldown < 0f)
        {
            cooldown = 0f;
			lastCooltime = 0f;
        }
    }

	public void Hold(){
		held = true;
	}
	public void Unhold(float time){
		if(held){ //bandaid on weird pointer event issue
			held = false;
			if(towerType == "Bullet" && continuousStrength > 0 && cooldown <= 0){
				StartCooldown(time);
				chargeImg.fillAmount = 0f;
				chargeImg.color = new Color(1f,1f,1f);
			}
		}
	}
	public void HandleEvent(GameEvent ge){
        CanvasSpinner cSpin = GameObject.Find("Dial").GetComponent<CanvasSpinner>();
		//various conditions under which bullet shouldn't fire
		if ((int)ge.args [0] != gunID)
			//Debug.Log ("not tower " + gunID);
			return;
		if (cooldown > 0) {
			//Debug.Log ("cooldown > 0");
			return;
		}
		if (cSpin.IsSpinning ()) {
			Debug.Log ("dial is spinning");
			return;
		}
		if (transform.gameObject.activeSelf != true) {
			Debug.Log ("we're not active");
			return;
		}
        if (useLockIsOn && useLockHasFired)
        {
            Debug.Log("uselock is on and this gun has already fired!");
            return;
        }

        if (useLockIsOn)
        {
            useLockHasFired = true;
            waitingForOtherGunsToFire = true;
        }

        //unlock dial spinner if it was rot-locked before
        cSpin.rotLockIsLocked = false;

        //self-repair chance
        if (selfRepairRate > 0f)
        {
            float rando = UnityEngine.Random.value * 100f;
            if (rando <= selfRepairRate)
            {
                Debug.Log("gun did self-repair");
                dial.ChangeHealth(selfRepairAmt);
            }
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
			SpawnShield();
			break;
        case "BulletTrap":
            //BulletTrap behavior
            SpawnProjectileTrap();
            break;
        case "BulletShield":
            //BulletShield behavior
            SpawnProjectileShield();
            break;
        case "TrapShield":
            //TrapShield behavior
            SpawnAllShieldTraps();
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
		float spawnRadius = Dial.TRACK_LENGTH * (TRAP_RANGE + 0.05f);
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
        float spawnRadius = Dial.TRACK_LENGTH * P_TRAP_RANGE;
        float longRadius = Dial.TRACK_LENGTH * (P_TRAP_RANGE + 0.2f);
        float shortRadius = Dial.TRACK_LENGTH * (P_TRAP_RANGE - 0.2f);
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
	public void SpawnShield(){
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
        RectTransform shieldRt = sc.GetComponent<RectTransform>();
        shieldRt.anchoredPosition = new Vector2(shieldRange*Mathf.Cos(shieldAngle),shieldRange*Mathf.Sin (shieldAngle));
        //Debug.Log("shield y should be " + shieldRange * Mathf.Sin(shieldAngle));
		shieldRt.rotation = this.gameObject.transform.rotation;
		dialCon.PlaceShield (GetCurrentLaneID() - 1, shield); //mark current lane as shielded (placed in array)
	}

    public void SpawnAllShieldTraps()
    {
        //this works differently from regular shield spawning
        //using 0-5 for lane IDS to be compatible with Dial
        Dial dialCon = GameObject.Find("Dial").gameObject.GetComponent<Dial>();
        GameObject holderObj = Instantiate(Resources.Load("Prefabs/MainCanvas/ShieldTrapHolder")) as GameObject;
        ShieldTrapHolder holder = holderObj.GetComponent<ShieldTrapHolder>();
        holder.SetUp();
        int currentLane = GetCurrentLaneID() - 1;
        List<int> spawnLanes = new List<int>();
        //use lane looping linked list
        //always will spawn in current lane if possible
        if (duplicate > 5) duplicate = 5;
        if (!dialCon.IsShielded(currentLane))
        {
            spawnLanes.Add(currentLane);
            int r;
            int left = laneList.GetRelativeLane(currentLane, -1);
            int right = laneList.GetRelativeLane(currentLane, 1);
            int left2 = laneList.GetRelativeLane(currentLane, -2);
            int right2 = laneList.GetRelativeLane(currentLane, 2);
            int opposite = laneList.GetRelativeLane(currentLane, 3);
            switch (duplicate)
            {
                case 0:
                    break;
                //random left or right adjacent lane
                case 1:
                    //if only left is available
                    if (dialCon.IsShielded(right) && !dialCon.IsShielded(left))
                    {
                        spawnLanes.Add(left);
                    }
                    //if only right is available
                    else if (dialCon.IsShielded(left) && !dialCon.IsShielded(right))
                    {
                        spawnLanes.Add(right);
                    }
                    //if both lanes are available, add one of them
                    else if (!dialCon.IsShielded(left) && !dialCon.IsShielded(right))
                    {
                        r = UnityEngine.Random.Range(0, 2);
                        if (r == 0)
                        {
                            spawnLanes.Add(left);
                        }
                        else
                        {
                            spawnLanes.Add(right);
                        }
                    }
                    break;
                //add to both adjacent lanes if available
                case 2:
                    if (!dialCon.IsShielded(left))
                    {
                        spawnLanes.Add(left);
                    }
                    if (!dialCon.IsShielded(right))
                    {
                        spawnLanes.Add(right);
                    }
                    break;
                //add to both adjacent plus a random 2-away lane
                case 3:
                    r = UnityEngine.Random.Range(0, 2);
                    //if only left is available
                    if (!dialCon.IsShielded(left) && dialCon.IsShielded(right))
                    {
                        spawnLanes.Add(right);
                        if (!dialCon.IsShielded(left2))
                        {
                            spawnLanes.Add(left2);
                        }
                    }
                    //if only right is available
                    else if (!dialCon.IsShielded(right) && dialCon.IsShielded(left))
                    {
                        spawnLanes.Add(right);
                        if (!dialCon.IsShielded(right2))
                        {
                            spawnLanes.Add(right2);
                        }
                    }
                    //if both left and right are available
                    else if (!dialCon.IsShielded(right) && !dialCon.IsShielded(left))
                    {
                        spawnLanes.Add(left);
                        spawnLanes.Add(right);
                        //if only left2 is available
                        if (!dialCon.IsShielded(left2) && dialCon.IsShielded(right2))
                        {
                            spawnLanes.Add(left2);
                        }
                        //if only right2 is available
                        else if (!dialCon.IsShielded(right2) && dialCon.IsShielded(left2))
                        {
                            spawnLanes.Add(right2);
                        }
                        //if both are available
                        else if (!dialCon.IsShielded(left2) && !dialCon.IsShielded(right2))
                        {
                            if (r == 0)
                            {
                                spawnLanes.Add(left2);
                            }
                            else
                            {
                                spawnLanes.Add(right2);
                            }
                        }
                    }
                    break;
                //add 2 on each side
                case 4:
                    if (!dialCon.IsShielded(left))
                    {
                        spawnLanes.Add(left);
                        if (!dialCon.IsShielded(left2))
                        {
                            spawnLanes.Add(left2);
                        }
                    }
                    if (!dialCon.IsShielded(right))
                    {
                        spawnLanes.Add(right);
                        if (!dialCon.IsShielded(right2))
                        {
                            spawnLanes.Add(right2);
                        }
                    }
                    break;
                //add to every lane if possible
                case 5:
                    bool oppPlaced = false;
                    if (!dialCon.IsShielded(left))
                    {
                        spawnLanes.Add(left);
                        if (!dialCon.IsShielded(left2))
                        {
                            spawnLanes.Add(left2);
                            if (!dialCon.IsShielded(opposite))
                            {
                                spawnLanes.Add(opposite);
                                oppPlaced = true;
                            }
                        }
                    }
                    if (!dialCon.IsShielded(right))
                    {
                        spawnLanes.Add(right);
                        if (!dialCon.IsShielded(right2))
                        {
                            spawnLanes.Add(right2);
                            if (!dialCon.IsShielded(opposite) && !oppPlaced)
                            {
                                spawnLanes.Add(opposite);
                            }
                        }
                    }
                    break;
                default:
                    Debug.Log("duplicate has an invalid value");
                    break;
            }
        }
        foreach (int i in spawnLanes)
        {
            SpawnShieldTrap(i, holder);
        }
        holder.DestroyIfEmpty();
        holder.SetShieldSprites();
    }

    //spawns a shield trap in a given lane with a given holder
    void SpawnShieldTrap(int lane, ShieldTrapHolder holder)
    {
        Debug.Log("lane in spawnshieldtrap: " + lane);
        int currentLane = GetCurrentLaneID() - 1;
        int laneDif = laneList.MinDistanceBetween(currentLane, lane);
        Dial dialCon = GameObject.Find("Dial").gameObject.GetComponent<Dial>();
        if (dialCon.IsShielded(lane)) //if there's already a shield there
        {
            Debug.Log("already a shield here, not placing ShieldTrap");
            return;
        }
        GameObject shieldTrap = Instantiate(Resources.Load("Prefabs/MainCanvas/ShieldTrap")) as GameObject; //make a shield
        shieldTrap.transform.SetParent(Dial.underLayer, false);
        ShieldTrap sc = shieldTrap.GetComponent<ShieldTrap>();
        //make it the type of shield-trap this thing deploys
        ConfigureShieldTrap(sc);
        //find your angle, adding +/- 60 for each lane removed from This gun's lane
        float shieldOwnangle = this.transform.eulerAngles.z + (60 * laneDif);
        float shieldAngle = (shieldOwnangle + 90) % 360;
        shieldAngle *= (float)Math.PI / 180;
        //find where to spawn the shield
        RectTransform shieldRt = sc.GetComponent<RectTransform>();
        shieldRt.anchoredPosition = new Vector2(shieldRange * Mathf.Cos(shieldAngle), shieldRange * Mathf.Sin(shieldAngle));
        //Debug.Log("shield y should be " + shieldRange * Mathf.Sin(shieldAngle));
        shieldRt.rotation = this.gameObject.transform.rotation;
        dialCon.PlaceShield(lane, shieldTrap); //mark current lane as shielded (placed in array)
        sc.SetMyLane(lane);
        sc.SetHolder(holder);
        holder.AddShield(sc);
    }
	
    void SpawnProjectileShield(){
        Dial dialCon = GameObject.Find ("Dial").gameObject.GetComponent<Dial>();
		if (dialCon.IsShielded (GetCurrentLaneID() - 1)) //if there's already a shield there
		{
			//play some sort of error sound
            cooldown = 0f;
            Debug.Log("already a shield in lane " + GetCurrentLaneID());
            return;
		}
		GameObject shield = Instantiate (Resources.Load ("Prefabs/MainCanvas/ProjectileShield")) as GameObject; //make a shield
        shield.transform.SetParent(Dial.underLayer, false);
		BulletShield bsc = shield.GetComponent<BulletShield>();
		//make it the type of shield this thing deploys
		ConfigureProjectileShield (bsc);
		//find your angle
		float shieldOwnangle = this.transform.eulerAngles.z;
		float shieldAngle = (shieldOwnangle +  90) % 360 ;
		shieldAngle *= (float)Math.PI / 180;
		//find where to spawn the shield
        RectTransform shieldRt = bsc.GetComponent<RectTransform>();
        shieldRt.anchoredPosition = new Vector2(shieldRange*Mathf.Cos(shieldAngle),shieldRange*Mathf.Sin (shieldAngle));
        //Debug.Log("shield y should be " + shieldRange * Mathf.Sin(shieldAngle));
		shieldRt.rotation = this.gameObject.transform.rotation;
		dialCon.PlaceShield (GetCurrentLaneID() - 1, shield); //mark current lane as shielded (placed in array)
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
        bc.comboKey = comboKey;
        bc.vampDrain = vampDrain;
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
        bc.vampDrain = vampDrain;
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
        tc.comboKey = comboKey;
        tc.vampDrain = vampDrain;
	}

    //Assigns skill values to projectile traps
    private void ConfigureProjectileTrap(ProjectileTrap tc)
    {
        tc.dmg = tc.baseDamage;
        tc.trapUses = (int)trapUses;
        tc.usesLeft = (int)trapUses;
        tc.aoe = AoE;
        //Debug.Log("setting tc.aoe to " + AoE);
        tc.attraction = attraction;
        tc.charge = charge;
        tc.split = split;
        tc.zone = GetCurrentLaneID();
        tc.vampDrain = vampDrain;
    }
	
	//Assigns skill values to shields
	private void ConfigureShield(Shield sc)
	{
        sc.shieldDurability = shieldDurability;
        sc.reflect = reflect;
        sc.frequency = frequency;
        sc.tempDisplace = tempDisplace;
        sc.absorb = absorb;
        sc.comboKey = comboKey;
        sc.vampDrain = vampDrain;
	}

    //Assigns skill values to shield traps
    private void ConfigureShieldTrap(ShieldTrap sc)
    {
        //Debug.Log("tempdisplace of gun is " + tempDisplace);
        sc.absorb = absorb;
        sc.duplicate = duplicate;
        sc.tempDisplace = tempDisplace;
        sc.field = field;
        sc.vampDrain = vampDrain;
    }

    //Assigns skill values to projectile shields
    private void ConfigureProjectileShield(BulletShield bsc)
    {
        bsc.reflect = reflect;
        bsc.frequency = frequency;
        bsc.penetration = penetration;
        bsc.continuousStrength = continuousStrength;
        bsc.vampDrain = vampDrain;
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

    //put getters/setters here

    public string GetTowerType(){
		return towerType;
	}
	
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
    public void SetSelfRepairRate(float prate)
    {
        selfRepairRate = prate;
    }
    public void SetSelfRepairAmt(float pamt)
    {
        selfRepairAmt = pamt;
    }
	public float GetContinuousStrength(){
		return continuousStrength;
	}
    #endregion
}
