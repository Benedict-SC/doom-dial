/*Thom*/

using UnityEngine;
using System.Collections;

public class AoE : MonoBehaviour {
	
	public float scale;
    float canvasUnitsPerAoePoint = 10f;
	public string parent;
	public float aoeDamage;
	//public Trap aoeTrapCon;
	
	CircleCollider2D collide;
    RectTransform rt;
	
    public float maxTime;
    Timer time;

    public float growTime = 0.7f;

    public bool canDamageDial = false;

    public float vampDrain = 10f;
    public bool vampIsOn;

    bool hasHitDial = false;
    Collider2D[] collsHit = new Collider2D[20];
    ContactFilter2D filter;
	
	// Use this for initialization
	void Start () {
        rt = GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(0.0001f,0.0001f);
		collide = GetComponent<CircleCollider2D>();
		time = new Timer();

        if (PlayerPrefsInfo.Int2Bool(PlayerPrefs.GetInt(PlayerPrefsInfo.s_vampire)))
        {
            vampIsOn = true;
        }
        Debug.Log("created AoE, canDamageDial = " + canDamageDial);

        filter = new ContactFilter2D();
        filter.NoFilter();
    }
	
	// Update is called once per frame
	void Update () {
			bool grown = false;
            float growPercent = time.TimeElapsedSecs()/growTime;
                if(growPercent > 1f){
                    growPercent = 1f;
                    grown = true;
                }
            float circleSize = growPercent * (scale * canvasUnitsPerAoePoint);
            rt.sizeDelta = new Vector2(circleSize,circleSize);
            collide.radius = circleSize/2.05f; //slightly smaller than image;
			if(grown){
				Destroy(gameObject);
			}
        
        if (canDamageDial && !hasHitDial)
        {
            Debug.Log("trying to find dial overlap");
            collide.OverlapCollider(filter, collsHit);
            for (int i = 0; i < collsHit.Length; i++)
            { //filter out anything that doesn't get damaged by the field
                Collider2D coll = collsHit[i];
                if (coll != null && coll.gameObject.tag == "Dial")
                {
                    DamageDial(coll.gameObject.GetComponent<Dial>());
                    Debug.Log("found dial overlap!");
                }
            }
        }
        
    }

    public void OnTriggerEnter2D(Collider2D coll)
    {
        Debug.Log("asdf");
        if (coll.gameObject.tag == "Dial")
        {
            Debug.Log("aoe detected dial trigger");
        }
    }

    void DamageDial(Dial d)
    {
        if (d.health < aoeDamage)
        {
            d.health = 0f;
        }
        else
        {
            d.health -= aoeDamage;
        }
        hasHitDial = true;
    }
}
