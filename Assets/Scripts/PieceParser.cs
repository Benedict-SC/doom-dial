using MiniJSON;
using System.IO;
using System;
using System.Collections.Generic;

public class PieceParser{

	public static void FillController(GunController gc, string filename){
		FileLoader fl = new FileLoader ("JSONData" + Path.DirectorySeparatorChar + "Towers",filename);
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
		float damage = 0.0f;
		float speed = 1.0f;
		float range = 0.5f;
		float cooldown = 1.0f;
		float knockback = 0.0f;
		float lifeDrain = 0.0f;
		float poison = 0.0f;
		float splashEffectPercent = 0.0f;
		float stun = 0.0f;
		float slowdownMin = 0.0f;
		float slowdownMax = 0.0f;
		float penetration = 0.0f;
		float shieldShred = 0.0f;
		int spread = 1;
		int splitType = 0;
		float homingStrength = 0.0f;
		float arcBoost = 0.0f;
		
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
			//poison - percent of health to remove over the course of 3 seconds
			float ppoison = (float)(double)pdata["poison"];
			if(ppoison > 0.0f)
				poisonCount++;
			//slow - in percent of enemy speed to subtract
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
			
			}else if(gc.GetTowerType().Equals("Trap")){
			
			}else if(gc.GetTowerType().Equals ("Shield")){
			
			}
			
		} 
		
	}

}
