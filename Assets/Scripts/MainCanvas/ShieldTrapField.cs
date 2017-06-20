using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShieldTrapField : DamageField {

    Dial dial;
    PolygonCollider2D polyCollide;
    List<Gun> towers;

    int myLane;
    public float drainPerTick; //health drain per tick
    public float coolPerTick; //cooldown reduction per tick
    float drainMult = 0.01f;
    float cooldownMult = 0.05f;

	// Use this for initialization
	protected override void Start () {
        polyCollide = GetComponent<PolygonCollider2D>();
        rt = GetComponent<RectTransform>();
        time = new Timer();
        filter = new ContactFilter2D();
        filter.NoFilter();
        dial = GameObject.Find("Dial").GetComponent<Dial>();
        grown = true; //for now, doesn't grow
	}
	
	// Update is called once per frame
	protected override void Update () {
        //ticks
        if ((ticksDone * tickLength) > (time.TimeElapsedSecs() - growTime))
        { //if a tick's length has elapsed since the last tick
            polyCollide.OverlapCollider(filter, stuffHit); //fill array with all colliders intersecting field
            fieldHit.Clear();
            for (int i = 0; i < stuffHit.Length; i++)
            { //filter out anything that doesn't get damaged by the field
                Collider2D coll = stuffHit[i];
                if (coll != null && ((coll.gameObject.tag == "Enemy") || (coll.gameObject.tag == "EnemyShield")))
                {
                    fieldHit.Add(coll);
                }
            }
            foreach (Collider2D basecol in fieldHit)
            {  //do the damage
                CircleCollider2D coll = (CircleCollider2D)basecol;
                if (coll == null)
                { //might be a shield whose enemy you destroyed in this loop
                    continue;
                }
                else {
                    ForEachTarget(coll);
                }
            }
            ticksDone++;
        }
        //lifetime
        if ((time.TimeElapsedSecs() - growTime) > maxTime)
        {
            Destroy(gameObject);
        }
        //growth
        /*
        if (!grown)
        {
            float growPercent = time.TimeElapsedSecs() / growTime;
            if (growPercent > 1f)
            {
                growPercent = 1f;
                grown = true;
            }
            float circleSize = growPercent * (aoeSize * canvasUnitsPerAoePoint);
            rt.sizeDelta = new Vector2(circleSize, circleSize);
            //collide.radius = circleSize / 2.05f; //slightly smaller than image;
        }
        */
    }

    public void SetUp()
    {
        towers = new List<Gun>();
        for (int i = 1; i <= 6; i++)
        {
            //Debug.Log("looking for Gun" + i);
            towers.Add(GameObject.Find("Gun" + i).GetComponent<Gun>());
        }
        coolPerTick *= cooldownMult;
        drainPerTick *= drainMult;
    }

    public void ReduceCooldown(float amt)
    {
        Debug.Log("called ReduceCooldown in ShieldTrapField");
        foreach (Gun g in towers)
        {
            if (g.GetCurrentLaneID() - 1 == myLane)
            {
                g.ReduceCooldownInstant(amt);
                return;
            }
        }
    }

    protected override void ForEachTarget(CircleCollider2D coll)
    {
        if (coll.gameObject.tag == "Enemy")
        {
            //Debug.Log("Here's an enemy in ShieldTrapField");
            Enemy e = coll.GetComponent<Enemy>();
            if (e != null)
            {
                //Drain
                if (drainPerTick > 0f)
                {
                    //Debug.Log("doing drain in ShieldTrapField");
                    e.TakeDamage(drainPerTick);
                    dial.ChangeHealth(drainPerTick); //multiplier applied in SetUp()
                }
                if (coolPerTick > 0f)
                {
                    ReduceCooldown(coolPerTick); //multiplier applied in SetUp()
                }
            }
        }
        else if (coll.gameObject.tag == "EnemyShield")
        {
            EnemyShield es = coll.GetComponent<EnemyShield>();
            if (es != null)
            {
                //es.TakeDamage(damagePerTick);
                //Drain
                if (drainPerTick > 0f)
                {

                }
            }
        }
    }

    public void SetMyLane(int i)
    {
        myLane = i;
    }
}
