/*Duncan*/

using MiniJSON;
using System.IO;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PieceParser{

	static float DAMAGE_DEFAULT = 2.0f;
	public static float SPEED_DEFAULT = 1.0f;
	static float RANGE_DEFAULT = 0.5f;
	static float COOLDOWN_DEFAULT = 0.1f;
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
	
	static float MAXIMUM_SLOWDOWN = .8f;
	static float PERCENT_AOE_RANGE_PER_PAIR = .25f;
	static float PERCENT_SHIELD_REGEN_SLOW_PER_PAIR = .25f;
    static float COOLDOWN_PER_SQUARE = .1f;

    static float SHIELD_HP_DEFAULT = 20.0f;
    static float SHIELD_SPEED_DEFAULT = 0f;
    static float SHIELD_RANGE_DEFAULT = 0f;
    static float SHIELD_COOLDOWN_DEFAULT = 2f;
    static float SHIELD_KNOCKBACK_DEFAULT = 0f;
    static float SHIELD_DRAIN_DEFAULT = 0f;
    static float SHIELD_POISON_DEFAULT = 0f;
    static float SHIELD_SPLASH_EFFECT_PERCENT_DEFAULT = 0f;
    static float SHIELD_SPLASH_DEFAULT = 0f;
    static float SHIELD_STUN_DEFAULT = 0f;
    static float SHIELD_SLOWDOWN_MIN_DEFAULT = 0f;
    static float SHIELD_SLOWDOWN_MAX_DEFAULT = 0f;
    static float SHIELD_PENETRATION_DEFAULT = 0f;
    static float SHIELD_SHRED_DEFAULT = 0f;
    static int SHIELD_SPREAD_DEFAULT = 1;
    static int SHIELD_SPLIT_DEFAULT = 0;
    static float SHIELD_HOMING_DEFAULT = 0f;
    static float SHIELD_ARC_DEFAULT = 0f;

    static float TRAP_DMG_DEFAULT = 5f;
    static float TRAP_SPEED_DEFAULT = 2f;
    static float TRAP_RANGE_DEFAULT = 2f;
    static float TRAP_COOLDOWN_DEFAULT = 2f;
    static float TRAP_KNOCKBACK_DEFAULT = 0f;
    static float TRAP_DRAIN_DEFAULT = 0f;
    static float TRAP_POISON_DEFAULT = 0f;
    static float TRAP_SPLASH_EFFECT_PERCENT_DEFAULT = 0f;
    static float TRAP_SPLASH_DEFAULT = 0f;
    static float TRAP_STUN_DEFAULT = 0f;
    static float TRAP_SLOWDOWN_MIN_DEFAULT = 0f;
    static float TRAP_SLOWDOWN_MAX_DEFAULT = 0f;
    static float TRAP_PENETRATION_DEFAULT = 0f;
    static float TRAP_SHRED_DEFAULT = 0f;
    static int TRAP_SPREAD_DEFAULT = 1;
    static int TRAP_SPLIT_DEFAULT = 0;
    static float TRAP_HOMING_DEFAULT = 0f;
    static float TRAP_ARC_DEFAULT = 0f;

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

        float shieldHP = SHIELD_HP_DEFAULT;
        float shieldSpeed = SHIELD_SPEED_DEFAULT;
        float shieldRange = SHIELD_RANGE_DEFAULT;
        float shieldCooldown = SHIELD_COOLDOWN_DEFAULT;
        float shieldKnockback = SHIELD_KNOCKBACK_DEFAULT;
        float shieldLifeDrain = SHIELD_DRAIN_DEFAULT;
        float shieldPoison = SHIELD_POISON_DEFAULT;
        float shieldSplashEffectPercent = SHIELD_SPLASH_EFFECT_PERCENT_DEFAULT;
        float shieldSplash = SHIELD_SPLASH_DEFAULT;
        float shieldStun = SHIELD_STUN_DEFAULT;
        float shieldSlowdownMin = SHIELD_SLOWDOWN_MIN_DEFAULT;
        float shieldSlowdownMax = SHIELD_SLOWDOWN_MAX_DEFAULT;
        float shieldPenetration = SHIELD_PENETRATION_DEFAULT;
        float shieldShieldShred = SHIELD_SHRED_DEFAULT;
        int shieldSpread = SHIELD_SPREAD_DEFAULT;
        float shieldHoming = SHIELD_HOMING_DEFAULT;
        float shieldArc = SHIELD_ARC_DEFAULT;
        int shieldSplit = SHIELD_SPLIT_DEFAULT;

        float trapDmg = TRAP_DMG_DEFAULT;
        float trapSpeed = TRAP_SPEED_DEFAULT;
        float trapRange = TRAP_RANGE_DEFAULT;
        float trapCooldown = TRAP_COOLDOWN_DEFAULT;
        float trapKnockback = TRAP_KNOCKBACK_DEFAULT;
        float trapLifeDrain = TRAP_DRAIN_DEFAULT;
        float trapPoison = TRAP_POISON_DEFAULT;
        float trapSplashEffectPercent = TRAP_SPLASH_EFFECT_PERCENT_DEFAULT;
        float trapSplash = TRAP_SPLASH_DEFAULT;
        float trapStun = TRAP_STUN_DEFAULT;
        float trapSlowdownMin = TRAP_SLOWDOWN_MIN_DEFAULT;
        float trapSlowdownMax = TRAP_SLOWDOWN_MAX_DEFAULT;
        float trapPenetration = TRAP_PENETRATION_DEFAULT;
        float trapShieldShred = TRAP_SHRED_DEFAULT;
        int trapSpread = TRAP_SPREAD_DEFAULT;
        int trapSplit = TRAP_SPLIT_DEFAULT;
        float trapHoming = TRAP_HOMING_DEFAULT;
        float trapArc = TRAP_ARC_DEFAULT;
		
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

            //add in cooldown time for each grid square taken by the piece
            List<System.Object> superList = (List<System.Object>)pdata["blockMap"];
            foreach(System.Object listObj in superList)
            {
                List<System.Object> pieceParts = (List<System.Object>)listObj;
                foreach(System.Object part in pieceParts)
                {
                    int partSpace = (int)(long)part;
                    if (partSpace == 1) //full squares
                    {
                        cooldown += 0.1f;
                    }
                    else if (partSpace >= 2) //triangles
                    {
                        cooldown += 0.05f;
                    }
                }
            }

			if(cooldown < 0.1f)
				cooldown = 0.1f;
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

        float shieldHP = SHIELD_HP_DEFAULT;
        float shieldSpeed = SHIELD_SPEED_DEFAULT;
        float shieldRange = SHIELD_RANGE_DEFAULT;
        float shieldCooldown = SHIELD_COOLDOWN_DEFAULT;
        float shieldKnockback = SHIELD_KNOCKBACK_DEFAULT;
        float shieldLifeDrain = SHIELD_DRAIN_DEFAULT;
        float shieldPoison = SHIELD_POISON_DEFAULT;
        float shieldSplashEffectPercent = SHIELD_SPLASH_EFFECT_PERCENT_DEFAULT;
        float shieldSplash = SHIELD_SPLASH_DEFAULT;
        float shieldStun = SHIELD_STUN_DEFAULT;
        float shieldSlowdownMin = SHIELD_SLOWDOWN_MIN_DEFAULT;
        float shieldSlowdownMax = SHIELD_SLOWDOWN_MAX_DEFAULT;
        float shieldPenetration = SHIELD_PENETRATION_DEFAULT;
        float shieldShieldShred = SHIELD_SHRED_DEFAULT;
        int shieldSpread = SHIELD_SPREAD_DEFAULT;
        float shieldHoming = SHIELD_HOMING_DEFAULT;
        float shieldArc = SHIELD_ARC_DEFAULT;
        int shieldSplit = SHIELD_SPLIT_DEFAULT;

        float trapDmg = TRAP_DMG_DEFAULT;
        float trapSpeed = TRAP_SPEED_DEFAULT; //using this for Arm Time
        float trapRange = TRAP_RANGE_DEFAULT;
        float trapCooldown = TRAP_COOLDOWN_DEFAULT;
        float trapKnockback = TRAP_KNOCKBACK_DEFAULT;
        float trapLifeDrain = TRAP_DRAIN_DEFAULT;
        float trapPoison = TRAP_POISON_DEFAULT;
        float trapSplashEffectPercent = TRAP_SPLASH_EFFECT_PERCENT_DEFAULT;
        float trapSplash = TRAP_SPLASH_DEFAULT;
        float trapStun = TRAP_STUN_DEFAULT;
        float trapSlowdownMin = TRAP_SLOWDOWN_MIN_DEFAULT;
        float trapSlowdownMax = TRAP_SLOWDOWN_MAX_DEFAULT;
        float trapPenetration = TRAP_PENETRATION_DEFAULT;
        float trapShieldShred = TRAP_SHRED_DEFAULT;
        int trapSplit = TRAP_SPLIT_DEFAULT;
        int trapSpread = TRAP_SPREAD_DEFAULT;
        float trapHoming = TRAP_HOMING_DEFAULT;
        float trapArc = TRAP_ARC_DEFAULT;

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
			if(pspeed < 0.0f)
				speedCount++;
			//cooldown - as number of seconds
			float pcool = (float)(double)pdata["cooldownFactor"];
			cooldown += pcool;

            //add in cooldown time for each grid square taken by the piece
            List<System.Object> superList = (List<System.Object>)pdata["blockMap"];
            foreach (System.Object listObj in superList)
            {
                List<System.Object> pieceParts = (List<System.Object>)listObj;
                foreach (System.Object part in pieceParts)
                {
                    int partSpace = (int)(long)part;
                    if (partSpace == 1) //full squares
                    {
                        cooldown += 0.1f;
                    }
                    else if (partSpace >= 2) //triangles
                    {
                        cooldown += 0.05f;
                    }
                }
            }

            if (cooldown < 0.1f)
				cooldown = 0.1f;
			if(pcool < 0.0f)
				cdrCount++;
			//poison - percent of health to remove every 0.5 seconds over the course of 3 seconds
			float ppoison = (float)(double)pdata["poison"];
			poison += ppoison;
			if(ppoison > 0.0f)
				poisonCount++;
			//slow - in percent of enemy speed - set enemy speed to this percent
			float pminslow = (float)(double)pdata["slowdownMin"];
			if(pminslow > 0)
				slowdownMin += 1f-pminslow;
			float pmaxslow = (float)(double)pdata["slowdownMax"];
			if(pmaxslow > 0)
				slowdownMax += 1f-pmaxslow;
			if(slowdownMin > MAXIMUM_SLOWDOWN)
				slowdownMin = MAXIMUM_SLOWDOWN;
			if(slowdownMax > MAXIMUM_SLOWDOWN)
				slowdownMax = MAXIMUM_SLOWDOWN; //cap slowdowns so they don't become knockbacks
			if(pmaxslow > 0.0f)
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

            //***Same thing, but for SHIELD stats
            //read values
            //damage - straightforward
            float pShieldHP = (float)(double)pdata["shieldMaxHP"];
            if (pShieldHP > 0.0f)
                damageCount++;
            shieldHP += pShieldHP;
            if (shieldHP < 0)
                shieldHP = 0f;
            //range - in percent of track
            float pShieldRange = (float)(double)pdata["shieldRange"];
            shieldRange += pShieldRange;
            if (shieldRange > 1.0f)
                shieldRange = 1.0f;
            if (shieldRange < 0.0f)
                shieldRange = 0.0f;
            if (pShieldRange > 0.0f)
                rangeCount++;
            //speed - in terms of seconds it takes to reach the end of the track. will need to be converted to actual game velocity
            float pShieldSpeed = (float)(double)pdata["shieldSpeed"];
            shieldSpeed += pShieldSpeed;
            if (shieldSpeed < 0.1f)
                shieldSpeed = 0.1f;
            if (pShieldSpeed < 0.0f)
                speedCount++;
            //poison - percent of health to remove every 0.5 seconds over the course of 3 seconds
            float pShieldPoison = (float)(double)pdata["shieldPoison"];
            shieldPoison += pShieldPoison;
            if (pShieldPoison > 0.0f)
                poisonCount++;
            //slow - in percent of enemy speed - set enemy speed to this percent
            float pShieldMinslow = (float)(double)pdata["shieldSlowdownMin"];
            if (pShieldMinslow > 0)
                shieldSlowdownMin += 1f - pShieldMinslow;
            float pShieldMaxslow = (float)(double)pdata["shieldSlowdownMax"];
            if (pShieldMaxslow > 0)
                shieldSlowdownMax += 1f - pShieldMaxslow;
            if (shieldSlowdownMin > MAXIMUM_SLOWDOWN)
                shieldSlowdownMin = MAXIMUM_SLOWDOWN;
            if (shieldSlowdownMax > MAXIMUM_SLOWDOWN)
                shieldSlowdownMax = MAXIMUM_SLOWDOWN; //cap slowdowns so they don't become knockbacks
            if (pmaxslow > 0.0f)
                slowCount++;
            //knockback - in terms of percent progress to roll back. value needs to be converted to an actual speed somehow. will take some fiddling with
            float pShieldKnockback = (float)(double)pdata["shieldKnockback"];
            shieldKnockback += pShieldKnockback;
            if (shieldKnockback > 0.5f)
                shieldKnockback = 0.5f;
            if (pShieldKnockback > 0.0f)
                knockbackCount++;
            //life drain - in percent of damage dealt to drain to dial
            float pShieldDrain = (float)(double)pdata["shieldLifeDrain"];
            shieldLifeDrain += pShieldDrain;
            if (shieldLifeDrain > 1.0f)
                shieldLifeDrain = 1.0f;
            if (pShieldDrain > 0.0f)
                drainCount++;
            //splash - not in radius, but in percent of its effects to apply to enemies in AOE
            float pShieldSplash = (float)(double)pdata["shieldSplashDmg"];
            shieldSplash += pShieldSplash;
            if (pShieldSplash > shieldSplashEffectPercent)
                shieldSplashEffectPercent = pShieldSplash; //only strongest piece is counted
            if (pShieldSplash > 0.0f)
                splashCount++;
            //stun - in seconds
            float pShieldStun = (float)(double)pdata["shieldStun"];
            shieldStun += pShieldStun;
            if (shieldStun > 5.0f)
                shieldStun = 5.0f;
            if (pShieldStun > 0.0f)
                stunCount++;
            //penetration - in... percentage of whatever penetration does
            float pShieldPene = (float)(double)pdata["shieldPen"];
            shieldPenetration += pShieldPene;
            if (shieldPenetration > 1.0f)
                shieldPenetration = 1.0f;
            if (pShieldPene > 0.0f)
                peneCount++;
            //shieldShred - in percentage
            float pShieldShred = (float)(double)pdata["shieldShieldShred"];
            shieldShieldShred += pShieldShred;
            if (shieldShieldShred > 1.0f)
                shieldShieldShred = 1.0f;
            if (pShieldShred > 0.0f)
                shredCount++;
            //spread - spread has different patterns - 1 is normal fire, 2 is side fire, 3 is alternating normal and side fire, and 4 is 3-way fire
            float pShieldSpread = (float)(double)pdata["shieldSpread"];
            if (pShieldSpread > shieldSpread)
                shieldSpread = (int)pShieldSpread;
            if (pShieldSpread > 1)
                spreadCount++;
            //split - again pattern-based. 0 is no split, 1-3 are the normal through rare effects described in the Tower Pieces spreadsheet
            float pShieldSplit = (float)(double)pdata["shieldSplit"];
            if (pShieldSplit > shieldSplit)
                shieldSplit = (int)pShieldSplit;
            if (pShieldSplit > 0)
                splitCount++;
            //homing - above 0.0 and it will home, the number describes the strength of the pull
            float pShieldHome = (float)(double)pdata["shieldHoming"];
            if (pShieldHome > shieldHoming)
                shieldHoming = pShieldHome;
            if (pShieldHome > 0.0f)
                homeCount++;
            //arc - above 0.0 and it'll arc, the number describes the damage bonus
            float pShieldArc = (float)(double)pdata["shieldArc"];
            if (pShieldArc > shieldArc)
                shieldArc = pShieldArc;
            if (pShieldArc > 0.0f)
                arcCount++;

            //Same thing, but for Trap stats
            //read values
            //damage - straightforward
            float pTrapDamage = (float)(double)pdata["trapDmg"];
            if (pTrapDamage > 0.0f)
                damageCount++;
            trapDmg += pTrapDamage;
            if (trapDmg < 0)
                trapDmg = 0f;
            //range - in percent of track
            float pTrapRange = (float)(double)pdata["trapRange"];
            trapRange += pTrapRange;
            if (trapRange > 1.0f)
                trapRange = 1.0f;
            if (trapRange < 0.0f)
                trapRange = 0.0f;
            if (pTrapRange > 0.0f)
                rangeCount++;
            //speed - in terms of seconds it takes to reach the end of the track. will need to be converted to actual game velocity
            float pTrapSpeed = (float)(double)pdata["trapMaxArmTime"];
            trapSpeed += pTrapSpeed;
            if (trapSpeed < 0.1f)
                trapSpeed = 0.1f;
            if (pTrapSpeed < 0.0f)
                speedCount++;
            //poison - percent of health to remove every 0.5 seconds over the course of 3 seconds
            float pTrapPoison = (float)(double)pdata["trapPoison"];
            trapPoison += pTrapPoison;
            if (pTrapPoison > 0.0f)
                poisonCount++;
            //slow - in percent of enemy speed - set enemy speed to this percent
            float pTrapMinslow = (float)(double)pdata["trapSlowdownMin"];
            if (pTrapMinslow > 0)
                trapSlowdownMin += 1f - pTrapMinslow;
            float pTrapMaxslow = (float)(double)pdata["trapSlowdownMax"];
            if (pTrapMaxslow > 0)
                trapSlowdownMax += 1f - pTrapMaxslow;
            if (trapSlowdownMin > MAXIMUM_SLOWDOWN)
                trapSlowdownMin = MAXIMUM_SLOWDOWN;
            if (trapSlowdownMax > MAXIMUM_SLOWDOWN)
                trapSlowdownMax = MAXIMUM_SLOWDOWN; //cap slowdowns so they don't become knockbacks
            if (pTrapMaxslow > 0.0f)
                slowCount++;
            //knockback - in terms of percent progress to roll back. value needs to be converted to an actual speed somehow. will take some fiddling with
            float pTrapKnockback = (float)(double)pdata["trapKnockback"];
            trapKnockback += pTrapKnockback;
            if (trapKnockback > 0.5f)
                trapKnockback = 0.5f;
            if (pTrapKnockback > 0.0f)
                knockbackCount++;
            //life drain - in percent of damage dealt to drain to dial
            float pTrapDrain = (float)(double)pdata["trapLifeDrain"];
            lifeDrain += pdrain;
            if (trapLifeDrain > 1.0f)
                trapLifeDrain = 1.0f;
            if (pTrapDrain > 0.0f)
                drainCount++;
            //splash - not in radius, but in percent of its effects to apply to enemies in AOE
            float pTrapSplash = (float)(double)pdata["trapSplashDmg"];
            trapSplash += pTrapSplash;
            if (pTrapSplash > trapSplashEffectPercent)
                trapSplashEffectPercent = pTrapSplash; //only strongest piece is counted
            if (pTrapSplash > 0.0f)
                splashCount++;
            //stun - in seconds
            float pTrapStun = (float)(double)pdata["trapStun"];
            trapStun += pTrapStun;
            if (trapStun > 5.0f)
                trapStun = 5.0f;
            if (pTrapStun > 0.0f)
                stunCount++;
            //penetration - in... percentage of whatever penetration does
            float pTrapPene = (float)(double)pdata["trapPenetration"];
            trapPenetration += pTrapPene;
            if (trapPenetration > 1.0f)
                trapPenetration = 1.0f;
            if (pTrapPene > 0.0f)
                peneCount++;
            //shieldShred - in percentage
            float pTrapShred = (float)(double)pdata["trapShieldShred"];
            trapShieldShred += pTrapShred;
            if (trapShieldShred > 1.0f)
                trapShieldShred = 1.0f;
            if (pTrapShred > 0.0f)
                shredCount++;
            //spread - spread has different patterns - 1 is normal fire, 2 is side fire, 3 is alternating normal and side fire, and 4 is 3-way fire
            float pTrapSpread = (float)(double)pdata["trapSpread"];
            if (pTrapSpread > trapSpread)
                trapSpread = (int)pTrapSpread;
            if (pTrapSpread > 1)
                spreadCount++;
            //split - again pattern-based. 0 is no split, 1-3 are the normal through rare effects described in the Tower Pieces spreadsheet
            float pTrapSplit = (float)(double)pdata["trapSplit"];
            if (pTrapSplit > trapSplit)
                trapSplit = (int)pTrapSplit;
            if (pTrapSplit > 0)
                splitCount++;
            //homing - above 0.0 and it will home, the number describes the strength of the pull
            float pTrapHome = (float)(double)pdata["trapHoming"];
            if (pTrapHome > trapHoming)
                trapHoming = pTrapHome;
            if (pTrapHome > 0.0f)
                homeCount++;
            //arc - above 0.0 and it'll arc, the number describes the damage bonus
            float pTrapArc = (float)(double)pdata["trapArc"];
            if (pTrapArc > trapArc)
                trapArc = pTrapArc;
            if (pTrapArc > 0.0f)
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
			//Debug.Log("speed is " + ((1f/speed) * SPEED_CONSTANT));
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
			//Debug.Log (gc.buttonID + " poison value is set to " + poison);
			gc.SetPoisonDur (3f);
			gc.SetChainPoison(chainPoisonBonus);
			gc.SetLeeches(hijackRegenBonus);
			//Slowdown
			gc.SetSlowdown (1f-slowdownMax); //for now.  eventually add in that scaling system for slow/fast enemies?
			//Debug.Log (slowdownMax + "is max slowdown");
			gc.SetSlowDur (0.75f);
			//Knockback
			gc.SetKnockback(knockback);
			//Lifedrain
			gc.SetLifeDrain (lifeDrain);
			//Splash
			gc.SetSplash (splash);
			gc.SetSplashRadiusBonus(splashBonusCount * PERCENT_AOE_RANGE_PER_PAIR);
			//Stun
			gc.SetStun (stun);
			//Penetration
			gc.SetPenetration (penetration);
			gc.SetPiercing(penetrationBonusCount);
			//Shieldshred
			gc.SetShieldShred (shieldShred);
			gc.SetShieldSlow(regenBonusCount*PERCENT_SHIELD_REGEN_SLOW_PER_PAIR);
			//Spread
			gc.SetSpread(spread);
			//SplitCount
			gc.SetSplit (splitType);
			gc.SetMultiSplit(recursiveSplitBonus);
			//Homing
			gc.SetIsHoming (homingStrength);
			//ArcStrength
			gc.SetDoesArc (arcBoost);
		}else if(gc.GetTowerType().Equals("Trap")){
            //***GET TRAP SCALARS FROM JOE***
            //Debug.Log("speed is " + ((1f / speed) * SPEED_CONSTANT));
            //Damage
            gc.SetDmg(trapDmg);
            //Range
            gc.SetRange(trapRange);
            //Speed
            gc.SetSpeed(trapSpeed);
            //Cooldown
            gc.SetCooldown(trapCooldown);
            //Poison
            gc.SetPoison(trapPoison);
            //Debug.Log(gc.buttonID + " poison value is set to " + poison);
            gc.SetPoisonDur(3f);
			gc.SetChainPoison(chainPoisonBonus);
			gc.SetLeeches(hijackRegenBonus);
            //Slowdown
            gc.SetSlowdown(trapSlowdownMax); //for now.  eventually add in that scaling system for slow/fast enemies?
            gc.SetSlowDur(0.75f);
            //Knockback
            gc.SetKnockback(trapKnockback);
            //Lifedrain
            gc.SetLifeDrain(trapLifeDrain);
            //Splash
            gc.SetSplash(trapSplash);
			gc.SetSplashRadiusBonus(splashBonusCount * PERCENT_AOE_RANGE_PER_PAIR);
            //Stun
            gc.SetStun(trapStun);
            //Penetration
            gc.SetPenetration(trapPenetration);
			gc.SetPiercing(penetrationBonusCount);
            //Shieldshred
            gc.SetShieldShred(trapShieldShred);
			gc.SetShieldSlow(regenBonusCount*PERCENT_SHIELD_REGEN_SLOW_PER_PAIR);
            //Spread
            gc.SetSpread(trapSpread);
            //SplitCount
            gc.SetSplit(trapSplit);
            //Homing
            gc.SetIsHoming(trapHoming);
            //ArcStrength
            gc.SetDoesArc(trapArc);

        }
        else if(gc.GetTowerType().Equals ("Shield")){
            //HP
            gc.SetShieldHP(shieldHP);
            //Range
            gc.SetRange(shieldRange);
            //Speed
            gc.SetSpeed(shieldSpeed);
            //Cooldown
            gc.SetCooldown(shieldCooldown);
            //Poison
            gc.SetPoison(shieldPoison);
            //Debug.Log(gc.buttonID + " poison value is set to " + poison);
            gc.SetPoisonDur(3f);
            gc.SetChainPoison(chainPoisonBonus);
            gc.SetLeeches(hijackRegenBonus);
            //Slowdown
            gc.SetSlowdown(shieldSlowdownMax); //for now.  eventually add in that scaling system for slow/fast enemies?
            gc.SetSlowDur(0.75f);
            //Knockback
            gc.SetKnockback(shieldKnockback);
            //Lifedrain
            gc.SetLifeDrain(shieldLifeDrain);
            //Splash
            gc.SetSplash(shieldSplash);
            gc.SetSplashRadiusBonus(splashBonusCount * PERCENT_AOE_RANGE_PER_PAIR);
            //Stun
            gc.SetStun(shieldStun);
            //Penetration
            gc.SetPenetration(shieldPenetration);
            gc.SetPiercing(penetrationBonusCount);
            //Shieldshred
            gc.SetShieldShred(shieldShieldShred);
            gc.SetShieldSlow(regenBonusCount * PERCENT_SHIELD_REGEN_SLOW_PER_PAIR);
            //Spread
            gc.SetSpread(shieldSpread);
            //SplitCount
            gc.SetSplit(shieldSplit);
            //Homing
            gc.SetIsHoming(shieldHoming);
            //ArcStrength
            gc.SetDoesArc(shieldArc);
        }
	}
}
