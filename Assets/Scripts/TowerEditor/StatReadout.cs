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
		Debug.Log("updated stats readout");
		Dictionary<string,float> dict = ge.args[0] as Dictionary<string,float>;
		string towerType = (string)(ge.args[1]);
		
		float cooldown = dict["cooldown"];
		float energyGain = dict["energyGain"];
		float comboKey = dict["comboKey"];
		float dmg = dict["dmg"];
		int trapUses = (int)(dict["trapUses"]);
		float shieldDurability = dict["shieldDurability"];
		float charge = dict["charge"];
		int split = (int)(dict["split"]);
		int penetration = (int)(dict["penetration"]);
		float continuousStrength = dict["continuousStrength"];
		float reflect = dict["reflect"];
		float frequency = dict["frequency"];
		float tempDisplace = dict["tempDisplace"];
		float absorb = dict["absorb"];
		float aoe = dict["aoe"];
		float attraction = dict["attraction"];
		int duplicate = (int)(dict["duplicate"]);
		float field = dict["field"];
		float selfRepairRate = dict["selfRepairRate"];
		float selfRepairAmt = dict["selfRepairAmt"];
		
		string finalText = "TOWER STATS:\n\n";
		finalText += string.Format("{0:N2}", cooldown) + " second cooldown\n";
		if(towerType.Equals("Bullet")){
			if(continuousStrength <= 0){
				finalText += "Deals " + string.Format("{0:N1}", dmg) + " total damage\n";
				if(charge > 0){
					finalText += "Explosion size +" + string.Format("{0:N1}", charge) + "\n";
				}
				if(split > 1){
					finalText += split + "-bullet burst fire\n";
				}
			}else{
				finalText += "Deals " + string.Format("{0:N1}", (dmg/100f)) + " damage per frame.\n";
				finalText += "^THIS IS BAD, HAVE BENEDICT REWORK IT TO BE DAMAGE PER SECOND\n";
				if(charge > 0){
					finalText += "Beam width increases over time.\n";
				}
				if(split > 1){
					finalText += "Fires " + (split-1) + " extra lasers.\n";
				}
			}
			if(penetration > 0){
				finalText += "Pierces through " + penetration + " enemies and shields\n";
			}
		}else if(towerType.Equals("Trap")){
			finalText += "Deals " + string.Format("{0:N1}", dmg) + " damage\n";
			if(trapUses > 1){
				finalText += "Goes off " + trapUses + " times.\n";
			}else{
				finalText += "Goes off once.\n";
			}
			if(aoe > 0){
				finalText += "Fires an area of effect (size " + aoe + ")\n";
			}
			if(attraction > 0){
				finalText += "Pulls in enemies (strength " + attraction + ")\n";
			}
			if(duplicate > 0){
				finalText += "Triggers " + (duplicate + 1) + " times every hit.\n";
			}
			if(field > 0){
				finalText += "Leaves a damaging field behind for " + field + " seconds.\n";
			}
		}else if(towerType.Equals("Shield")){
			finalText += shieldDurability + " HP\n";
			if(frequency > 0){
				finalText += "Regenerates " + frequency + " HP/sec\n";
			}
			if(reflect > 0){
				finalText += "Deals " + ((int)(reflect*100f)) + "% of enemy damage back to it.\n";
			}
			if(tempDisplace > 0){
				finalText += "Teleports enemy back " + ((int)(reflect*100f)) + "% of the track.\n";
			}
			if(absorb > 0){
				finalText += "Max HP increases by " + ((int)absorb) + " when shield survives a hit.\n";
			}
		}else if(towerType.Equals("BulletTrap")){
			finalText += "Fires " + ((int)(split + 3)) + " bullets in a radius on hit.\n"; 
			finalText += "Deals " + string.Format("{0:N1}", dmg) + " damage per bullet.\n";
			if(charge > 0){
				finalText += "Bullet explosion size +" + string.Format("{0:N1}", charge) + "\n";
			}
			if(aoe > 0){
				finalText += "Bullet range is " + aoe + " higher than base.\n";
			}
			if(attraction > 0){
				finalText += "Bullets home in on enemies (strength " + attraction + "\n)";
			}
		}else if(towerType.Equals("BulletShield")){
			finalText += "3 charges\n";
			if(continuousStrength <= 0){
				finalText += "Fires a pulse when hit.\n";
			}else{
				finalText += "Fires " + ((int)continuousStrength) + " pulses when hit.\n";
			}
			finalText += "Pulse deals " + string.Format("{0:N1}", dmg) + " damage\n";
			if(penetration > 0){
				finalText += "Lowers enemy shield max HP by " + (penetration * 5) + ".\n";
			}
			if(reflect > 0){
				finalText += "Deals " + ((int)(reflect*100f)) + "% of enemy damage back per pulse.\n";
			}
			if(frequency > 0){
				finalText += "Slows enemies by " + ((int)(frequency*15f)) + "% for " + frequency + " seconds.\n";
			}
		}else if(towerType.Equals("TrapShield")){
			if (absorb > 0)
            {
                finalText += "Absorbs " + ((int)absorb * ShieldTrap.drainMultiplier) + "% of enemy HP when hit.\n";
            }
            if (duplicate > 0)
            {
                finalText += "Covers a total of " + duplicate + " zones.\n";
            }
            if (tempDisplace > 0)
            {
                finalText += "When hit, reduces cooldown for the tower in the current lane by " + (tempDisplace * ShieldTrap.cooldownMult).ToString("0.0") + " seconds.\n";
            }
            if (field > 0)
            {
                finalText += "When destroyed, leaves behind an HP absorption field that lasts for " + (int)field + " seconds.\n";
                if (tempDisplace > 0)
                {
                    finalText += "While enemies are in contact with this field, cooldown for the tower in this zone is sped up.\n";
                }
            }
		}else{
			Debug.Log("invalid tower type in stat readout");
		}
		text.text = finalText;
	}

}
