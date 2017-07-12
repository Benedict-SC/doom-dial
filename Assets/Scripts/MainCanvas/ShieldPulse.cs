using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class ShieldPulse : Weapon {

	protected RectTransform rt;
    public GameObject parentMask;
    Vector2 growthRate = new Vector2(8f,8f);
    int lane = 0;
    float maxAngle;
    float minAngle;
    CircleCollider2D ccoll;

    Timer delay;
    float delaySecs;
    public bool vampIsOn = false;
    public Dial dial;
	
	void Awake () {
		parentMask = transform.parent.gameObject;
		rt = GetComponent<RectTransform>();
        ccoll = GetComponent<CircleCollider2D>();
        ccoll.radius = 0;
        rt.sizeDelta = new Vector2(0.001f,0.001f);
        delay = new Timer();
	}
    public void ConfigurePulse(int givenLane,float secs){
        lane = givenLane;
        parentMask.transform.eulerAngles = new Vector3(0,0,(lane-1)*-60);
        delaySecs = secs;
    }
	
	void Update () {
        if(delay.TimeElapsedSecs() > delaySecs){
            rt.sizeDelta += growthRate;
            ccoll.radius = rt.sizeDelta.x / 2f;
            if(rt.sizeDelta.x > 1000f){ //when it's real big
                Destroy(parentMask);
            }
        }
	}
    public virtual void OnTriggerEnter2D(Collider2D coll) {
        if (coll.gameObject.tag == "Enemy") {
            Enemy e = coll.gameObject.GetComponent<Enemy>();
            int eLane = Dial.LaneFromPosition(coll.gameObject);
            if(eLane == lane){
                OnHit(e);
            }
        }
    }
        
    public virtual void OnHit(Enemy e){
       if (vampIsOn)
        {
            if (e.GetHP() < vampDrain)
            {
                dial.ChangeHealth(e.GetHP());
            }
            else
            {
                dial.ChangeHealth(vampDrain);
            }
        }

       e.TakeDamage(dmg);
       if(frequency > 0){
           float mult = 1.0f - (0.15f * frequency);
           e.Slow(mult,frequency);
       }
       if(penetration > 0){
           float shredAmount = penetration * 5f;
           e.ShredShield(shredAmount);
           Debug.Log("we're shreddin': " + shredAmount);
       }
    }
}

