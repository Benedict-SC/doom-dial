using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class ProjectileTrap : Trap {

    //these apply to radial bullets
    static float DEFAULT_BULLET_RANGE = 100f;
    float bulletRange = DEFAULT_BULLET_RANGE;
    int bulletCount;
    float bulletHoming;
    float bulletCharge;

    protected override void FireEffect()
    {
        Debug.Log("PTrap FireEffect() called");
        //aoe adds bullet range
        if (aoe > 0f)
        {
            bulletRange = aoe;
        }
        //split adds # of bullets
        //ie if split is 1, it'll spawn 1 bullet, and so on
        bulletCount = split;
        //charge adds size of explosion
        if (charge > 0f)
        {
            bulletCharge = charge;
        }
        //attraction adds homing on bullets
        if (attraction > 0f)
        {
            bulletHoming = attraction;
        }

        SpawnRadialBullets(bulletCount);
    }

    //spawns bCount bullets in equal intervals, in a circle around This PTrap
    void SpawnRadialBullets(int bCount)
    {
        for (int i = 0; i <= bCount - 1; i++)
        {
            Debug.Log ("called instantiate bullet in ProjectileTrap.cs");
            GameObject bullet = Instantiate(Resources.Load("Prefabs/MainCanvas/BulletRadial")) as GameObject; //make a bullet
            RectTransform bulletRect = (RectTransform)bullet.transform;
            RectTransform rt = GetComponent<RectTransform>(); //this pt's recttransform
            BulletRadial bc = bullet.GetComponent<BulletRadial>();
            bullet.transform.SetParent(Dial.spawnLayer, false);
            //make it the type of bullet this thing fires
            ConfigureBullet(bc);

            //find your angle/handle spread (equidistant bullets around trap)
            float ownangle = this.transform.eulerAngles.z; //rotation of this trap
            float angle = (ownangle + 90) % 360; //rotation of this trap
            angle *= (float)Math.PI / 180;
            //Debug.Log ("original angle: " + angle);
            //set angle for each bullet
            float circleSlice = 360 / bCount;
            circleSlice *= Mathf.Deg2Rad;
            angle = (angle + (circleSlice * i));
            
            //find where to spawn the bullet
            bc.spawnx = rt.anchoredPosition.x;
            bc.spawny = rt.anchoredPosition.y;
            bc.UpdateSpawnDist();
            //Debug.Log (bc.speed);
            bulletRect.anchoredPosition = new Vector2(bc.spawnx, bc.spawny);
            bc.transform.rotation = transform.rotation;
            bc.vx = bc.speed * (float)Math.Cos(angle);
            bc.vy = bc.speed * (float)Math.Sin(angle);
        }
    }

    //configure a radial bullet's stats
    void ConfigureBullet(BulletRadial bc)
    {
        bc.charge = bulletCharge;
        bc.attraction = bulletHoming;
        bc.range = bulletRange;
        bc.ignoredEnemy = enemyHit; //bullets ignore the enemy that tripped this trap
    }
}
