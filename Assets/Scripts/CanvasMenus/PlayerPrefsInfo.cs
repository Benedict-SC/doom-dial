using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerPrefsInfo : MonoBehaviour {

    //vars accessed during gameplay
    static float totalDropRateBoost;
    static float totalRarityRateBoost;
    static float totalOmnitechDropRateBoost;

    //boost amounts -- 15% for now
    public static float dropBoostPerRisk = .15f;
    public static float rarityBoostPerRisk = .15f;
    public static float omnitechBoostPerRisk = .15f;

    //property names
    public static string s_skinnyFarLane = "skinnyFarLane"; //drop up
    public static string s_skinnyMidLane = "skinnyMidLane"; //drop up
    public static string s_skinnyNearLane = "skinnyNearLane"; //drop up
    public static string s_inverseDialSpin = "reverseDialSpin"; //drop up
    public static string s_useLock = "useLock"; //rarity rate up
    public static string s_rotLock = "rotLock"; //rarity rate up
    public static string s_vampire = "vampire"; //rarity rate up
    public static string s_ambush = "ambush"; //drop up
    public static string s_sabotage = "sabotage"; //drop up
    public static string s_tougherEnemies = "tougherEnemies"; //omnitech drop up

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public static void CalculateDropRateBoost()
    {
        totalDropRateBoost = 0f;
        if (Int2Bool(PlayerPrefs.GetInt(s_skinnyFarLane)))
        {
            totalDropRateBoost += dropBoostPerRisk;
        }
        if (Int2Bool(PlayerPrefs.GetInt(s_skinnyMidLane)))
        {
            totalDropRateBoost += dropBoostPerRisk;
        }
        if (Int2Bool(PlayerPrefs.GetInt(s_skinnyNearLane)))
        {
            totalDropRateBoost += dropBoostPerRisk;
        }
        if (Int2Bool(PlayerPrefs.GetInt(s_inverseDialSpin)))
        {
            totalDropRateBoost += dropBoostPerRisk;
        }
        if (Int2Bool(PlayerPrefs.GetInt(s_ambush)))
        {
            totalDropRateBoost += dropBoostPerRisk;
        }
        if (Int2Bool(PlayerPrefs.GetInt(s_sabotage)))
        {
            totalDropRateBoost += dropBoostPerRisk;
        }
        if (totalDropRateBoost > 1f) totalDropRateBoost = 1f;
        if (totalDropRateBoost < 0f) totalDropRateBoost = 0f;
        //PrintAllRateBoosts();
    }

    public static void CalculateRarityRateBoost()
    {
        totalRarityRateBoost = 0f;
        if (Int2Bool(PlayerPrefs.GetInt(s_useLock)))
        {
            totalRarityRateBoost += rarityBoostPerRisk;
        }
        if (Int2Bool(PlayerPrefs.GetInt(s_rotLock)))
        {
            totalRarityRateBoost += rarityBoostPerRisk;
        }
        if (Int2Bool(PlayerPrefs.GetInt(s_vampire)))
        {
            totalRarityRateBoost += rarityBoostPerRisk;
        }
        if (totalRarityRateBoost > 1f) totalRarityRateBoost = 1f;
        if (totalRarityRateBoost < 0f) totalRarityRateBoost = 0f;
        //PrintAllRateBoosts();
    }

    public static void CalculateOmnitechRateBoost()
    {
        totalOmnitechDropRateBoost = 0f;
        if (Int2Bool(PlayerPrefs.GetInt(s_tougherEnemies)))
        {
            totalOmnitechDropRateBoost += omnitechBoostPerRisk;
        }
        if (totalOmnitechDropRateBoost > 1f) totalOmnitechDropRateBoost = 1f;
        if (totalOmnitechDropRateBoost < 0f) totalOmnitechDropRateBoost = 0f;
        //PrintAllRateBoosts();
    }

    public static void PrintAllRateBoosts()
    {
        Debug.Log("droprate: " + totalDropRateBoost + ", rarityrate: " + totalRarityRateBoost + ", omnitechrate: " + totalOmnitechDropRateBoost);
    }

    public static float GetDropRateBoost()
    {
        return totalDropRateBoost;
    }

    public static float GetRarityRateBoost()
    {
        return totalRarityRateBoost;
    }

    public static float GetOmnitechDropRateBoost()
    {
        return totalOmnitechDropRateBoost;
    }

    public static bool Int2Bool(int i)
    {
        if (i == 0) return false;
        else return true;
    }

    public static int Bool2Int(bool b)
    {
        if (b) return 1;
        else return 0;
    }
}
