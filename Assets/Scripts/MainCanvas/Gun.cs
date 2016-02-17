using UnityEngine;
using UnityEngine.UI;
using MiniJSON;
using System;
using System.Collections.Generic;

public class Gun : MonoBehaviour,EventHandler{

	Canvas canvas;
	public bool decalSet = false;

	public readonly float TRACK_LENGTH = 110.8f; //hard coded to avoid querying track size all the time
	// ^^^ RELATIVE TO CENTER
	
	//Tower type: "Bullet", "Trap", or "Shield"
	string towerType;
	
	//***Skill values begin here***
	float dmg; //damage dealt out (direct value)
	float speed; //speed of the bullet
	public float range; //range -- expressed in percent of the length of the lane
	float knockback; //knockback -- positive value for distance knocked back
	float lifeDrain; //lifedrain on enemy
	float poison; //poison damage on enemy
	float poisonDur; //how long poison lasts, in seconds
	float splash; //percent (0.0f - .75f) of effects to carry to enemies hit by splash
	float splashRad = 4.0f; //radius of splash damage (default for now)
	float stun; //amount (time?) of enemy stun
	float slowdown; //enemy slowdown -- scale of 1 to 10, can't go over 8
	float slowDur; //how long slowdown lasts
	float penetration; //ignores this amount of enemy shield
	float shieldShred; //lowers enemy shield's max value by this
	float trapArmTime; //time in seconds to arm a trap
	float splitCount; //number of pieces it splits into
	float homingStrength; //strength of homing :P
	float arcDmg; //dmg bonus of arc -- if above 0, it will arc
	int spread; //1 is normal, 2 is V, 3 is alternating V and I, 4 is three shots
	
	bool chainsPoison;
	float slowsShields;
	float aoeRadiusBonus = 1.0f;
	bool multiSplits;
	int pierces = 0;
	
	float shieldHP; //shield max HP
	float shieldRegen; //shield regen rate
	float shieldRange = 55f; //just so it's not hardcoded
	//***Skill values end here***
	
	/* Another tower attribute
	 * But not passed to bullets
	 */
	float cooldownFactor = 1.0f; //percentage of max cooldown time.  By default 1.0
	float DEF_COOLDOWN = 2.0f;
	
	float cooldown = 0.0f;
	float maxcool;
	
	bool shootingV = true; //for use by spread 3 -- are we shooting in a v this turn or not?
	bool isPaused = false;
	public int buttonID; //assign in the Unity Editor to match the corresponding button
	//in the future, we'll assign this value in scripts to deal with changing gun placements
	
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
		
		//defaults
		/*towerType = "Shield";
		shieldRange = 1f;
		dmg = 10;
		speed = defaultBulletSpeed;
		range = defaultBulletRange;
		shieldHP = 100;*/
		
