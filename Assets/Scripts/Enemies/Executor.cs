using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class Executor : Enemy
{
    protected float SPEED_CONST = 5.0f; //always added to impactTime so it's not insanely fast
    protected float origImpTime; //original impact time, as shown in the json

    public override void Start()
    {
        base.Start();
        origImpTime = impactTime;
    }

    public override void Update()
    {
        impactTime *= (dialCon.health / 100f);
        impactTime += SPEED_CONST;
        Debug.Log("impactTime is " + impactTime);
        base.Update();
        impactTime = origImpTime; //set speed back to default to recalc each frame
        Debug.Log("impactTime set back to " + impactTime);
    }
}