using UnityEngine;
using System.Collections;
using System;
using UnityEngine.UI;

public class Bullet : Weapon {

    float TRACK_LENGTH = 110.8f + 5; //hard coded to avoid querying track size all the time
                                     // ^^^ RELATIVE TO WHERE BULLET STARTS, NOT CENTER
    public float speed;
    public float spawnx;
    public float spawny;
    public float vx;
    public float vy;
    float spawnDistFromCenter = 0f;
    RectTransform rt;
    bool isActive = true;
    float distance; //used in Update
    public int splitCode = 0; //used in Gun.cs

    public GameObject enemyHit; //for use by AoE

    CircleCollider2D collide2D;

    Image bulletImg;

    // Use this for initialization
    void Start () {
        bulletImg = GetComponent<Image>();

        //set up collider to correct size
        RectTransform sr = (RectTransform)transform;
        float radius = sr.rect.size.x / 2;
        collide2D = GetComponent<CircleCollider2D>();
        collide2D.radius = radius;

        //get rt
        rt = (RectTransform)transform;
    }
	
	// Update is called once per frame
	void Update () {
        //don't do anything if paused
        if (Pause.paused)
            return;
        //basic movement
        rt.anchoredPosition = new Vector2(rt.anchoredPosition.x + vx, rt.anchoredPosition.y + vy);
        //get distance traveled
        distance = (float)Math.Sqrt((rt.anchoredPosition.x - spawnx) * (rt.anchoredPosition.x - spawnx)
                                + (rt.anchoredPosition.y - spawny) * (rt.anchoredPosition.y - spawny));
        //destroy self at end of track
        if (distance > TRACK_LENGTH + (Dial.DIAL_RADIUS - spawnDistFromCenter))
        {
            Collide();
        }
    }

    //actions for this bullet to take upon hitting something
    public void Collide()
    {
        GameObject.Destroy(gameObject);
    }

    public void UpdateSpawnDist()
    {
        RectTransform rt = (RectTransform)transform;
        spawnDistFromCenter = Mathf.Sqrt((spawnx * spawnx) + (spawny * spawny));
    }

    public bool CheckActive()
    {
        return isActive;
    }
}
