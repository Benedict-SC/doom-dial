using UnityEngine;
using System;
using System.Collections;

public class FullZoneWave : MonoBehaviour {

    float SCALE_FINAL = 3; //scale of wave at end of the lane
    Vector3 originalScale;
    float TRACK_LENGTH = 110.8f + 5; //copied from Bullet
    float speed = 1.5f; //speed of wave
    OldShield parentShield = null; //if a shield created This, set this value
    Dial dial;

    float vx;
    float vy;
    float spawnx;
    float spawny;
    float gunDistFromCenter;

    public float damage = 0;
    public float knockback; //Reversion -- ask joe what this means??
    public bool stun; //whether stun wave is sent out
    public bool slowdown; //gravity well to center of track
    public bool lifeDrain; //converts life taken past its' HP to yours (??? ask joe)
    public float poison; //puff out poison shotgun bursts - amt of damage per second
    public float poisonDur; //length of poison effect in second

    //vampire drain stuff
    public float vampDrain = 15f;
    bool vampIsOn = false;

    // Use this for initialization
    void Start () {
        transform.SetParent(Dial.spawnLayer, false);
        RectTransform shieldRT = (RectTransform)parentShield.transform;
        RectTransform rt = (RectTransform)transform;
        //find your angle
        float parentAngle = parentShield.gameObject.transform.eulerAngles.z;
        float angle = (parentAngle + 90) % 360;
        angle *= (float)Math.PI / 180;
        //Debug.Log ("original angle: " + angle);
        angle = (angle - (float)Math.PI / 6f) + ((((float)Math.PI / 3f) / (2))); //handles spread effect
        //find where to spawn the wave
        gunDistFromCenter = (float)Math.Sqrt(shieldRT.anchoredPosition.x * shieldRT.anchoredPosition.x +
                                                   shieldRT.anchoredPosition.y * shieldRT.anchoredPosition.y);
        gunDistFromCenter += 0.47f;
        spawnx = gunDistFromCenter * (float)Math.Cos(angle);
        spawny = gunDistFromCenter * (float)Math.Sin(angle);
        //Debug.Log (bc.speed);
        rt.anchoredPosition = new Vector2(spawnx, spawny);
        transform.rotation = parentShield.gameObject.transform.rotation;
        vx = speed * (float)Math.Cos(angle);
        vy = speed * (float)Math.Sin(angle);

        originalScale = rt.localScale;

        dial = GameObject.Find("Dial").GetComponent<Dial>();

        if (PlayerPrefsInfo.Int2Bool(PlayerPrefs.GetInt(PlayerPrefsInfo.s_vampire)))
        {
            vampIsOn = true;
        }
    }

    // Update is called once per frame
    void Update()
    {
        //movement
        RectTransform rt = (RectTransform)transform;
        rt.anchoredPosition = new Vector2(rt.anchoredPosition.x + vx, rt.anchoredPosition.y + vy);
        float distance = (float)Math.Sqrt((rt.anchoredPosition.x - spawnx) * (rt.anchoredPosition.x - spawnx)
                                               + (rt.anchoredPosition.y - spawny) * (rt.anchoredPosition.y - spawny));
        //distance checking
        if (distance > TRACK_LENGTH + (Dial.DIAL_RADIUS - gunDistFromCenter))
        {
            Destroy(this.gameObject);
        }
        //scaling
        Vector3 newScale = Vector3.Lerp(originalScale, originalScale * 3, distance / TRACK_LENGTH);
        rt.localScale = newScale;
    }

    //when it hits an enemy
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.tag == "Enemy")
        {
            Enemy ec = other.gameObject.GetComponent<Enemy>();
            if (vampIsOn)
            {
                float ehp = ec.GetHP();
                if (ehp < damage * vampDrain)
                {
                    dial.ChangeHealth(ehp);
                }
                else
                {
                    dial.ChangeHealth(damage * vampDrain);
                }
            }
            ec.TakeDamage(damage);
            Debug.Log("inflicting zone wave statuses");
            ec.ZoneWaveInflictedStatus(this);
        }
    }

    //fun visual/sound effects :D
    void Effects()
    {

    }

    public void SetParentShield(OldShield sc)
    {
        parentShield = sc;
    }

    public void SetDamage(float dmg)
    {
        damage = dmg;
    }

    public void SetSpeed(float spd)
    {
        speed = spd;
    }

    /*
    //sets holderShield's values from a given shield
    public void ConfigureHolderFromParent(Shield sc)
    {
        damage = sc.splashDmg;
        knockback = sc.knockback;
        stun = sc.stun;
        slowdown = sc.slowdown;
        lifeDrain = sc.lifeDrain;
        poison = sc.poison;
        poisonDur = sc.poisonDur;
    }
    */
}
