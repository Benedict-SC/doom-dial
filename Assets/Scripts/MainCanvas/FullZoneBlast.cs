using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FullZoneBlast : MonoBehaviour {

    float damage; //damage dealt to each enemy
    int zoneID; //zone ID of This blast, creator should call SetZoneID()
    Shield parentShield = null; //if a shield created This, assign it

	// Use this for initialization
	void Start () {
        
	}
	
	// Update is called once per frame
	void Update () {
        Blast();
        Destroy(this);
    }

    //apply damage and effects to all enemies in This zone
    void Blast()
    {
        List<Enemy> enemyList = Dial.GetAllEnemiesInZone(zoneID);
        for (int i = 0; i < enemyList.Count; i++)
        {
            enemyList[i].TakeDamage(damage);
            if (parentShield != null && enemyList[i] != null)
            {
                enemyList[i].ShieldInflictedStatus(parentShield);
            }
        }
    }

    //fun visual/sound effects :D
    void Effects()
    {

    }

    public void SetZoneID(int zi)
    {
        zoneID = zi;
    }

    public void SetParentShield(Shield sc)
    {
        parentShield = sc;
    }

    public void SetDamage(float dmg)
    {
        damage = dmg;
    }
}
