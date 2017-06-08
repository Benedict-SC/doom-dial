using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.IO;
using MiniJSON;

public class Skizzard : Boss{

	float damage = 5f;
	float lifeDrain = 0f;
	float lifeDrainPerRevolution = 1f;
	
	float startingTheta = 0f;
	float startingSpeed = .003f;
	float speedPerRevolution = .002f;
	float maxSpeed = 0.05f;
	
	float enemyBoostMultiplier = 2f;
	float enemyBoostDuration = 4f;
	
	float diveAcc = 0.03f;
	
	GameObject overlay;

	enum state{MOVING,DIVING,RETREATING};
	state currentState = state.MOVING;
	
	public override void Start(){
		base.Start();
		maxHP = 150;
		hp = 100;
		thetas = new Vector3(startingTheta,startingSpeed,0f);
		overlay = transform.Find("Health").gameObject;
	}
	public override void Update(){
		base.Update ();
		if(thetaOverflowedThisFrame){
			OnRevolution();
		}
		if(currentState == state.RETREATING){
			if(radii.x > Dial.FULL_LENGTH-3){
				radii.x = Dial.FULL_LENGTH-3;
				radii.y = 0f;
				radii.z = 0f;
				currentState = state.MOVING;
			}
		}
		//Debug.Log(currentState);
	}
	public override void OnTriggerEnter2D(Collider2D coll){
		base.OnTriggerEnter2D(coll);
		overlay.transform.localScale = new Vector3(1f,hp/maxHP,1f);
	}
	
	public void OnRevolution(){
		if(currentState == state.MOVING){//start a dive if a dive isn't already in progress
			currentState = state.DIVING;
			radii.y = -0.2f;
			radii.z = -diveAcc;
		}
	
		if(mode >= 1){//gain speed
			if(thetas.y < maxSpeed){
				thetas.y += speedPerRevolution;
				if(thetas.y > maxSpeed)
					thetas.y = maxSpeed;
			}
		}
		if(mode >= 2){//enemies gain speed
			List<Enemy> enemylist = Dial.GetAllEnemies();
			foreach(Enemy e in enemylist){
				GameObject zappything = GameObject.Instantiate(Resources.Load ("Prefabs/MainCanvas/ZappyThing")) as GameObject;
				zappything.GetComponent<RectTransform>().sizeDelta = e.GetComponent<RectTransform>().sizeDelta * 1.7f;
				zappything.GetComponent<Lifespan>().BeginLiving(3f);
				zappything.GetComponent<Lifespan>().SetImageToFade(true);
				zappything.transform.SetParent(e.transform,false);
				
				e.SpeedUp(enemyBoostMultiplier,enemyBoostDuration);
			}
		}
		if(mode >= 3){//gain life drain
			lifeDrain += lifeDrainPerRevolution;
		}
	}
	
	public void HitDial(){
		if(currentState == state.MOVING)
			return; //don't want to hit it the instant we spawn
		//this is a direct HP reduction, which doesn't call any of the stuff an enemy_arrived event would- do something fancier later
		GameEvent dmgEvent = new GameEvent("dial_damaged");
		dmgEvent.addArgument(gameObject);
		dmgEvent.addArgument(damage + lifeDrain);
		EventManager.Instance().RaiseEvent(dmgEvent);
		HealDamage(lifeDrain);
		
		currentState = state.RETREATING;
		radii.y = -radii.y / 2;
		radii.z = diveAcc;		
	}
	
}
