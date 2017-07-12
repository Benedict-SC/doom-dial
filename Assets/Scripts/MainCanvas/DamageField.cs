using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DamageField : MonoBehaviour {

	public float aoeSize;
    protected float canvasUnitsPerAoePoint = 10f;

    public float damagePerTick;
    public float tickLength = 0.5f;
    public float maxTime;
    protected Timer time;
    protected int ticksDone = 0;

    public float growTime = 0.7f;
    protected bool grown = false;
	
	protected CircleCollider2D collide;
    protected Collider2D[] stuffHit = new Collider2D[30];
    protected List<Collider2D> fieldHit = new List<Collider2D>();
    protected ContactFilter2D filter;
    protected RectTransform rt;

    Dial dial;

    //vampdrain risk
    bool vampIsOn = false;
	
	// Use this for initialization
	protected virtual void Start () {
		collide = GetComponent<CircleCollider2D>();
        rt = GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(0.0001f,0.0001f);
        time = new Timer();
        filter = new ContactFilter2D();
        filter.NoFilter();
        dial = GameObject.Find("Dial").GetComponent<Dial>();

        if (PlayerPrefsInfo.Int2Bool(PlayerPrefs.GetInt(PlayerPrefsInfo.s_vampire)))
        {
            vampIsOn = true;
        }
	}
	
	// Update is called once per frame
	protected virtual void Update () {
            //ticks
            if( (ticksDone*tickLength) > (time.TimeElapsedSecs() - growTime) ){ //if a tick's length has elapsed since the last tick
                collide.OverlapCollider(filter,stuffHit); //fill array with all colliders intersecting field
                fieldHit.Clear();
                for(int i = 0; i < stuffHit.Length; i++){ //filter out anything that doesn't get damaged by the field
                    Collider2D coll = stuffHit[i];
                    if(coll != null && ((coll.gameObject.tag == "Enemy") || (coll.gameObject.tag == "EnemyShield")) ){
                        fieldHit.Add(coll);
                    }
                }
                foreach(Collider2D basecol in fieldHit){  //do the damage
                    CircleCollider2D coll = (CircleCollider2D)basecol;
                    if(coll == null){ //might be a shield whose enemy you destroyed in this loop
                        continue;
                    }else{
                        ForEachTarget(coll);
                    }
                }
                ticksDone++;
            }
            //lifetime
            if( (time.TimeElapsedSecs() - growTime) > maxTime ){
                Destroy(gameObject);
            }
            //growth
            if(!grown){
                float growPercent = time.TimeElapsedSecs()/growTime;
                if(growPercent > 1f){
                    growPercent = 1f;
                    grown = true;
                }
                float circleSize = growPercent * (aoeSize * canvasUnitsPerAoePoint);
                rt.sizeDelta = new Vector2(circleSize,circleSize);
                collide.radius = circleSize/2.05f; //slightly smaller than image;
            }
        
	}	

    //called once each tick
    protected virtual void ForEachTarget(CircleCollider2D coll){
        if(coll.gameObject.tag == "Enemy"){
            Enemy e = coll.GetComponent<Enemy>();
            if(e != null){
                if (vampIsOn)
                {
                    if (e.GetHP() < damagePerTick)
                    {
                        dial.ChangeHealth(e.GetHP());
                    }
                    else
                    {
                        dial.ChangeHealth(damagePerTick);
                    }
                }
                e.TakeDamage(damagePerTick);
            }
        }else if(coll.gameObject.tag == "EnemyShield"){
            EnemyShield es = coll.GetComponent<EnemyShield>();
            if(es != null){
                es.TakeDamage(damagePerTick);
            }
        }
    }
}
