﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
using MiniJSON;

public class GunController : MonoBehaviour, EventHandler {

	public readonly float TRACK_LENGTH = 4.52f; //hard coded to avoid querying track size all the time
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
	float splash; //radius of splash damage
	float stun; //amount (time?) of enemy stun
	float slowdown; //enemy slowdown -- scale of 1 to 10, can't go over 8
	float slowDur; //how long slowdown lasts
	float penetration; //ignores this amount of enemy shield
	float shieldShred; //lowers enemy shield's max value by this
	float trapArmTime; //time in seconds to arm a trap

	int spread; //number of shots fired at once, default should be 1.

	bool doesSplit; //whether it splits in 2 at the end of its path/collision
	bool isHoming; //whether it homes in on nearest enemy
	bool doesArc; //whether it arcs (travels over enemies until it hits the ground at max range)

	float shieldHP; //shield max HP
	float shieldRegen; //shield regen rate
	float shieldRange; //just so it's not hardcoded
	//***Skill values end here***

	/* Another tower attribute
	 * But not passed to bullets
	 */
	float cooldownFactor = 1.0f; //percentage of max cooldown time.  By default 1.0
	float DEF_COOLDOWN = 2.0f;

	float cooldown = 0.0f;
	float maxcool;

	public int buttonID; //assign in the Unity Editor to match the corresponding button
	//in the future, we'll assign this value in scripts to deal with changing gun placements

	//float defaultBulletSpeed = 0.2f;
	//float defaultBulletRange = 1.0f;

	// Use this for initialization
	void Start () {
		EventManager.Instance ().RegisterForEventType ("shot_fired", this);
		GameObject overlayObject = transform.Find("CooldownLayer").gameObject;
		overlayObject.transform.localScale = new Vector3 (0, 0, 1);

		//defaults
		/*towerType = "Shield";
		shieldRange = 1f;
		dmg = 10;
		speed = defaultBulletSpeed;
		range = defaultBulletRange;
		shieldHP = 100;*/
		shieldRange = 1.0f;

		if (spread > 2)
			spread = 2;
	}
	
	// Update is called once per frame
	void Update () {
		if (cooldown > 0) {
			cooldown -= 0.05f; //tweak this for a one-second cooldown from 1.0f
			//SpriteRenderer overlay = this.gameObject.GetComponentInChildren<SpriteRenderer> ();
			GameObject overlayObject = transform.Find("CooldownLayer").gameObject;
			overlayObject.transform.localScale = new Vector3 (cooldown / maxcool, cooldown / maxcool, 1);
			if(cooldown < 0)
				cooldown = 0;
		}
	}

	public void HandleEvent(GameEvent ge){

		//various conditions under which bullet shouldn't fire
		if ((int)ge.args [0] != buttonID)
			//Debug.Log ("not tower " + buttonID);
			return;
		if (cooldown > 0) {
			Debug.Log ("cooldown > 0");
			return;
		}
		if (GameObject.Find ("Dial").GetComponent<SpinScript> ().IsSpinning ()) {
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
			//Debug.Log ("it's a bullet yo (spread is " + spread + ")");
			for (int i = 1; i <= spread; i++)
			{
				Debug.Log ("called instantiate bullet");
				GameObject bullet = Instantiate (Resources.Load ("Prefabs/Bullet")) as GameObject; //make a bullet
				BulletController bc = bullet.GetComponent<BulletController>();
				//make it the type of bullet this thing fires
				ConfigureBullet (bc);
				//find your angle
				float ownangle = this.transform.eulerAngles.z;
				float angle = (ownangle +  90) % 360 ;
				angle *= (float)Math.PI / 180;
				Debug.Log ("original angle: " + angle);
				angle = (angle - (float)Math.PI / 6f) + ((((float)Math.PI / 3f) / (spread + 1)) * i); //handles spread effect
				//find where to spawn the bullet
				float gunDistFromCenter = (float)Math.Sqrt (transform.position.x*transform.position.x + transform.position.y*transform.position.y);
				gunDistFromCenter += 0.47f;
				bc.spawnx = gunDistFromCenter * (float)Math.Cos (angle);
				bc.spawny = gunDistFromCenter * (float)Math.Sin (angle);
				//Debug.Log (bc.speed);
				bc.transform.position = new Vector3(bc.spawnx,bc.spawny,bc.transform.position.z);
				bc.vx = bc.speed * (float)Math.Cos(angle);
				bc.vy = bc.speed * (float)Math.Sin(angle);
			}
			break;
		case "Trap":
			for (int i = 1; i <= spread; i++)
			{
				GameObject trap = Instantiate (Resources.Load ("Prefabs/Trap")) as GameObject; //make a trap
				TrapController tp = trap.GetComponent<TrapController>();
				//make it the type of trap this thing deploys
				ConfigureTrap (tp);
				//find your angle
				float trapOwnangle = this.transform.eulerAngles.z;
				float trapAngle = (trapOwnangle +  90) % 360 ;
				trapAngle *= (float)Math.PI / 180;
				trapAngle = (trapAngle - (float)Math.PI / 6f) + ((((float)Math.PI / 3f) / (spread + 1)) * i); //handles spread effect
				//find where to spawn the trap *****IMPLEMENT LANE-LENGTH AT SOME POINT
				float trapSpawnRange = range;
				trapSpawnRange *= TRACK_LENGTH;
				tp.spawnx = trapSpawnRange * (float)Math.Cos (trapAngle);
				tp.spawny = trapSpawnRange * (float)Math.Sin (trapAngle);
			}
			break;
		case "Shield":
			DialController dialCon = GameObject.Find ("Dial").gameObject.GetComponent<DialController>();
			if (dialCon.IsShielded (GetCurrentLaneID() - 1)) //if there's already a shield there
			{
				dialCon.DestroyShield(GetCurrentLaneID() - 1); //destroy that shield
				Debug.Log ("destroyed previous shield");
			}
			GameObject shield = Instantiate (Resources.Load ("Prefabs/Shield")) as GameObject; //make a shield
			ShieldController sc = shield.GetComponent<ShieldController>();
			//make it the type of shield this thing deploys
			ConfigureShield (sc);
			//find your angle
			float shieldOwnangle = this.transform.eulerAngles.z;
			float shieldAngle = (shieldOwnangle +  90) % 360 ;
			shieldAngle *= (float)Math.PI / 180;
			//find where to spawn the shield
			float shieldSpawnRange = shieldRange;
			shieldSpawnRange += 0.5f;
			sc.spawnx = shieldSpawnRange * (float)Math.Cos (shieldAngle);
			sc.spawny = shieldSpawnRange * (float)Math.Sin (shieldAngle);
			sc.gameObject.transform.rotation = this.gameObject.transform.rotation;
			dialCon.PlaceShield (GetCurrentLaneID() - 1, shield); //mark current lane as shielded (placed in array)
			break;
		default:
			print ("Uh oh, I didn't receive a valid towerType string value!");
			Debug.Log("value is: " + towerType);
			break;
		}

	}
	
