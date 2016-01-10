using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class Cheater : Enemy{
	bool shieldedOnce = false;
	public override void Update(){
		base.Update();
		if(shield == null && !shieldedOnce){
			if(hp <= (maxhp/2f)){
				List<System.Object> fragList = new List<System.Object>();
				Dictionary<string,System.Object> fragDict = new Dictionary<string,System.Object>();
				fragDict.Add("fragAngle",0.0);
				fragDict.Add("fragArc",360.0);
				fragList.Add(fragDict);
				GiveShield(50f,2f,0.04f,fragList);
				shieldedOnce = true;
			}
		}
	}
	
}

