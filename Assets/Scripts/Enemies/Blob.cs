using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class Blob : Enemy{

	float blastDamage = 0f;
	Timer blastTimer;
	float blastDuration = 1.5f;
	bool blowingUp = false;
	
	bool sittingThere = false;
	
	public override void Start(){
		base.Start ();
		blastTimer = new Timer();
	}

	public override void Update(){
		if (!moving){
			base.Update();
			return;
		}
		base.Update();
		
		if(!sittingThere && progress > 0.5f){
			sittingThere = true;
			steering.allowedToMove = false;
		}
		
		if(blowingUp){
			if(blastTimer.TimeElapsedSecs() >= blastDuration){
				blowingUp = false;
				//hurt dial
				GameEvent boom = new GameEvent("dial_damaged");
				boom.addArgument(this.gameObject);
				boom.addArgument(blastDamage);
				EventManager.Instance().RaiseEvent(boom);
				//hurt boss if applicable
				GameObject bossObj = GameObject.FindWithTag("Boss");
				if(bossObj != null){
					Boss b = bossObj.GetComponent<Boss>();
					b.TakeDamage(blastDamage);
				}
				//hurt enemies
				List<Enemy> casualties = Dial.GetAllEnemiesInZone(GetCurrentTrackID());
				foreach(Enemy e in casualties){
					if(e != this){
						e.TakeDamage(blastDamage);
					}
				}
				base.Die();
			}
		}
	}
	public override void OnTriggerEnter2D(Collider2D coll){
		base.OnTriggerEnter2D(coll);
		if(coll.gameObject.tag.Equals("Enemy")){
			//absorb the enemy
			Enemy e = coll.gameObject.GetComponent<Enemy>();
			e.canDropPiece = false;
			e.Die();
			radius += 0.02f;
			ScaleEnemy();
			blastDamage += 5;
		}
	}
	public override void Die ()
	{
		dead = true;
		if(!blowingUp){
			blastTimer.Restart();
			blowingUp = true;
			//play the explosion animation
		}
		
	}

}
