using MiniJSON;
using System.IO;
using System;
using System.Collections.Generic;
using UnityEngine;

public class PieceParser{

	static float DAMAGE_DEFAULT = 2.0f;
	public static float SPEED_DEFAULT = 1.0f;
	static float RANGE_DEFAULT = 0.5f;
	static float COOLDOWN_DEFAULT = 2.0f;
	static float KNOCKBACK_DEFAULT = 0f;
	static float DRAIN_DEFAULT = 0f;
	static float POISON_DEFAULT = 0f;
	static float SPLASH_EFFECT_PERCENT_DEFAULT = 0f;
	static float SPLASH_DEFAULT = 0f;
	static float STUN_DEFAULT = 0f;
	static float SLOWDOWN_MIN_DEFAULT = 0f;
	static float SLOWDOWN_MAX_DEFAULT = 0f;
	static float PENETRATION_DEFAULT = 0f;
	static float SHRED_DEFAULT = 0f;
	static int SPREAD_DEFAULT = 1;
	static int SPLIT_TYPE_DEFAULT = 0;
	static float HOMING_STRENGTH_DEFAULT = 0f;
	static float ARC_BOOST_DEFAULT = 0f;

	public static Dictionary<string,float> GetStatsFromGrid(List<string> files){
		Dictionary<string,float> result = new Dictionary<string, float>();
		float damage = DAMAGE_DEFAULT;
		float speed = SPEED_DEFAULT;
		float range = RANGE_DEFAULT;
		float cooldown = COOLDOWN_DEFAULT;
		float knockback = KNOCKBACK_DEFAULT;
		float lifeDrain = DRAIN_DEFAULT;
		float poison = POISON_DEFAULT;
		float splashEffectPercent = SPLASH_EFFECT_PERCENT_DEFAULT;
		float splash = SPLASH_DEFAULT;
		float stun = STUN_DEFAULT;
		float slowdownMin = SLOWDOWN_MIN_DEFAULT;
		float slowdownMax = SLOWDOWN_MAX_DEFAULT;
		float penetration = PENETRATION_DEFAULT;
		float shieldShred = SHRED_DEFAULT;
		int spread = SPREAD_DEFAULT;
		int splitType = SPLIT_TYPE_DEFAULT;
		float homingStrength = HOMING_STRENGTH_DEFAULT;
		float arcBoost = ARC_BOOST_DEFAULT;
		
		int splashBonusCount = 0;
		int penetrationBonusCount = 0;
		int regenBonusCount = 0;
		bool chainStunBonus = false;
		bool chainPoisonBonus = false;
		bool circleExplosionBonus = false;
		bool lethargyPoisonBonus = false;
		bool hijackRegenBonus = false;
		bool recursiveSplitBonus = false;
		
		//count pieces for bonus purposes
		int damageCount = 0;
		int poisonCount = 0;
		int slowCount = 0;
		int peneCount = 0;
		int shredCount = 0;
		int drainCount = 0;
		int speedCount = 0;
		int stunCount = 0;
		int knockbackCount = 0;
		int	rangeCount = 0;
		int splashCount = 0;
		int arcCount = 0;
		int splitCount = 0;
		int spreadCount = 0;
		int homeCount = 0;
		int cdrCount = 0;
		foreach(string piecefile in files){
			FileLoader fl = new FileLoader ("JSONData" + Path.DirectorySeparatorChar + "Pieces",piecefile);
			string json = fl.Read ();
			Dictionary<string,System.Object> pdata = (Dictionary<string,System.Object>)Json.Deserialize (json);
			
			//read values
			//damage - straightforward
			float pdamage = (float)(double)pdata["dmg"];
			if(pdamage > 0.0f)
				damageCount++;
			damage += pdamage;
			//range - in percent of track
			float prange = (float)(double)pdata["range"];
			range += prange;
			if(range > 1.0f)
				range = 1.0f;
			if(range < 0.0f)
				range = 0.0f;
			if(prange > 0.0f)
				rangeCount++;
			//speed - in terms of seconds it takes to reach the end of the track. will need to be converted to actual game velocity
			float pspeed = (float)(double)pdata["speed"];
			speed += pspeed;
			if(speed < 0.1f)
				speed = 0.1f;
			if(pspeed > 0.0f)
				speedCount++;
			//cooldown - as number of seconds
			float pcool = (float)(double)pdata["cooldownFactor"];
			cooldown += pcool;
			if(cooldown < 0.25f)
				cooldown = 0.25f;
			if(pcool < 0.0f)
				cdrCount++;
			//poison - percent of health to remove every 0.5 seconds over the course of 3 seconds
			float ppoison = (float)(double)pdata["poison"];
			if(ppoison > 0.0f)
				poisonCount++;
			//slow - in percent of enemy speed - set enemy speed to this percent
			float pminslow = (float)(double)pdata["slowdownMin"];
			slowdownMin += pminslow;
			float pmaxslow = (float)(double)pdata["slowdownMax"];
			slowdownMax += pmaxslow;
			if(slowdownMin < -1.0f)
				slowdownMin = -1.0f;
			if(slowdownMax < -1.0f)
				slowdownMax = -1.0f; //cap slowdowns so they don't become knockbacks
			if(pmaxslow < 0.0f)
				slowCount++;
			//knockback - in terms of percent progress to roll back. value needs to be converted to an actual speed somehow. will take some fiddling with
			float pknockback = (float)(double)pdata["knockback"];
			knockback += pknockback;
			if(knockback > 0.5f)
				knockback = 0.5f;
			if(pknockback > 0.0f)
				knockbackCount++;
			//life drain - in percent of damage dealt to drain to dial
			float pdrain = (float)(double)pdata["lifeDrain"];
			lifeDrain += pdrain;
			if(lifeDrain > 1.0f)
				lifeDrain = 1.0f;
			if(pdrain > 0.0f)
				drainCount++;
			//splash - not in radius, but in percent of its effects to apply to enemies in AOE
			float psplash = (float)(double)pdata["splash"];
			splash += psplash;
			if(psplash > splashEffectPercent)
				splashEffectPercent = psplash; //only strongest piece is counted
			if(psplash > 0.0f)
				splashCount++;
			//stun - in seconds
			float pstun = (float)(double)pdata["stun"];
			stun += pstun;
			if(stun > 5.0f)
				stun = 5.0f;
			if(pstun > 0.0f)
				stunCount++;
			//penetration - in... percentage of whatever penetration does
			float ppene = (float)(double)pdata["penetration"];
			penetration += ppene;
			if(penetration > 1.0f)
				penetration = 1.0f;
			if(ppene > 0.0f)
				peneCount++;
			//shieldShred - in percentage
			float pshred = (float)(double)pdata["shieldShred"];
			shieldShred += pshred;
			if(shieldShred > 1.0f)
				shieldShred = 1.0f;
			if(pshred > 0.0f)
				shredCount++;
			//spread - spread has different patterns - 1 is normal fire, 2 is side fire, 3 is alternating normal and side fire, and 4 is 3-way fire
			int pspread = (int)(long)pdata["spread"];
			if(pspread > spread)
				spread = pspread;
			if(pspread > 1)
				spreadCount++;
			//split - again pattern-based. 0 is no split, 1-3 are the normal through rare effects described in the Tower Pieces spreadsheet
			int psplit = (int)(long)pdata["split"];
			if(psplit > splitType)
				splitType = psplit;
			if(psplit > 0)
				splitCount++;
			//homing - above 0.0 and it will home, the number describes the strength of the pull
			float phome = (float)(double)pdata["homing"];
			if(phome > homingStrength)
				homingStrength = phome;
			if(phome > 0.0f)
				homeCount++;
			//arc - above 0.0 and it'll arc, the number describes the damage bonus
			float parc = (float)(double)pdata["arc"];
			if(parc > arcBoost)
				arcBoost = parc;
			if(parc > 0.0f)
				arcCount++;
		} 
		
		penetrationBonusCount = Math.Min(speedCount,peneCount);
		splashBonusCount = Math.Min(splashCount,rangeCount);
		regenBonusCount = Math.Min(shredCount,slowCount);
		hijackRegenBonus = Math.Min (drainCount,peneCount) > 0;
		chainStunBonus = Math.Min (stunCount,knockbackCount) > 0;
		chainPoisonBonus = Math.Min (spreadCount,poisonCount) > 0;
		lethargyPoisonBonus = Math.Min (slowCount,poisonCount) > 0;
		recursiveSplitBonus = Math.Min (splitCount,spreadCount) > 0;
		circleExplosionBonus = Math.Min (arcCount,splashCount) > 0;
			
		//put the results in the dictionary
		
		result.Add("damage",damage);
		result.Add("range",range);
		result.Add("speed",speed);
		result.Add("cdr",cooldown);
		result.Add("slowMin",slowdownMin);
		result.Add("slowMax",slowdownMax);
		result.Add("stun",stun);
		result.Add("knockback",knockback);
		result.Add("poison",poison);
		result.Add("shred",shieldShred);
		result.Add("penetration",penetration);
		result.Add("drain",lifeDrain);
		result.Add("split",(float)splitType);
		result.Add("spread",(float)spread);
		result.Add("homing",homingStrength);
		result.Add("arc",arcBoost);
		result.Add("splash",splash);
		//bonuses
		result.Add("pierce",(float)penetrationBonusCount);
		result.Add("aoe",(float)splashBonusCount);
		result.Add("antiregen",(float)regenBonusCount);
		if(hijackRegenBonus)
			result.Add("hijackregen",1f);
		else
			result.Add("hijackregen",-1f);
		if(chainStunBonus)
			result.Add("chainstun",1f);
		else
			result.Add("chainstun",-1f);
		if(chainPoisonBonus)
			result.Add("chainpoison",1f);
		else
			result.Add("chainpoison",-1f);
		if(lethargyPoisonBonus)
			result.Add("lethargy",1f);
		else
			result.Add("lethargy",-1f);
		if(recursiveSplitBonus)
			result.Add("divide",1f);
		else
			result.Add("divide",-1f);
		if(circleExplosionBonus)
			result.Add("circle",1f);
		else
			result.Add("circle",-1f);
		
		return result;
	}
	public static float SPEED_CONSTANT = 5f;
	//DUPLICATE METHOD USING GUN INSTEAD OF GUNCONTROLLER
	public static void FillController(Gun gc, string filename){
		//FileLoader fl = new FileLoader ("JSONData" + Path.DirectorySeparatorChar + "Towers",filename);
		FileLoader fl = new FileLoader (Application.persistentDataPath,"Towers",filename);
		string json = fl.Read ();
		Dictionary<string,System.Object> data = (Dictionary<string,System.Object>)Json.Deserialize (json);
		
		List<System.Object> pieces = data["pieces"] as List<System.Object>;
		//ignore the anchor and rotation data in here
		List<string> pieceFilenames = new List<string>();
		foreach(System.Object pieceObj in pieces){
			Dictionary<string,System.Object> pieceJson = pieceObj as Dictionary<string,System.Object>;
			string s = (string)pieceJson["pieceFilename"];
			pieceFilenames.Add(s);
		}
		
		//defaults
		//ALL THESE VALUES ARE NOT ACTUAL BULLET PROPERTIES
		//VALUES MUST BE CONVERTED TO APPROPRIATE TOWER TYPE
		float damage = DAMAGE_DEFAULT;
		float speed = SPEED_DEFAULT;
		float range = RANGE_DEFAULT;
		float cooldown = COOLDOWN_DEFAULT;
		float knockback = KNOCKBACK_DEFAULT;
		float lifeDrain = DRAIN_DEFAULT;
		float poison = POISON_DEFAULT;
		float splashEffectPercent = SPLASH_EFFECT_PERCENT_DEFAULT;
		float splash = SPLASH_DEFAULT;
		float stun = STUN_DEFAULT;
		float slowdownMin = SLOWDOWN_MIN_DEFAULT;
		float slowdownMax = SLOWDOWN_MAX_DEFAULT;
		float penetration = PENETRATION_DEFAULT;
		float shieldShred = SHRED_DEFAULT;
		int spread = SPREAD_DEFAULT;
		int splitType = SPLIT_TYPE_DEFAULT;
		float homingStrength = HOMING_STRENGTH_DEFAULT;
		float arcBoost = ARC_BOOST_DEFAULT;		
		
		int splashBonusCount = 0;
		int penetrationBonusCount = 0;
		int regenBonusCount = 0;
		bool chainStunBonus = false;
		bool chainPoisonBonus = false;
		bool circleExplosionBonus = false;
		bool lethargyPoisonBonus = false;
		bool hijackRegenBonus = false;
		bool recursiveSplitBonus = false;
		
		//count pieces for bonus purposes
		int damageCount = 0;
		int poisonCount = 0;
		int slowCount = 0;
		int peneCount = 0;
		int shredCount = 0;
		int drainCount = 0;
		int speedCount = 0;
		int stunCount = 0;
		int knockbackCount = 0;
		int	rangeCount = 0;
		int splashCount = 0;
		int arcCount = 0;
		int splitCount = 0;
		int spreadCount = 0;
		int homeCount = 0;
		int cdrCount = 0;
		
		/*
		float pdamage = 5;
		float prange = 0;
		float pspeed = 0;
		float pcool = 0;
		float ppoison = 0;
		float pminslow = 0;
		float pmaxslow = 0;
		float pknockback = 0;
		float pdrain = 0;
		float psplash = 0;
		float pstun = 0;
		float ppene = 0;
		float pshred = 0;
		int pspread = 1;
		int psplit = 0;
		float phome = 0;
		float parc = 0;
		*/
		
		foreach(string pfilename in pieceFilenames){
			FileLoader pieceLoader = new FileLoader("JSONData" + Path.DirectorySeparatorChar + "Pieces", pfilename);
			string pieceJson = pieceLoader.Read();
			Dictionary<string,System.Object> pdata = (Dictionary<string,System.Object>)Json.Deserialize(pieceJson);
			
			//read values
			//damage - straightforward
			float pdamage = (float)(double)pdata["dmg"];
			if(pdamage > 0.0f)
				damageCount++;
			damage += pdamage;
			if(damage < 0)
				damage = 0f;
			//range - in percent of track
			float prange = (float)(double)pdata["range"];
			range += prange;
			if(range > 1.0f)
				range = 1.0f;
			if(range < 0.0f)
				range = 0.0f;
			if(prange > 0.0f)
				rangeCount++;
			//speed - in terms of seconds it takes to reach the end of the track. will need to be converted to actual game velocity
			float pspeed = (float)(double)pdata["speed"];
			speed += pspeed;
			if(speed < 0.1f)
				speed = 0.1f;
			if(pspeed > 0.0f)
				speedCount++;
			//cooldown - as number of seconds
			float pcool = (float)(double)pdata["cooldownFactor"];
			cooldown += pcool;
			if(cooldown < 0.25f)
				cooldown = 0.25f;
			if(pcool < 0.0f)
				cdrCount++;
			//poison - percent of health to remove every 0.5 seconds over the course of 3 seconds
			float ppoison = (float)(double)pdata["poison"];
			poison += ppoison;
			if(ppoison > 0.0f)
				poisonCount++;
			//slow - in percent of enemy speed - set enemy speed to this percent
			float pminslow = (float)(double)pdata["slowdownMin"];
			slowdownMin += pminslow;
			float pmaxslow = (float)(double)pdata["slowdownMax"];
			slowdownMax += pmaxslow;
			if(slowdownMin < -1.0f)
				slowdownMin = -1.0f;
			if(slowdownMax < -1.0f)
				slowdownMax = -1.0f; //cap slowdowns so they don't become knockbacks
			if(pmaxslow < 0.0f)
				slowCount++;
			//knockback - in terms of percent progress to roll back. value needs to be converted to an actual speed somehow. will take some fiddling with
			float pknockback = (float)(double)pdata["knockback"];
			knockback += pknockback;
			if(knockback > 0.5f)
				knockback = 0.5f;
			if(pknockback > 0.0f)
				knockbackCount++;
			//life drain - in percent of damage dealt to drain to dial
			float pdrain = (float)(double)pdata["lifeDrain"];
			lifeDrain += pdrain;
			if(lifeDrain > 1.0f)
				lifeDrain = 1.0f;
			if(pdrain > 0.0f)
				drainCount++;
			//splash - not in radius, but in percent of its effects to apply to enemies in AOE
			float psplash = (float)(double)pdata["splash"];
			splash += psplash;
			if(psplash > splashEffectPercent)
				splashEffectPercent = psplash; //only strongest piece is counted
			if(psplash > 0.0f)
				splashCount++;
			//stun - in seconds
			float pstun = (float)(double)pdata["stun"];
			stun += pstun;
			if(stun > 5.0f)
				stun = 5.0f;
			if(pstun > 0.0f)
				stunCount++;
			//penetration - in... percentage of whatever penetration does
			float ppene = (float)(double)pdata["penetration"];
			penetration += ppene;
			if(penetration > 1.0f)
				penetration = 1.0f;
			if(ppene > 0.0f)
				peneCount++;
			//shieldShred - in percentage
			float pshred = (float)(double)pdata["shieldShred"];
			shieldShred += pshred;
			if(shieldShred > 1.0f)
				shieldShred = 1.0f;
			if(pshred > 0.0f)
				shredCount++;
			//spread - spread has different patterns - 1 is normal fire, 2 is side fire, 3 is alternating normal and side fire, and 4 is 3-way fire
			int pspread = (int)(long)pdata["spread"];
			if(pspread > spread)
				spread = pspread;
			if(pspread > 1)
				spreadCount++;
			//split - again pattern-based. 0 is no split, 1-3 are the normal through rare effects described in the Tower Pieces spreadsheet
			int psplit = (int)(long)pdata["split"];
			if(psplit > splitType)
				splitType = psplit;
			if(psplit > 0)
				splitCount++;
			//homing - above 0.0 and it will home, the number describes the strength of the pull
			float phome = (float)(double)pdata["homing"];
			if(phome > homingStrength)
				homingStrength = phome;
			if(phome > 0.0f)
				homeCount++;
			//arc - above 0.0 and it'll arc, the number describes the damage bonus
			float parc = (float)(double)pdata["arc"];
			if(parc > arcBoost)
				arcBoost = parc;
			if(parc > 0.0f)
				arcCount++;
		} 
		
		penetrationBonusCount = Math.Min(speedCount,peneCount);
		splashBonusCount = Math.Min(splashCount,rangeCount);
		regenBonusCount = Math.Min(shredCount,slowCount);
		hijackRegenBonus = Math.Min (drainCount,peneCount) > 0;
		chainStunBonus = Math.Min (stunCount,knockbackCount) > 0;
		chainPoisonBonus = Math.Min (spreadCount,poisonCount) > 0;
		lethargyPoisonBonus = Math.Min (slowCount,poisonCount) > 0;
		recursiveSplitBonus = Math.Min (splitCount,spreadCount) > 0;
		circleExplosionBonus = Math.Min (arcCount,splashCount) > 0;
		
		//DONE GETTING RAW VALUES
		
		//Set tower stats based on these values
		if(gc.GetTowerType().Equals ("Bullet")){
			Debug.Log("speed is " + ((1f/speed) * SPEED_CONSTANT));
			//Damage
			gc.SetDmg (damage);
			//Range
			gc.SetRange (range);
			//Speed
			gc.SetSpeed ((1f/speed) * SPEED_CONSTANT);
			//Cooldown
			gc.SetCooldown (cooldown);
			//Poison
			gc.SetPoison (poison);
			Debug.Log (gc.buttonID + " poison value is set to " + poison);
			gc.SetPoisonDur (3f);
			//Slowdown
			gc.SetSlowdown (slowdownMax); //for now.  eventually add in that scaling system for slow/fast enemies?
			gc.SetSlowDur (0.75f);
			//Knockback
			gc.SetKnockback(knockback);
			//Lifedrain
			gc.SetLifeDrain (lifeDrain);
			//Splash
			gc.SetSplash (splash);
			//Stun
			gc.SetStun (stun);
			//Penetration
			gc.SetPenetration (penetration);
			//Shieldshred
			gc.SetShieldShred (shieldShred);
			//Spread
			gc.SetSpread(spread);
			//SplitCount
			gc.SetSplit (splitType);
			//Homing
			gc.SetIsHoming (homingStrength);
			//ArcStrength
			gc.SetDoesArc (arcBoost);
		}else if(gc.GetTowerType().Equals("Trap")){
            //***GET TRAP SCALARS FROM JOE***
            Debug.Log("speed is " + ((1f / speed) * SPEED_CONSTANT));
            //Damage
            gc.SetDmg(damage);
            //Range
            gc.SetRange(range);
            //Speed
            gc.SetSpeed(speed * SPEED_CONSTANT);
            //Cooldown
            gc.SetCooldown(cooldown);
            //Poison
            gc.SetPoison(poison);
            Debug.Log(gc.buttonID + " poison value is set to " + poison);
            gc.SetPoisonDur(3f);
            //Slowdown
            gc.SetSlowdown(slowdownMax); //for now.  eventually add in that scaling system for slow/fast enemies?
            gc.SetSlowDur(0.75f);
            //Knockback
            gc.SetKnockback(knockback);
            //Lifedrain
            gc.SetLifeDrain(lifeDrain);
            //Splash
            gc.SetSplash(splash);
            //Stun
            gc.SetStun(stun);
            //Penetration
            gc.SetPenetration(penetration);
            //Shieldshred
            gc.SetShieldShred(shieldShred);
            //Spread
            gc.SetSpread(spread);
            //SplitCount
            gc.SetSplit(splitType);
            //Homing
            gc.SetIsHoming(homingStrength);
            //ArcStrength
            gc.SetDoesArc(arcBoost);

        }
        else if(gc.GetTowerType().Equals ("Shield")){
			
		}
	}
}
