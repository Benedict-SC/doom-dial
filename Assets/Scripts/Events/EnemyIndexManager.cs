using UnityEngine;
using System.Collections;

public class EnemyIndexManager : MonoBehaviour {

    //After being SEEN once, add its image to the bestiary
    static void IncrementTimesSeen(string enemyFile)
    {
        //implement
    }
    
    static void IncrementTimesHit(string enemyFile)
    {
        //implement later, and do this for other functions here too
    }

    //First time you kill an enemy, reveal its HP and base shield stat in bestiary
    //After 10 kills you get its description/ability/other stats
    //50 kills: get emblem for towers
    //100 kills: ability to use it in level editor
    static void IncrementTimesKilled(string enemyFile)
    {
        //implementation
    }

    //After getting hit once by an enemy, reveal its damage stat in bestiary
    static void IncrementTimesHitBy(string enemyFile)
    {
        //implement
    }

    static void IncrementTimesKilledBy(string enemyFile)
    {
        //imp
    }

    static void IncrementTotalKills()
    {
        //implementation
    }

    static void IncrementTotalDeaths()
    {
        //imp
    }

    static void IncrementTotalHits()
    {
        //asdf
    }

    static void IncrementTotalHitBy()
    {
        //asdfasdf
    }
}
