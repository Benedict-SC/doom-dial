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
	
	// Use this for initialization
	void Start () {
        rt = GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(0.0001f,0.0001f);
		collide = GetComponent<CircleCollider2D>();
		time = new Timer();
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
	}
	
	
}
