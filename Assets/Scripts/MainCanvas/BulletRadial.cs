﻿using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class BulletRadial : Bullet {

    public float range; //max range, distance at which the bullet dies
    public GameObject ignoredEnemy; //enemy that tripped the ptrap - is ignored
    int initialLane;

    void Start()
    {
        base.Start();
        initialLane = GetCurrentLaneID();
    }
	
	// Update is called once per frame
	void Update ()
    {
        Debug.Log("BulletRadial range is " + range);
        //don't do anything if paused
        if (Pause.paused)
            return;
        //basic movement
        rt.anchoredPosition = new Vector2(rt.anchoredPosition.x + vx, rt.anchoredPosition.y + vy);
        //get distance traveled
        distance = (float)Math.Sqrt((rt.anchoredPosition.x - spawnx) * (rt.anchoredPosition.x - spawnx)
                                + (rt.anchoredPosition.y - spawny) * (rt.anchoredPosition.y - spawny));
        Debug.Log("BulletRadial distance is " + distance);
        //die at range or (not apparently) at zone borders
        //dying when hitting the dial is handled in Dial.cs's OnTriggerEnter2D()
        if (distance > range/* || initialLane != GetCurrentLaneID()*/)
        {
            Debug.Log("calling Collide() on BulletRadial");
            Collide();
        }
    }

    //uses position to return lane ID, 1-6 clockwise
    int GetCurrentLaneID()
    {
        int result = -1;

        float x = rt.anchoredPosition.x;
        float y = rt.anchoredPosition.y;
        //Debug.Log("bulletRadial coords are " + x + ", " + y);
        float angle = Mathf.Atan2(y, x); //angle in radians measured from x-axis
        //Debug.Log("initial angle measured is " + angle);
        angle %= 2 * Mathf.PI;
        //Debug.Log("angle after conversion is " + angle);
        float pi = Mathf.PI;
        if (angle > pi / 6 && angle < pi / 2)
        {
            result = 1;
        }
        else if ((angle < pi / 6 && angle > 0f) || (angle > 11 * pi / 6))
        {
            result = 2;
        }
        else if (angle > 3 * pi / 2 && angle < 11 * pi / 6)
        {
            result = 3;
        }
        else if (angle > 7 * pi / 6 && angle < 3 * pi / 2)
        {
            result = 4;
        }
        else if (angle > 5 * pi / 6 && angle < 7 * pi / 6)
        {
            result = 5;
        }
        else if (angle > pi / 2 && angle < 5 * pi / 6)
        {
            result = 6;
        }
        if (result == -1)
        {
            //Debug.Log("BulletRadial's GetCurrentLaneID() failed, returned -1");
        }
        return result;
    }
}
