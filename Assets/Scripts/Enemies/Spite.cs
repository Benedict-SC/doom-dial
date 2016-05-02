/*Thom*/

using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class Spite : Enemy
{
    bool damageRisen = false;

    public override void Update()
    {
        //if you hit this guy with slowdown bullet, its dmg +20
        if (slowInProgress == true && !damageRisen)
        {
            Debug.Log("Spite raised damage due to slowdown!");
            damageRisen = true;
            impactDamage += 20f;
        }
        base.Update();
    }
}