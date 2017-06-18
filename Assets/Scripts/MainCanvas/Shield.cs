using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class Shield : Weapon {

    public float hp; //current hp

    public float maxHealthSize;
    public float minHealthSize = 23f;

	protected GameObject hpMeter;
	protected RectTransform rt;
    protected RectTransform hprt;

    protected GameObject dialObj;
    protected Dial dial;
	
    Timer regenTimer;
	
	// Use this for initialization
	void Start () {
		
		//default
        regenTimer = new Timer();
		
		hp = shieldDurability;
		rt = GetComponent<RectTransform>();
		hpMeter = transform.Find("ShieldHP").gameObject;
		hprt = hpMeter.GetComponent<RectTransform>();
        maxHealthSize = hprt.sizeDelta.y;

		UpdateHPMeter();
	}
	
	// Update is called once per frame
	void Update () {
		if(hp < shieldDurability){//regen
            float moreHealth = regenTimer.TimeElapsedSecs() * frequency;
            hp += moreHealth;
            if(hp > shieldDurability){
                hp = shieldDurability;
            }
            UpdateHPMeter();
            regenTimer.Restart();
        }
	}
    public virtual void OnTriggerEnter2D(Collider2D coll) {
        if (coll.gameObject.tag == "Enemy") {
            Enemy e = coll.gameObject.GetComponent<Enemy>();

            if (e.GetComponentInChildren<Saboteur>() != null) //if this enemy is a Saboteur
            {
                hp = 0;
                Debug.Log("shield destroyed by saboteur");
                e.ReduceDamage(-hp); //increase the saboteur's damage
                OnDeath(e.GetDamage());
                return;
            }

            if(hp >= shieldDurability){
                regenTimer.Restart();
            }
            float initialDamage = e.GetBaseDamage();
            float initialHP = hp;
            e.ReduceDamage(hp); //reduce enemy damage
            hp -= initialDamage; //get hit
            OnHit(e,initialDamage,initialHP);
            float overkill = 0f;
            if(hp <= 0){
                overkill = -hp;
                hp = 0f;
            }
            UpdateHPMeter();
            if(hp == 0f){
                OnDeath(overkill);
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
    public void OnDeath(float overkill)
    {
        Destroy(gameObject);
    }

<<<<<<< HEAD
    public virtual void OnHit(Enemy e){
        
=======
    public void OnHit(Enemy e,float unreducedDamage,float unreducedHP){
        if(reflect > 0f){
            float dealtBack = reflect * unreducedDamage;
            e.TakeDamage(dealtBack);
        }
        if(tempDisplace > 0f){
            RectTransform ert = e.GetComponent<RectTransform>();
            Vector2 dir = (Vector2.zero - ert.anchoredPosition).normalized;
            dir *= ((Dial.TRACK_LENGTH - 15f) * tempDisplace); //15 is because it shouldn't knock back the whole exact track length
            ert.anchoredPosition -= dir;
        }
        if(absorb > 0f){
            Debug.Log("max hp: " + shieldDurability + " -> " + (shieldDurability + absorb));
            shieldDurability += absorb;
            UpdateHPMeter();
        }
>>>>>>> origin/master
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
		Debug.Log ("shield HP: " + hp + "/" + shieldDurability);
	}
}

