using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;
using UnityEngine.UI;

public class Laser : Weapon{
    RectTransform rt;
    BoxCollider2D hitbox;
    float growSpeed = 20f;
    float maxLength = 117f;
    float chargeRate = .025f;
    public float chargePercent = 0f;
    float widthPerChargePoint = 3f;
    float baseWidth;
    enum LaserState{
        GROWING,
        STOPPED,
        SHUTDOWN
    }
    LaserState state;
    Image laserbeam;

    Timer holdTime;

    public int penetrationsLeft;

    public class DistanceComparer : IComparer<Collider2D>{
        public int Compare(Collider2D x, Collider2D y){
            CircleCollider2D xcol = (CircleCollider2D)x;
            CircleCollider2D ycol = (CircleCollider2D)y;
            Vector2 xpos = xcol.gameObject.GetComponent<RectTransform>().anchoredPosition;
            Vector2 ypos = ycol.gameObject.GetComponent<RectTransform>().anchoredPosition;
            float xdist = xpos.magnitude - xcol.radius;
            float ydist = ypos.magnitude - ycol.radius;

            if(xdist > ydist){
                return 1;
            }else if(ydist > xdist){
                return -1;
            }else{
                return 0;
            }
        }
    }

    void Awake(){
        rt = GetComponent<RectTransform>();
        hitbox = GetComponent<BoxCollider2D>();
        rt.sizeDelta = new Vector2(rt.sizeDelta.x,0f);
        baseWidth = rt.sizeDelta.x;
        state = LaserState.GROWING;
        laserbeam = GetComponent<Image>();
        holdTime = new Timer();
    }
    void Update(){
        
        Collider2D[] stuffHit = new Collider2D[20];
        ContactFilter2D filter = new ContactFilter2D();
        filter.NoFilter();
        hitbox.OverlapCollider(filter,stuffHit);
        List<Collider2D> customFiltered = new List<Collider2D>();
        for(int i = 0; i < stuffHit.Length; i++){
            Collider2D coll = stuffHit[i];
            if(coll != null && ((coll.gameObject.tag == "Enemy") || (coll.gameObject.tag == "EnemyShield")) ){
                customFiltered.Add(coll);
            }
        }
        customFiltered.Sort(new DistanceComparer()); //now custom filter is ordered by closest to farthest
        int numToCheck = customFiltered.Count;

        //do some growth checking
        if(state == LaserState.SHUTDOWN){
            //do nothing
        }else if(penetration + 1 <= numToCheck){
            numToCheck = penetration + 1;
            state = LaserState.STOPPED;
        }else{
            state = LaserState.GROWING;
        }
        
        if(state == LaserState.GROWING){
            float newheight = rt.sizeDelta.y + growSpeed;
            if(newheight > maxLength){
                newheight = maxLength;
                state = LaserState.STOPPED;
            }
            rt.sizeDelta = new Vector2(rt.sizeDelta.x,newheight);
        }else if(state == LaserState.SHUTDOWN){
            float newheight = rt.sizeDelta.y - growSpeed;
            if(newheight <= 0 ){
                newheight = 0;
                dmg = 0;
            }
            rt.sizeDelta = new Vector2(rt.sizeDelta.x,newheight);
        }

        for(int i = 0; i < numToCheck; i++){ 
            CircleCollider2D coll = (CircleCollider2D)customFiltered[i];
            if(coll == null){ //might be a shield whose enemy you destroyed in this loop
                continue;
            }else{
                if(coll.gameObject.tag == "Enemy"){
                    Enemy e = coll.GetComponent<Enemy>();
                    if(e != null){
                        e.TakeDamage(dmg/100f);
                    }
                }else if(coll.gameObject.tag == "EnemyShield"){
                    EnemyShield es = coll.GetComponent<EnemyShield>();
                    if(es != null){
                        es.TakeDamage(dmg/100f);
                    }
                }
            }
        }
        if(state == LaserState.STOPPED && customFiltered.Count >= penetration + 1){
            CircleCollider2D coll = (CircleCollider2D)customFiltered[customFiltered.Count - 1];
            Vector2 pos = coll.gameObject.GetComponent<RectTransform>().anchoredPosition;
            float dist = pos.magnitude - coll.radius;
            dist -= Dial.DIAL_RADIUS;
            dist += 3; //so the beam goes far enough in to collide;
            rt.sizeDelta = new Vector2(rt.sizeDelta.x,dist);
        }

        //charge stuff
        if(charge > 0f){
            //each point of aoe width = ...2 max beam width?
            chargePercent += chargeRate/charge;
            if(chargePercent > 1f){
                chargePercent = 1f;
            }
            rt.sizeDelta = new Vector2(baseWidth + (chargePercent*charge*widthPerChargePoint),rt.sizeDelta.y);
            laserbeam.color = new Color(1f-chargePercent,1f,1f);
        }

        if(holdTime.TimeElapsedSecs() > continuousStrength){
            state = LaserState.SHUTDOWN;
        }
        hitbox.offset = new Vector2(0,rt.rect.height/2);
        hitbox.size = new Vector2(rt.rect.width,rt.rect.height);
    }
}