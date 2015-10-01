using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class StatReadout : MonoBehaviour, EventHandler{

	Text text;

	public void Start(){
		text = gameObject.GetComponent<Text>();
		EventManager em = EventManager.Instance();
		em.RegisterForEventType("readout_update",this);
	}
	public void Update(){
	
	}
	public void HandleEvent(GameEvent ge){
		Dictionary<string,float> dict = ge.args[0] as Dictionary<string,float>;
		
		float damage = dict["damage"];
		float range = dict["range"];
		float speed = dict["speed"];
		float cdr = dict["cdr"];
		float slowMin = dict["slowMin"];
		float slowMax = dict["slowMax"];
		float stun = dict["stun"];
		float knockback = dict["knockback"];
		float poison = dict["poison"];
		float drain = dict["drain"];
		float shred = dict["shred"];
		float penetration = dict["penetration"];
		int split = (int)dict["split"];
		int spread = (int)dict["spread"];
		float homing = dict["homing"];
		float arc = dict["arc"];
		float splash = dict["splash"];
		//bonuses
		int pierce =(int) dict["pierce"];
		int aoe = (int) dict["aoe"];
		int antiregen = (int) dict["antiregen"];
		bool hijackregen = dict["hijackregen"] > 0;
		bool chainstun = dict["chainstun"] > 0;
		bool chainpoison = dict["chainpoison"] > 0;
		bool lethargy = dict["lethargy"] > 0;
		bool divide = dict["divide"] > 0;
		bool circle = dict["circle"] > 0;
		
		string sign = "";
		if(damage > 0)
			sign = "";
		else
			sign = "-";
		string damageLine = "Damage: " + sign + (int)damage;
		string speedLine = "Speed: " + speed + " seconds to edge";
		string rangeLine = "Distance: " + (100*range) + "% of track";
		string reloadLine = "Reload: " + cdr + " seconds";
		string penetrationLine = "Penetration: ignore " + (penetration*100) + "% of shields";
		string shredLine = "Shield Shred: break " + (shred*100) + "% of shields";
		
		string finalText = "TOWER STATS:\n\n";
		finalText += damageLine + "\n";
		finalText += speedLine + "\n";
		finalText += rangeLine + "\n";
		finalText += reloadLine + "\n";
		if(penetration > 0)
			finalText += penetrationLine + "\n";
		if(shred > 0)
			finalText += shredLine + "\n";
			
		text.text = finalText;
	}

}
