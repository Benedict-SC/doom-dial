using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class BulletShield : Weapon {

    public float hp; //current hp

    public float maxHealthSize;
    public float minHealthSize = 23f;

	protected GameObject hpMeter;
	protected RectTransform rt;
    protected RectTransform hprt;

    Dial dial;
	
	void Start () {
		hp = 3;
        shieldDurability = 3;
        dmg = 6f; //base damage
		rt = GetComponent<RectTransform>();
		hpMeter = transform.Find("ShieldHP").gameObject;
		hprt = hpMeter.GetComponent<RectTransform>();
        maxHealthSize = hprt.sizeDelta.y;

		UpdateHPMeter();

        dial = GameObject.Find("Dial").GetComponent<Dial>();
	}
	
	void Update () {
        //no regen on these bad boys
	}
    public virtual void OnTriggerEnter2D(Collider2D coll) {
        if (coll.gameObject.tag == "Enemy") {
            Enemy e = coll.gameObject.GetComponent<Enemy>();

            if (e.GetComponentInChildren<Saboteur>() != null) //if this enemy is a Saboteur
            {
                Debug.Log("shield destroyed by saboteur");
                e.ReduceDamage(-e.GetBaseDamage()); //increase the saboteur's damage
                hp = 0;
                OnDeath();
                return;
            }

            hp -= 1; //get hit
            OnHit(e,e.GetBaseDamage());
            float overkill = 0f;
            if(hp <= 0){
                overkill = -hp;
                hp = 0f;
            }
            UpdateHPMeter();
            if(hp == 0f){
                OnDeath();
            }
        }
    }
	
	public void UpdateHPMeter ()
	{
		float percentHP = hp/shieldDurability;
        float differential = maxHealthSize - minHealthSize;
        hprt.sizeDelta = new Vector2(hprt.sizeDelta.x,minHealthSize + (percentHP*differential));
	}

    //dialDmg is any extra damage given to dial that Shield couldn't block
    public void OnDeath()
    {
        Destroy(gameObject);
    }
        
    public virtual void OnHit(Enemy e,float unreducedDamage){
        float pulseDamage = dmg;
        if(reflect > 0f){
            pulseDamage += reflect * unreducedDamage;
        }
        int pulseTimes = 1;
        if(continuousStrength > 0){
            pulseTimes += (int)(continuousStrength-1);
        }
        for(int i = 0; i < pulseTimes; i++){
            GameObject pulse = Instantiate(Resources.Load ("Prefabs/MainCanvas/ShieldPulseMask")) as GameObject;
            pulse.transform.SetParent(Dial.underLayer,false);
            ShieldPulse sp = pulse.transform.Find("ShieldPulse").GetComponent<ShieldPulse>();
            sp.dial = dial;
            sp.dmg = pulseDamage;
            if (vampDrain > 0f)
            {
                sp.vampDrain = vampDrain;
                sp.vampIsOn = true;
            }
            sp.frequency = frequency;
            sp.penetration = penetration;
            sp.ConfigurePulse(GetCurrentLaneID(),i*0.3f);
        }
        if (vampDrain > 0f)
        {
            //Vampire drain
            //Debug.Log("vampdrain is on on shield");
            float amt = e.GetHP();
            dial.ChangeHealth(vampDrain * amt * .4f);
            e.TakeDamage(vampDrain * amt * .8f);
            //Debug.Log("new dial health is " + dial.health);
        }
    }

    public int GetCurrentLaneID(){ 
		float degrees = ((360-Mathf.Atan2(rt.anchoredPosition.y,rt.anchoredPosition.x) * Mathf.Rad2Deg)+90 + 360)%360;
		//Debug.Log(degrees);
		if(degrees >= 60.0 && degrees < 120.0){
			return 2;
		}else if(degrees >= 120.0 && degrees < 180.0){
			return 3;
		}else if(degrees >= 180.0 && degrees < 240.0){
			return 4;
		}else if(degrees >= 240.0 && degrees < 300.0){
			return 5;
		}else if(degrees >= 300.0 && degrees < 360.0){
			return 6;
		}else if(degrees >= 360.0 || degrees < 60.0){
			return 1;
		}else{
			//what the heck, this shouldn't happen
			Debug.Log ("What the heck, this shouldn't happen");
			return 0;
		}
	}

    public void PrintHP ()
	{
		Debug.Log ("bullet shield HP: " + hp + "/" + shieldDurability);
	}
}

