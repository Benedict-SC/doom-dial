using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShieldTrapHolder : MonoBehaviour {

    List<ShieldTrap> shields;
    List<int> lanesCovered;
    List<Gun> towers;

	// Use this for initialization
	void Start () {
        
	}

    void Awake()
    {
        
    }

    public void SetUp()
    {
        shields = new List<ShieldTrap>();
        lanesCovered = new List<int>();
        towers = new List<Gun>();
        for (int i = 1; i <= 6; i++)
        {
            //Debug.Log("looking for Gun" + i);
            towers.Add(GameObject.Find("Gun" + i).GetComponent<Gun>());
        }
        //Debug.Log("towers size: " + towers.Count);
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    public void AddShield(ShieldTrap st)
    {
        //Debug.Log(shields == null);
        shields.Add(st);
        lanesCovered.Add(st.GetMyLane());
    }

    //sets each shield's sprites based on position relative to other shields
    public void SetShieldSprites()
    {
        //TODO
    }

    //applies instant cooldown reduction to all shielded towers, amt in seconds
    //ie cuts amt seconds off of each tower's cooldown
    public void ReduceCooldown(float amt)
    {
        foreach (Gun g in towers)
        {
            if (lanesCovered.Contains(g.GetCurrentLaneID() - 1))
            {
                g.ReduceCooldownInstant(amt);
            }
        }
    }

    //destroys self if empty
    public void DestroyIfEmpty()
    {
        if (shields.Count == 0 && lanesCovered.Count == 0)
        {
            Destroy(gameObject);
        }
    }

    //destroys entire shield chain
    public void DestroyEntireShield()
    {
        for (int i = 0; i < shields.Count; i++)
        {
            Destroy(shields[i].gameObject);
        }
        Destroy(gameObject);
    }
}
