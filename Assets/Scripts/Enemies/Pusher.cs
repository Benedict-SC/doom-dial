/*Thom*/

using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class Pusher : Enemy
{
    bool damageRisen = false;

    //change this as needed to adjust what percentage of a trap's dmg the pusher gets
    float ExtraDMG_SCALAR = 1f; 

    public override void Update()
    {
        base.Update();
    }

    public override void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.tag == "Trap")
        {
            //parent the trap to this enemy
            Trap trapCon = other.gameObject.GetComponent<Trap>();
            RectTransform rt = other.gameObject.GetComponent<RectTransform>();
            rt.SetParent(GetComponent<RectTransform>(), false); //should this be false?.. //false yeah
            impactDamage += trapCon.dmg * ExtraDMG_SCALAR;
            Debug.Log("Pusher's new damage after getting Trap is: " + impactDamage);
        }
        base.OnTriggerEnter2D(other);
    }

    public override void Die()
    {
        transform.DetachChildren(); //this detatches the health and the collider, so they stay on the map
        //which we don't want, so try and update this so it only detaches the trap
        base.Die();
    }
}