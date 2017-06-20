using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ShieldTrap : Shield {

    public float dmgReductionPercent; //reduces damage by this percent - ie if this is 40, enemy's damage is reduce dto 60%
    public float healPerSec; //used for heal-over-time effect with Field stat
    public float drainMultiplier;
    public float cooldownMult = 1f;

    int myLane; //lane ID this shield is on
    ShieldTrapHolder holder;
    float fieldRange = 64f;

	// Use this for initialization
	void Start () {
        shieldDurability = 100f;
        dmgReductionPercent = 100f;
        hp = 100f;
        rt = GetComponent<RectTransform>();
        hpMeter = transform.Find("ShieldHP").gameObject;
        hprt = hpMeter.GetComponent<RectTransform>();
        maxHealthSize = hprt.sizeDelta.y;

        UpdateHPMeter();

        dialObj = GameObject.FindGameObjectWithTag("Dial");
        dial = dialObj.GetComponent<Dial>();
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    public override void OnTriggerEnter2D(Collider2D coll)
    {
        if (coll.gameObject.tag == "Enemy")
        {
            Enemy e = coll.gameObject.GetComponent<Enemy>();

            if (e.GetComponentInChildren<Saboteur>() != null) //if this enemy is a Saboteur
            {
                Debug.Log("shield destroyed by saboteur");
                // TODO ---v
                e.ReduceDamage( -(e.GetDamage()) ); 
                OnDeath(e.GetDamage());
                return;
            }

            //reduce enemy's damage by percent
            e.ReduceDamage(e.GetDamage() * dmgReductionPercent);

            //trigger trap effects
            OnHit(e, e.GetBaseDamage(), hp);

            //destroy entire shield
            holder.DestroyEntireShield();
        }
    }

    //Trap Effects
    public override void OnHit(Enemy e, float unreducedDamage, float unreducedHP)
    {
        //Drain (absorb)
        //heal yourself and damage enemy
        dial.ChangeHealth(absorb * drainMultiplier);
        e.TakeDamage(absorb * drainMultiplier);

        //Cooldown (temporal displacement)
        //any currently cooling down towers in front of shields get their cooldown reduced
        //Debug.Log("tempdisplace: " + tempDisplace);
        //Debug.Log("cooldownmult: " + cooldownMult);
        //Debug.Log("going to reduce cooldown by " + (tempDisplace * cooldownMult));
        holder.ReduceCooldown(tempDisplace * cooldownMult);

        //Field Time (field)
        //spawn an Effect Field with the necessary stats
    }

    //sets a ShieldTrapField in this lane
    public void DropField()
    {
        if (field > 0f)
        {
            GameObject damageField = Instantiate(Resources.Load("Prefabs/MainCanvas/ShieldTrapField")) as GameObject;
            RectTransform dfrt = damageField.GetComponent<RectTransform>();
            dfrt.rotation = rt.rotation;
            float fieldOwnAngle = this.transform.eulerAngles.z;
            float fieldAngle = (fieldOwnAngle + 90f) % 360f;
            fieldAngle *= (float)Math.PI / 180;
            dfrt.anchoredPosition = new Vector2(fieldRange * Mathf.Cos(fieldAngle), fieldRange * Mathf.Sin(fieldAngle));
            damageField.transform.SetParent(Dial.underLayer.transform, false);
            ShieldTrapField sf = damageField.GetComponent<ShieldTrapField>();
            sf.drainPerTick = absorb; //multiplier is in ShieldTrapField.cs
            sf.coolPerTick = tempDisplace; //multiplier is in ShieldTrapField.cs
            sf.maxTime = field;
            sf.SetUp();
            sf.SetMyLane(myLane);
        }
    }

    //sets this shield's lane ID
    public void SetMyLane(int i)
    {
        myLane = i;
    }

    public void SetHolder(ShieldTrapHolder h)
    {
        holder = h;
    }

    public int GetMyLane()
    {
        return myLane;
    }

}
