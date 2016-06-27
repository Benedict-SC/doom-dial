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
    public float shieldShred; //TODO percent of enemy shield to steal on hit
    public float knockback; //TODO Reversion -- ask joe what this means??
    public bool stun; //TODO whether stun wave is sent out
    public bool slowdown; //TODO gravity well to center of track
    public float lifeDrain; //TODO converts life taken past its' HP to yours (??? ask joe)
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
	
	float regenBase; //to measure regen time
	
	// Use this for initialization
	void Start () {
		
		//defaults for testing
		maxHP = 30.0f;
		regenRate = 1.0f; //regens once every X seconds
		regenAmt = 1.0f; //amount to regen every regenRate seconds
		
		hp = maxHP;
		rt = GetComponent<RectTransform>();
		hpMeter = transform.FindChild("ShieldHP").gameObject;
		
		UpdateHPMeter();
		
		regenBase = 0.0f;
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
	
	public void PrintHP ()
	{
		Debug.Log ("shield HP: " + hp);
	}
}

