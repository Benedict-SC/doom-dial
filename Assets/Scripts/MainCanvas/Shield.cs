using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class Shield : MonoBehaviour {
	
	public float maxHP;
	public float hp;
	public float regenRate;
	public float regenAmt;
    public float speedBoost; //TODO speed boost given to bullets shot through This shield
    public float rangeBoost; //TODO speed boost given to bullets shot through This shield
    public float penProtect; //TODO percent protection from enemy bullets' Penetration values
    public float shieldShred; //percent of enemy shield to steal on hit, expressed as a decimal
    public float knockback; //TODO Reversion -- ask joe what this means??
    public bool stun; //TODO whether stun wave is sent out
    public bool slowdown; //TODO gravity well to center of track
    public bool lifeDrain; //converts life taken past its' HP to yours (??? ask joe)
    public float poison; //TODO puff out poison shotgun bursts - amt of damage per second
    public float poisonDur; //TODO length of poison effect in second
    public float spread; //TODO bullet shot through gets arc lightning to X enemies at Y distance
    public float spreadRadius; //TODO Y distance of above
    public float split; //TODO mini shields on the side
    public bool homing; //TODO shields the lane w/ most enemies
    public bool arc; //TODO shield opposite side
    public bool splash; //TODO explodes when spent
    public float splashDmg; //TODO dmg for splash

    public float spawnx;
	public float spawny;
	
	GameObject hpMeter;
	RectTransform rt;

    GameObject dialObj;
    Dial dial;

    int currentLaneID;
	
	float regenBase; //to measure regen time
	
	// Use this for initialization
	void Start () {
		
		//defaults for testing
		regenRate = 1.0f; //regens once every X seconds
		regenAmt = 1.0f; //amount to regen every regenRate seconds
		
		hp = maxHP;
		rt = GetComponent<RectTransform>();
		hpMeter = transform.FindChild("ShieldHP").gameObject;
		
		UpdateHPMeter();
		
		regenBase = 0.0f;

        dialObj = GameObject.FindGameObjectWithTag("Dial");
        dial = dialObj.GetComponent<Dial>();

        currentLaneID = GetCurrentLaneID();
	}
	
	// Update is called once per frame
	void Update () {
		
		if (Time.time - regenBase >= regenRate) //regen stuff here
		{
			hp += regenAmt;
			if (hp > maxHP)
				hp = maxHP;
			regenBase = Time.time;
			UpdateHPMeter ();
		}
	}
	
	public void UpdateHPMeter ()
	{
		//Debug.Log ("Shield HP updated");
		hpMeter.transform.localScale = new Vector3(hpMeter.transform.localScale.x, hp / maxHP + .1f, hpMeter.transform.localScale.z);
	}

    //handles on-death effects - Blast, Life Drain, and Stun Wave
    //dialDmg is any extra damage given to dial that Shield couldn't block
    public void OnDestroyEffects(float dialDmg)
    {
        Debug.Log("onDestroyEffects called");
        //lifedrain effect
        if (lifeDrain)
        {
            dial.ChangeHealth(dialDmg);
        }
        //stun wave
        if (stun)
        {
            //remove all effects except Stun to be applied to the Wave
            shieldShred = 0;
            knockback = 0;
            slowdown = false;
            lifeDrain = false;
            poison = 0;

            //spawn a wave
            Debug.Log("shield stun wave started");
            GameObject zoneWave = Instantiate(Resources.Load("Prefabs/MainCanvas/FullZoneWave")) as GameObject;
            FullZoneWave fzw = zoneWave.GetComponent<FullZoneWave>();
            fzw.SetParentShield(this);
            fzw.ConfigureHolderFromParent(this);
        }
        //blast wave
        if (splash)
        {
            Debug.Log("shield splash dmg started");
            GameObject zoneBlast = Instantiate(Resources.Load("Prefabs/MainCanvas/FullZoneBlast")) as GameObject;
            currentLaneID = GetCurrentLaneID();
            FullZoneBlast fzb = zoneBlast.GetComponent<FullZoneBlast>();
            fzb.SetZoneID(currentLaneID);
            fzb.SetParentShield(this);
            fzb.SetDamage(splashDmg);
        }
    }

    int GetCurrentLaneID()
    {
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
        else {
            Debug.Log("somehow a gun has a very very wrong angle");
            return -1;
        }
    }

    public void PrintHP ()
	{
		Debug.Log ("shield HP: " + hp);
	}

    public void PrintSpeedBoost()
    {
        Debug.Log("shield speedboost = " + speedBoost);
    }
}

