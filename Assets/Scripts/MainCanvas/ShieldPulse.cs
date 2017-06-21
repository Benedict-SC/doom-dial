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
	
	void Awake () {
		parentMask = transform.parent.gameObject;
		rt = GetComponent<RectTransform>();
        ccoll = GetComponent<CircleCollider2D>();
        ccoll.radius = 0;
        rt.sizeDelta = new Vector2(0.001f,0.001f);
	}
    public void ConfigurePulse(int lane){
        maxAngle = (( 90f + ((lane-1) * 60f) ) + 360f) % 360f; //degrees counterclockwise from x axis of counterclockwisemost edge of zone
        parentMask.transform.eulerAngles = new Vector3(0,0,(lane-1)*-60);
        Debug.Log("euler angles z: " + parentMask.transform.eulerAngles.z);
    }
	
	void Update () {
		rt.sizeDelta += growthRate;
        ccoll.radius = rt.sizeDelta.x / 2f;
        if(rt.sizeDelta.x > 1000f){ //when it's real big
            Destroy(parentMask);
        }
	}
    public virtual void OnTriggerEnter2D(Collider2D coll) {
        if (coll.gameObject.tag == "Enemy") {
            Enemy e = coll.gameObject.GetComponent<Enemy>();
            RectTransform ert = e.GetComponent<RectTransform>();
            float enemyAngle = Mathf.Atan2(ert.anchoredPosition.y,ert.anchoredPosition.x) * Mathf.Rad2Deg;
            if(enemyAngle < 0f){
                enemyAngle += 360f;
            }
            if(maxAngle <= 30f && enemyAngle >= 330f){ //oh my god fuck this wraparound shit, let's just handle our known special case. i hate math
                enemyAngle -= 360f;
            }
            if(enemyAngle <= maxAngle && enemyAngle >= (maxAngle - 60f)){
                OnHit(e);
            }
        }
    }
        
    public virtual void OnHit(Enemy e){
       e.TakeDamage(dmg);
    }
}