	public void SetValuesFromJSON(string filename){
		FileLoader fl = new FileLoader ("JSONData" + Path.DirectorySeparatorChar + "Towers",filename);
		string json = fl.Read ();
		Dictionary<string,System.Object> data = (Dictionary<string,System.Object>)Json.Deserialize (json);
		
		string imgfilename = data ["decalFilename"] as string;
		SpriteRenderer img = transform.FindChild("Label").gameObject.GetComponent<SpriteRenderer> ();
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
		
		towerType = data ["towerType"] as string;
		
		if(filename.Equals("piecetower")){
			PieceParser.FillController(this,filename);
			return;
		}
		
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
		doesSplit = (bool)data ["doesSplit"];
		isHoming = (bool)data ["isHoming"];
		doesArc = (bool)data ["doesArc"];
		shieldHP = (float)(double)data ["shieldHP"];
	}
	
	//Assigns skill values to bullets
	private void ConfigureBullet(BulletController bc)
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
		bc.splash = splash;
		bc.stun = stun;
		bc.slowdown = slowdown;
		bc.slowDur = slowDur;
		bc.penetration = penetration;
		bc.shieldShred = shieldShred;
		bc.doesSplit = doesSplit;
		bc.isHoming = isHoming;
		bc.doesArc = doesArc;
		Debug.Log ("bullet slowDur is " + bc.slowDur);
	}
	
	//Assigns skill values to traps
	private void ConfigureTrap(TrapController bc)
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
		bc.stun = stun;
		bc.slowdown = slowdown;
		bc.slowDur = slowDur;
		bc.penetration = penetration;
		bc.shieldShred = shieldShred;
		bc.maxArmingTime = trapArmTime;
	}
	
	//Assigns skill values to shields
	private void ConfigureShield(ShieldController bc)
	{
		if (shieldHP == 0)
			print ("Check your shield HP value!  might be 0!");
		bc.maxHP = shieldHP;
		//bc.regenRate = shieldRegen; //commented out since regen rate doesn't vary, according to joe
	}
	
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
		Debug.Log ("Set tower damage to " + dmg);
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
	public void SetSplash(float pSplash)
	{
		splash = pSplash;
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
	public void SetShieldShred(float pShieldShred)
	{
		shieldShred = pShieldShred;
	}
	public void SetTrapArmTime(float pTrapArmTime)
	{
		trapArmTime = pTrapArmTime;
	}
	public void SetSpread(int pSpread)
	{
		spread = pSpread;
	}
	public void SetDoesSplit(bool pDoesSplit)
	{
		doesSplit = pDoesSplit;
	}
	public void SetIsHoming(bool pIsHoming)
	{
		isHoming = pIsHoming;
	}
	public void SetDoesArc(bool pDoesArc)
	{
		doesArc = pDoesArc;
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
}
