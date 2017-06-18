/*Duncan*/

using MiniJSON;
using System.IO;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PieceParser{

    //Default values
    static float COOLDOWN_DEFAULT = 0.0f;
    static float ENERGYGAIN_DEFAULT = 0.0f;
    static float COMBOKEY_DEFAULT = 0.0f;

    static float DMG_DEFAULT = 1.0f;
    static int TRAPUSES_DEFAULT = 1;
    static float SHIELDDURABILITY_DEFAULT = 10.0f;
    static float CHARGE_DEFAULT = 0.0f;
    static int SPLIT_DEFAULT = 1;
    static int PENETRATION_DEFAULT = 0;
    static float CONTINUOUS_DEFAULT = 0.0f;
    static float REFLECT_DEFAULT = 0.0f;
    static float FREQUENCY_DEFAULT = 0.0f;
    static float TEMPDISPLACE_DEFAULT = 0.0f;
    static float ABSORB_DEFAULT = 0.0f;
    static float AOE_DEFAULT = 0.0f;
    static float ATTRACTION_DEFAULT = 0.0f;
    static int DUPLICATE_DEFAULT = 0;
    static float FIELD_DEFAULT = 0.0f;

	public static Dictionary<string,float> GetStatsFromGrid(List<string> files){
		Dictionary<string,float> result = new Dictionary<string, float>();
        //Tech Abilities

        //Generic tower
        float cooldown = COOLDOWN_DEFAULT; //weapon cooldown
        float energyGain = ENERGYGAIN_DEFAULT; //each successful use gives additional energy
        float comboKey = COMBOKEY_DEFAULT; //chance for a base weapon type combo to occur (?)

        //Bullet only
        float dmg = DMG_DEFAULT; //Damage dealt per shot

        //Trap only
        int trapUses = TRAPUSES_DEFAULT; //No. of uses a trap has

        //Shield only
        float shieldDurability = SHIELDDURABILITY_DEFAULT; //Health of the shield

        //Bullet and BulletTrap
        float charge = CHARGE_DEFAULT; //Size of on-hit explosion
        int split = SPLIT_DEFAULT; //Bullets, no. of split bullets.  BT, no. of radial bullets on hit

        //Bullet and BulletShield
        int penetration = PENETRATION_DEFAULT; //Bullet, penetration.  BS, shield shred.
        float continuousStrength = CONTINUOUS_DEFAULT; //Bullet, laser firing time.  BS, pulse duration.

        //Shield and BulletShield
        float reflect = REFLECT_DEFAULT; //reflection (?)
        float frequency = FREQUENCY_DEFAULT; //Shield, regen rate.  BS, slow (?)

        //Shield and TrapShield
        float tempDisplace = TEMPDISPLACE_DEFAULT; //Shield, teleport distance.  TS, cooldown.
        float absorb = ABSORB_DEFAULT; //Shield, durability.  TS, lifedrain.

        //Trap and BulletTrap
        float aoe = AOE_DEFAULT; //Trap, AoE size.  PT, range.
        float attraction = ATTRACTION_DEFAULT; //Trap, pull.  PT, homing.

        //Trap and TrapShield
        int duplicate = DUPLICATE_DEFAULT; //Trap, triggers. (?)  TS, zone range.
        float field = FIELD_DEFAULT; //field time (?)

        //count pieces for bonus purposes
        int cooldownCount = 0;
        int energyGainCount = 0;
        int comboKeyCount = 0;
        int dmgCount = 0;
        int trapUsesCount = 0;
        int shieldDurabilityCount = 0;
        int chargeCount = 0;
        int splitCount = 0;
        int penetrationCount = 0;
        int continuousCount = 0;
        int reflectCount = 0;
        int frequencyCount = 0;
        int tempDisplaceCount = 0;
        int absorbCount = 0;
        int aoeCount = 0;
        int attractionCount = 0;
        int duplicateCount = 0;
        int fieldCount = 0;

        foreach (string piecefile in files){
			FileLoader fl = new FileLoader ("JSONData" + Path.DirectorySeparatorChar + "Pieces",piecefile);
			string json = fl.Read ();
			Dictionary<string,System.Object> pdata = (Dictionary<string,System.Object>)Json.Deserialize (json);
			
			//read values

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
            float pcooldown = (float)(double)pdata["cooldown"];
            cooldown += pcooldown;

			if(cooldown < 0.1f)
				cooldown = 0.1f;
            //energyGain - amount of energy given
            float penergyGain = (float)(double)pdata["energyGain"];
            if (penergyGain > 0.0f)
                energyGainCount++;
            energyGain += penergyGain;
            //comboKey - chance of effect happening
            float pcomboKey = (float)(double)pdata["comboKey"];
            if (pcomboKey > 0.0f)
                comboKeyCount++;
            comboKey += pcomboKey;
            //damage - Hp of damage dealt
            float pdamage = (float)(double)pdata["dmg"];
            if (pdamage > 0.0f)
                dmgCount++;
            dmg += pdamage;
            //trapUses - no. of times a trap will detonate
            int ptrapUses = (int)(long)pdata["trapUses"];
            if (ptrapUses > 0)
                trapUsesCount++;
            trapUses += ptrapUses;
            //shieldDurability, in amount of HP the shield has
            float pshieldDurability = (float)(double)pdata["shieldDurability"];
            if (pshieldDurability > 0.0f)
                shieldDurabilityCount++;
            shieldDurability += pshieldDurability;
            //charge, in radius of explosion
            float pcharge = (float)(double)pdata["charge"];
            if (pcharge > 0.0f)
                chargeCount++;
            charge += pcharge;
            //split - 1 is no split, i.e. 1 bullet
            int psplit = (int)(long)pdata["split"];
            split += psplit;
            if (psplit > 0)
                splitCount++;
            //penetration - in number of enemies penetrated
            int ppene = (int)(long)pdata["penetration"];
			penetration += ppene;
			if(ppene > 0)
				penetrationCount++;
            //continuousStrength, in seconds for laser/pulse time
            float pcontinuous = (float)(double)pdata["continuousStrength"];
            if (pcontinuous > 0.0f)
                continuousCount++;
            continuousStrength += pcontinuous;
            //reflect, in (???)
            float preflect = (float)(double)pdata["reflect"];
            if (preflect > 0.0f)
                reflectCount++;
            reflect += preflect;
            //frequency - multiplier for various uses
            float pfrequency = (float)(double)pdata["frequency"];
            if (pfrequency > 0.0f)
                frequencyCount++;
            frequency += pfrequency;
            //tempDisplace - multiplier
            float ptemp = (float)(double)pdata["tempDisplace"];
            if (ptemp > 0.0f)
                tempDisplaceCount++;
            tempDisplace += ptemp;
            if(tempDisplace > 1f){
                tempDisplace = 1f; //cap at 100% of track
            }
            //absorb
            float pabsorb = (float)(double)pdata["absorb"];
            if (pabsorb > 0.0f)
                absorbCount++;
            absorb += pabsorb;
            //AoE
            float paoe = (float)(double)pdata["aoe"];
            if (paoe > 0.0f)
                aoeCount++;
            aoe += paoe;
            //attraction
            float pattraction = (float)(double)pdata["attraction"];
            if (pattraction > 0.0f)
                attractionCount++;
            attraction += pattraction;
            //duplicate
            int pduplicate = (int)(long)pdata["duplicate"];
            if (pduplicate > 0)
                duplicateCount++;
            duplicate += pduplicate;
            //field
            float pfield = (float)(double)pdata["field"];
            if (pfield > 0.0f)
                fieldCount++;
            field += pfield;
		}

        //put the results in the dictionary

        result.Add("cooldown", cooldown);
        result.Add("energyGain", energyGain);
        result.Add("comboKey", comboKey);
        result.Add("dmg",dmg);
		result.Add("trapUses",trapUses);
		result.Add("shieldDurability",shieldDurability);
		result.Add("charge",charge);
		result.Add("split",(float)split);
		result.Add("penetration",penetration);
		result.Add("continuousStrength",continuousStrength);
		result.Add("reflect",reflect);
		result.Add("frequency",frequency);
		result.Add("tempDisplace",tempDisplace);
		result.Add("absorb",absorb);
		result.Add("aoe",aoe);
        result.Add("attraction", attraction);
        result.Add("duplicate", duplicate);
        result.Add("field", field);
        
        //bonuses
		/*
        Not sure if this is relevant as of weapon overhaul?
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
			result.Add("circle",-1f);*/
		
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

        //Tech Abilities

        //Generic tower
        float cooldown = COOLDOWN_DEFAULT; //weapon cooldown
        float energyGain = ENERGYGAIN_DEFAULT; //each successful use gives additional energy
        float comboKey = COMBOKEY_DEFAULT; //chance for a base weapon type combo to occur (?)

        //Bullet only
        float dmg = DMG_DEFAULT; //Damage dealt per shot

        //Trap only
        int trapUses = TRAPUSES_DEFAULT; //No. of uses a trap has

        //Shield only
        float shieldDurability = SHIELDDURABILITY_DEFAULT; //Health of the shield

        //Bullet and BulletTrap
        float charge = CHARGE_DEFAULT; //Size of on-hit explosion
        int split = SPLIT_DEFAULT; //Bullets, no. of split bullets.  BT, no. of radial bullets on hit

        //Bullet and BulletShield
        int penetration = PENETRATION_DEFAULT; //Bullet, penetration.  BS, shield shred.
        float continuousStrength = CONTINUOUS_DEFAULT; //Bullet, laser firing time.  BS, pulse duration.

        //Shield and BulletShield
        float reflect = REFLECT_DEFAULT; //reflection (?)
        float frequency = FREQUENCY_DEFAULT; //Shield, regen rate.  BS, slow (?)

        //Shield and TrapShield
        float tempDisplace = TEMPDISPLACE_DEFAULT; //Shield, teleport distance.  TS, cooldown.
        float absorb = ABSORB_DEFAULT; //Shield, durability.  TS, lifedrain.

        //Trap and BulletTrap
        float aoe = AOE_DEFAULT; //Trap, AoE size.  PT, range.
        float attraction = ATTRACTION_DEFAULT; //Trap, pull.  PT, homing.

        //Trap and TrapShield
        int duplicate = DUPLICATE_DEFAULT; //Trap, triggers. (?)  TS, zone range.
        float field = FIELD_DEFAULT; //field time (?)

        //count pieces for bonus purposes
        int cooldownCount = 0;
        int energyGainCount = 0;
        int comboKeyCount = 0;
        int dmgCount = 0;
        int trapUsesCount = 0;
        int shieldDurabilityCount = 0;
        int chargeCount = 0;
        int splitCount = 0;
        int penetrationCount = 0;
        int continuousCount = 0;
        int reflectCount = 0;
        int frequencyCount = 0;
        int tempDisplaceCount = 0;
        int absorbCount = 0;
        int aoeCount = 0;
        int attractionCount = 0;
        int duplicateCount = 0;
        int fieldCount = 0;

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

        foreach (string pfilename in pieceFilenames){
			FileLoader pieceLoader = new FileLoader("JSONData" + Path.DirectorySeparatorChar + "Pieces", pfilename);
			string pieceJson = pieceLoader.Read();
			Dictionary<string,System.Object> pdata = (Dictionary<string,System.Object>)Json.Deserialize(pieceJson);

            //read values

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
            float pcooldown = (float)(double)pdata["cooldown"];
            cooldown += pcooldown;

            if (cooldown < 0.1f)
                cooldown = 0.1f;
            //energyGain - amount of energy given
            float penergyGain = (float)(double)pdata["energyGain"];
            if (penergyGain > 0.0f)
                energyGainCount++;
            energyGain += penergyGain;
            //comboKey - chance of effect happening
            float pcomboKey = (float)(double)pdata["comboKey"];
            if (pcomboKey > 0.0f)
                comboKeyCount++;
            comboKey += pcomboKey;
            //damage - Hp of damage dealt
            float pdamage = (float)(double)pdata["dmg"];
            if (pdamage > 0.0f)
                dmgCount++;
            dmg += pdamage;
            //trapUses - no. of times a trap will detonate
            int ptrapUses = (int)(long)pdata["trapUses"];
            if (ptrapUses > 0)
                trapUsesCount++;
            trapUses += ptrapUses;
            //shieldDurability, in amount of HP the shield has
            float pshieldDurability = (float)(double)pdata["shieldDurability"];
            if (pshieldDurability > 0.0f)
                shieldDurabilityCount++;
            shieldDurability += pshieldDurability;
            //charge, in radius of explosion
            float pcharge = (float)(double)pdata["charge"];
            if (pcharge > 0.0f)
                chargeCount++;
            charge += pcharge;
            //split - 1 is no split, i.e. 1 bullet
            int psplit = (int)(long)pdata["split"];
            if (psplit > split)
                split = psplit; //use only the highest split piece
            if (psplit > 0)
                splitCount++;
            //penetration - in... percentage of whatever penetration does
            int ppene = (int)(long)pdata["penetration"];
            penetration += ppene;
            if (ppene > 0)
                penetrationCount++;
            //continuousStrength, in seconds for laser/pulse time
            float pcontinuous = (float)(double)pdata["continuousStrength"];
            if (pcontinuous > 0.0f)
                continuousCount++;
            continuousStrength += pcontinuous;
            //reflect, in (???)
            float preflect = (float)(double)pdata["reflect"];
            if (preflect > 0.0f)
                reflectCount++;
            reflect += preflect;
            //frequency - multiplier for various uses
            float pfrequency = (float)(double)pdata["frequency"];
            if (pfrequency > 0.0f)
                frequencyCount++;
            frequency += pfrequency;
            //tempDisplace - multiplier
            float ptemp = (float)(double)pdata["tempDisplace"];
            if (ptemp > 0.0f)
                tempDisplaceCount++;
            tempDisplace += ptemp;
            //absorb
            float pabsorb = (float)(double)pdata["absorb"];
            if (pabsorb > 0.0f)
                absorbCount++;
            absorb += pabsorb;
            //AoE
            float paoe = (float)(double)pdata["aoe"];
            if (paoe > 0.0f)
                aoeCount++;
            aoe += paoe;
            //attraction
            float pattraction = (float)(double)pdata["attraction"];
            if (pattraction > 0.0f)
                attractionCount++;
            attraction += pattraction;
            //duplicate
            int pduplicate = (int)(long)pdata["duplicate"];
            if (pduplicate > 0)
                duplicateCount++;
            duplicate += pduplicate;
            //field
            float pfield = (float)(double)pdata["field"];
            if (pfield > 0.0f)
                fieldCount++;
            field += pfield;
        }

        //Set tower stats based on these values
        gc.SetCooldown(cooldown);
        gc.SetEnergyGain(energyGain);
        gc.SetComboKey(comboKey);
        gc.SetDmg(dmg);
        gc.SetTrapUses(trapUses);
        gc.SetShieldDurability(shieldDurability);
        gc.SetCharge(charge);
        gc.SetSplit(split);
        gc.SetPenetration(penetration);
        gc.SetContinuousStrength(continuousStrength);
        gc.SetReflect(reflect);
        gc.SetFrequency(frequency);
        gc.SetTempDisplace(tempDisplace);
        gc.SetAbsorb(absorb);
        gc.SetAoE(aoe);
        gc.SetAttraction(attraction);
        gc.SetDuplicate(duplicate);
        gc.SetField(field);
	}
}
