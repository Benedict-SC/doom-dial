using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class Melded : Enemy{
	
	public override void AddToBonus(List<System.Object> bonusList){
		
			Dictionary<string,System.Object> enemyDict = new Dictionary<string,System.Object>();
			enemyDict.Add("enemyID","melders");
			enemyDict.Add("trackID",(long)GetCurrentTrackID());
		if(!spawnedByBoss)
			bonusList.Add(enemyDict);
		
	}
	
}
