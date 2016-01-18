using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class Shield : MonoBehaviour {
	
	public float maxHP;
	public float hp;
	float regenRate;
	float regenAmt;
	
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
		Debug.Log ("Shield HP updated");
		hpMeter.transform.localScale = new Vector3(hpMeter.transform.localScale.x, hp / maxHP + .1f, hpMeter.transform.localScale.z);
	}
	
	public void PrintHP ()
	{
		Debug.Log ("shield HP: " + hp);
	}
}

