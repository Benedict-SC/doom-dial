using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using MiniJSON;
using System.Collections.Generic;
using System.IO;

public class EnemyIndexManager : MonoBehaviour {

    //After being SEEN once, add its image to the bestiary
    //First time you kill an enemy, reveal its HP and base shield stat in bestiary
    //After 10 kills you get its description/ability/other stats
    //50 kills: get emblem for towers
    //100 kills: ability to use it in level editor
    //After getting hit once by an enemy, reveal its damage stat in bestiary
    //accepts the following stats: timesHit, timesKilled, timesHitBy, timesKilledBy, timesSeen
    static WaveManager2 interestedWaveManager;
    void Start() {
        interestedWaveManager = GetComponent<WaveManager2>();
    }
    public static void IncrementEnemyLogStat(string enemyFile, string statName)
    {
        FileLoader fl = FileLoader.GetSaveDataLoader("Bestiary", "bestiary_logging");
        string json = fl.Read();
        Dictionary<string, System.Object> enDict = (Dictionary<string, System.Object>)Json.Deserialize(json);
        List<System.Object> enemies = enDict["enemyLogs"] as List<System.Object>;
        foreach (System.Object enemy in enemies)
        {
            Dictionary<string, System.Object> edata = enemy as Dictionary<string, System.Object>;
            string filename = edata["name"] as string;
            if (filename.Equals(enemyFile)) //find the correct enemy corresponding to enemyFile
            {
                //increment the indicated stat
                long stat = (long)edata[statName];
                stat += 1;
                edata[statName] = stat;

            }
        }
		string newfiledata = Json.Serialize(enDict);
        fl.Write(newfiledata);
    }

    //accepts the following stats: totalKills, totalDeaths, totalHits, totalHitBy
    public static void IncrementTotalsLogStat(string statName)
    {
        FileLoader fl = FileLoader.GetSaveDataLoader("Bestiary", "bestiary_logging");
        string json = fl.Read();
        Dictionary<string, System.Object> enDict = (Dictionary<string, System.Object>)Json.Deserialize(json);
        //increment the indicated stat
        long stat = (long)enDict[statName];
        stat += 1;
        enDict[statName] = stat;
        string newfiledata = Json.Serialize(enDict);
        fl.Write(newfiledata);
    }
    public static void LogHitEnemy(string enemyfile){
    	EnemyIndexManager.IncrementEnemyLogStat(enemyfile,"timesHit");
    	EnemyIndexManager.IncrementTotalsLogStat("totalHits");
    }
    public static void LogHitByEnemy(string enemyfile){
		EnemyIndexManager.IncrementEnemyLogStat(enemyfile,"timesHitBy");
    	EnemyIndexManager.IncrementTotalsLogStat("totalHitBy");
    }
    public static void LogPlayerDeath(string enemyfile){
		EnemyIndexManager.IncrementEnemyLogStat(enemyfile,"timesKilledBy");
    	EnemyIndexManager.IncrementTotalsLogStat("totalDeaths");
    }
    public static void LogEnemyDeath(string enemyfile){
        interestedWaveManager.guysKilled++;
		EnemyIndexManager.IncrementEnemyLogStat(enemyfile,"timesKilled");
    	EnemyIndexManager.IncrementTotalsLogStat("totalKills");
    }
    public static void LogEnemyAppearance(string enemyfile){
    	EnemyIndexManager.IncrementEnemyLogStat(enemyfile,"timesSeen");
    	Debug.Log(enemyfile);
    }
}
