using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShieldTrapHolder : MonoBehaviour {

    List<ShieldTrap> shields;
    List<int> lanesCovered;

	// Use this for initialization
	void Start () {
        
	}

    void Awake()
    {
        shields = new List<ShieldTrap>();
        lanesCovered = new List<int>();
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