		if (spread > 4)
			spread = 4;
		if (spread < 1)
			spread = 1;
	}
	
	// Update is called once per frame
	void Update () {
		isPaused = Pause.paused;
		if (!isPaused) {
			if (cooldown > 0) {
				cooldown -= 0.05f; //tweak this for a one-second cooldown from 1.0f
				if (cooldown < 0)
					cooldown = 0;
				cooldownImg.fillAmount = GetCooldownRatio();
			}
		}
	}
	#region Firing (make the gun shoot a thing)
	public void Fire(){
		if(GamePause.paused)
			return;
		GameEvent nge = new GameEvent ("shot_fired");
		nge.addArgument (buttonID);
		EventManager.Instance ().RaiseEvent (nge);
	}
	public void HandleEvent(GameEvent ge){
		
		//various conditions under which bullet shouldn't fire
		if ((int)ge.args [0] != buttonID)
			//Debug.Log ("not tower " + buttonID);
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
		
		cooldown = maxcool; //start cooldown
		
		//Decide what to do based on tower type ("Bullet", "Trap", or "Shield")
		switch (towerType)
		{
		case "Bullet":
			switch (spread)
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
			for (int i = 1; i <= spread; i++)
			{
				//Debug.Log ("range is " + range);
				float spawnRadius = Dial.TRACK_LENGTH * range;
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
		default:
			print ("Uh oh, I didn't receive a valid towerType string value!");
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
			if(spread == 4){
				bc.splitCount = 0; //don't split if you're the middle split on a 3-way thing
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
				bc.spreadCode = -1;
			}else{
				bc.spreadCode = 1;
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
				bc.spreadCode = -1;
			}else{
				bc.spreadCode = 1;
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
		if (speed == 0 || range == 0 || dmg == 0)
			Debug.Log("Check your speed, range, and/or dmg!  One might be 0!");
		bc.dmg = dmg;
		bc.speed = speed;
		bc.range = range;
		bc.knockback = knockback;
		bc.lifeDrain = lifeDrain;
		bc.poison = poison;
		bc.poisonDur = poisonDur;
		bc.chainsPoison = chainsPoison;
		bc.splash = splash;
		bc.splashRad = splashRad * aoeRadiusBonus;
		bc.stun = stun;
		bc.slowdown = slowdown;
		bc.slowDur = slowDur;
		bc.penetration = penetration;
		bc.shieldShred = shieldShred;
		bc.slowsShields = slowsShields;
		bc.splitCount = splitCount;
		bc.multiSplits = multiSplits;
		bc.homingStrength = homingStrength;
		bc.arcDmg = arcDmg;
		bc.isSplitBullet = false;
		bc.piercesLeft = pierces;
		if(pierces > 0)
			bc.pierces = true;
		//Debug.Log ("bullet slowDur is " + bc.slowDur);
	}
	
	//Assigns skill values to traps
	private void ConfigureTrap(Trap bc)
	{
		if (range == 0 || dmg == 0)
			print ("Check your range and/or dmg!  One might be 0!");
		bc.dmg = dmg;
		bc.range = range;
		bc.knockback = knockback;
		bc.lifeDrain = lifeDrain;
		bc.poison = poison;
		bc.poisonDur = poisonDur;
		bc.splash = splash;
		bc.splashRad = splashRad * aoeRadiusBonus;
		bc.stun = stun;
		bc.slowdown = slowdown;
		bc.slowDur = slowDur;
		bc.penetration = penetration;
		bc.shieldShred = shieldShred;
		bc.maxArmingTime = trapArmTime;
	}
	
	//Assigns skill values to shields
	private void ConfigureShield(Shield sc)
	{
		if (shieldHP == 0)
			print ("Check your shield HP value!  might be 0!");
		sc.maxHP = shieldHP;
		//bc.regenRate = shieldRegen; //commented out since regen rate doesn't vary, according to joe
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
		if (angle > -2.0 && angle < 2.0)
			return 1;
		else if (angle > 58.0 && angle < 62.0)
			return 6;
		else if (angle > 118.0 && angle < 122.0)
			return 5;
		else if (angle > 178.0 && angle < 182.0)
			return 4;
		else if (angle > 238.0 && angle < 242.0)
			return 3;
		else if (angle > 298.0 && angle < 302.0)
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
	
	public void SetDmg(float pdmg)
	{
		dmg = pdmg;
		//Debug.Log ("Set tower damage to " + dmg);
	}
	public void SetSpeed(float pspeed)
	{
		speed = pspeed;
	}
	public void SetRange(float prange)
	{
		range = prange;
	}
	public void SetKnockback(float pknockback)
	{
		knockback = pknockback;
	}
	public void SetLifeDrain(float pLifeDrain)
	{
		lifeDrain = pLifeDrain;
	}
	public void SetPoison(float pPoison)
	{
		poison = pPoison;
	}
	public void SetPoisonDur(float pPoisonDur)
	{
		poisonDur = pPoisonDur;
	}
	public void SetChainPoison(bool chainPois){
		chainsPoison = chainPois;
	}
	public void SetSplash(float pSplash)
	{
		splash = pSplash;
	}
	public void SetSplashRadiusBonus(float percentBonus){
		aoeRadiusBonus = 1f + percentBonus;
	}
	public void SetStun(float pStun)
	{
		stun = pStun;
	}
	public void SetSlowdown(float pSlowdown)
	{
		slowdown = pSlowdown;
	}
	public void SetSlowDur(float pSlowDur)
	{
		slowDur = pSlowDur;
	}
	public void SetPenetration(float pPenetration)
	{
		penetration = pPenetration;
	}
	public void SetPiercing(int pierceCount){
		pierces = pierceCount;
	}
	public void SetShieldShred(float pShieldShred)
	{
		shieldShred = pShieldShred;
	}
	public void SetShieldSlow(float pShieldSlow){
		slowsShields = pShieldSlow;
	}
	public void SetTrapArmTime(float pTrapArmTime)
	{
		trapArmTime = pTrapArmTime;
	}
	public void SetSpread(int pSpread)
	{
		spread = pSpread;
	}
	public void SetSplit(int pDoesSplit)
	{
		splitCount = pDoesSplit;
	}
	public void SetMultiSplit(bool pMultiSplits){
		multiSplits = pMultiSplits;
	}
	public void SetIsHoming(float pIsHoming)
	{
		homingStrength = pIsHoming;
	}
	public void SetDoesArc(float pDoesArc)
	{
		arcDmg = pDoesArc;
	}
	public void SetShieldHP(float pShieldHP)
	{
		shieldHP = pShieldHP;
	}
	public void SetShieldRegen(float pShieldRegen)
	{
		shieldRegen = pShieldRegen;
	}
	public void SetShieldRange(float pShieldRange)
	{
		shieldRange = pShieldRange;
	}
	public void SetCooldown(float pCooldown)
	{
		maxcool = pCooldown;
	}
	#endregion
}
