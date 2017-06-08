using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DamageField : MonoBehaviour {

	public float aoeSize;
    float canvasUnitsPerAoePoint = 10f;

    public float damagePerTick;
    public float tickLength = 0.5f;
    public float maxTime;
    Timer time;
    int ticksDone = 0;

    public float growTime = 0.7f;
    bool grown = false;
	
	CircleCollider2D collide;
    RectTransform rt;
	
	// Use this for initialization
	void Start () {
		collide = GetComponent<CircleCollider2D>();
        rt = GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(0.0001f,0.0001f);
        time = new Timer();
	}
	
	// Update is called once per frame
	void Update () {
            //ticks
            if( (ticksDone*tickLength) > (time.TimeElapsedSecs() - growTime) ){ //if a tick's length has elapsed since the last tick
                Collider2D[] stuffHit = new Collider2D[30];
                ContactFilter2D filter = new ContactFilter2D();
                filter.NoFilter();
                collide.OverlapCollider(filter,stuffHit); //fill array with all colliders intersecting field
                List<Collider2D> fieldHit = new List<Collider2D>();
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
                        if(coll.gameObject.tag == "Enemy"){
                            Enemy e = coll.GetComponent<Enemy>();
                            if(e != null){
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
}
