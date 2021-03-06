using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class OldShield : Weapon {

    public float spawnx;
	public float spawny;
    public float hp; //current hp

	GameObject hpMeter;
	RectTransform rt;

    GameObject dialObj;
    Dial dial;

    int currentLaneID;
	
	float regenBase; //to measure regen time
    float regenAmt;
	
	// Use this for initialization
	void Start () {
		
		//default
		regenAmt = 1.0f; //amount to regen every regenRate seconds
		
		hp = shieldDurability;
		rt = GetComponent<RectTransform>();
		hpMeter = transform.Find("ShieldHP").gameObject;
		
		UpdateHPMeter();
		
		regenBase = 0.0f;

        dialObj = GameObject.FindGameObjectWithTag("Dial");
        dial = dialObj.GetComponent<Dial>();

        currentLaneID = GetCurrentLaneID();
	}
	
	// Update is called once per frame
	void Update () {
		
		if (Time.time - regenBase >= frequency) //regen stuff here
		{
			hp += regenAmt;
			if (hp > shieldDurability)
				hp = shieldDurability;
			regenBase = Time.time;
			UpdateHPMeter ();
		}
	}
	
	public void UpdateHPMeter ()
	{
		//Debug.Log ("Shield HP updated");
		hpMeter.transform.localScale = new Vector3(hpMeter.transform.localScale.x, hp / shieldDurability + .1f, hpMeter.transform.localScale.z);
	}

    //handles on-death effects - Blast, Life Drain, and Stun Wave
    //dialDmg is any extra damage given to dial that Shield couldn't block
    public void OnDestroyEffects(float dialDmg)
    {
        /*
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
        */
    }

    public int GetCurrentLaneID(){
		float angle = transform.eulerAngles.z;
		angle = (360f-angle)%360f;
		if (angle > 28.0 && angle < 32.0)
			return 1;
		else if (angle > 88.0 && angle < 92.0)
			return 2;
		else if (angle > 148.0 && angle < 152.0)
			return 3;
		else if (angle > 208.0 && angle < 212.0)
			return 4;
		else if (angle > 268.0 && angle < 272.0)
			return 5;
		else if (angle > 328.0 && angle < 332.0)
			return 6;
		else{
			Debug.Log ("somehow a gun has a very very wrong angle");
			return -1;
		}
	}

    public void PrintHP ()
	{
		Debug.Log ("shield HP: " + hp);
	}

    /*public void PrintSpeedBoost()
    {
        Debug.Log("shield speedboost = " + speedBoost);
    }*/
}

