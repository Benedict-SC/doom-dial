using System;
using UnityEngine;
using UnityEngine.UI;

public class SplitFirer : MonoBehaviour{
    Bullet template;
    float firingTime = 0.3f;
    Timer fireTimer;
    int maxShots;
    int shotsFired = 0;
    float givenAngle;
    public void Configure(Bullet b, int times, float angle){
        template = b;
        maxShots = times;
        givenAngle = angle;
    }
    public void Start(){
        fireTimer = new Timer();
    }
    public void Update(){
        if(fireTimer.TimeElapsedSecs() > (shotsFired * (firingTime/ (maxShots-1) ) )){
            FireShot();
            if(shotsFired >= maxShots){ //finish firing
                Destroy(this.gameObject);
            }
        }
    }
    void FireShot(){
        GameObject newBullet = GameObject.Instantiate(template.gameObject);
        newBullet.transform.SetParent(Dial.spawnLayer,false);
        RectTransform rt = GetComponent<RectTransform>();
        RectTransform bulletRect = newBullet.GetComponent<RectTransform>();
        Bullet bc = newBullet.GetComponent<Bullet>();
        //following code copied from SpawnBulletI in Gun.cs
        //if it's having problems where normal bullets aren't, check to see if the code there has diverged
			float angle = (givenAngle +  90) % 360 ;
			angle *= (float)Math.PI / 180;
			//find where to spawn the bullet
			float gunDistFromCenter = (float)Math.Sqrt (rt.anchoredPosition.x*rt.anchoredPosition.x + rt.anchoredPosition.y*rt.anchoredPosition.y);
			gunDistFromCenter += 0.47f;
			bc.spawnx = gunDistFromCenter * (float)Math.Cos (angle);
			bc.spawny = gunDistFromCenter * (float)Math.Sin (angle);
			bc.UpdateSpawnDist();
			//Debug.Log (bc.speed);
			bulletRect.anchoredPosition = new Vector2(bc.spawnx,bc.spawny);
			bc.transform.rotation = transform.rotation;
			bc.vx = bc.speed * (float)Math.Cos(angle);
			bc.vy = bc.speed * (float)Math.Sin(angle);
        newBullet.SetActive(true);
        shotsFired++;
    }
}