﻿using UnityEngine;
using System.Collections;

public class Weapon : MonoBehaviour {

    //Tech Abilities

    //Generic tower
    public float cooldown; //weapon cooldown
    public float energyGain; //each successful use gives additional energy
    public float comboKey; //chance for a base weapon type combo to occur (?)

    //Bullet only
    public float dmg; //Damage dealt per shot

    //Trap only
    public int trapUses; //No. of uses a trap has

    //Shield only
    public float shieldDurability; //Health of the shield

    //Bullet and BulletTrap
    public float charge; //Size of on-hit explosion
    public int split; //Bullets, no. of split bullets.  BT, no. of radial bullets on hit

    //Bullet and BulletShield
    public int penetration; //Bullet, penetration.  BS, shield shred.
    public float continuousStrength; //Bullet, laser firing time.  BS, pulse duration.

    //Shield and BulletShield
    public float reflect; //reflection (?)
    public float frequency; //Shield, regen rate.  BS, slow (?)

    //Shield and TrapShield
    public float tempDisplace; //Shield, teleport distance.  TS, cooldown.
    public float absorb; //Shield, durability.  TS, lifedrain.

    //Trap and BulletTrap
    public float aoe; //Trap, AoE size.  PT, range.
    public float attraction; //Trap, pull.  PT, homing.

    //Trap and TrapShield
    public int duplicate; //Trap, triggers. (?)  TS, zone range.
    public float field; //field time (?)

    //Vamp risk
    public float vampDrain; //lifedrain amt, as a percent of damage dealt

    public int zone;

    // Use this for initialization
    void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
