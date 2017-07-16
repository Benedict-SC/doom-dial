using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class Steering : MonoBehaviour{

	//public RectTransform testTarget;
	AIPath enemyPath = null;
	
	public bool allowedToMove = true;
	public Enemy enemy = null;//fight the power
	
	public Vector2 vel = Vector2.zero;
	public Vector2 acc = Vector2.zero;
	RectTransform rt = null;
	void Start(){rt = GetComponent<RectTransform>();}
	
	bool matchOrientationToVelocity = true;
	
	public float maxSpeed = 3f;
	public float referenceMaxSpeed = 3f;
	public float maxAccel = 0.5f;
	public float referenceMaxAccel = 0.5f;
	public int lookAhead = 1;	
	
	public float speedMult = 1f;
	public float boostDuration = 0f;
	Timer boostTimer;

	public Vector2 pftarget;

	Dictionary<System.Object,Vector2> externalForces = new Dictionary<System.Object,Vector2>();
	
	void Update(){
		if(enemy == null){return;}
		if(!enemy.moving){
			return;
		}
		//speed boost should be allowed to expire even when enemy is frozen
		if(speedMult != 1f && boostTimer != null){
			if(boostTimer.TimeElapsedSecs() > boostDuration){
				speedMult = 1f;
			}
		}
		if(enemy.IsFrozen()){
			return;
		}if(!allowedToMove){
			return;
		}if(stunned){
			return;
		}
		
		if(!clipVelocity){ //being knocked back
			acc = drag;
		}else if(enemyPath != null){ //pathfollowing
			FollowPath(enemyPath);
		}
		
		//external forces
		Vector2 adjustedAcceleration = acc;
		foreach(KeyValuePair<System.Object,Vector2> entry in externalForces)
		{
			adjustedAcceleration += entry.Value;
		}

		vel += adjustedAcceleration;
		
		if(vel.magnitude > maxSpeed && clipVelocity){
			vel.Normalize();
			vel *= maxSpeed * speedMult;
		}
		rt.anchoredPosition += vel;
		if(matchOrientationToVelocity){
			transform.eulerAngles = new Vector3(0f,0f,Mathf.Atan2(vel.y,vel.x)*Mathf.Rad2Deg);
		}
	}
	
	public void Seek(Vector2 target){
		Vector2 linear = target - rt.anchoredPosition;
		linear.Normalize();
		linear *= maxAccel;
		acc = linear;
	}
	public void FollowPath(AIPath path){
		if(rt == null){
			rt = GetComponent<RectTransform>();
		}
		Vector2 target = path.PathFollowingTarget(rt.anchoredPosition,lookAhead);
		pftarget = target;
		Seek (target);
	}
	public void StartFollowingPath(AIPath path){
		enemyPath = path;
	}
	
	#region StatusEffects (when status fucks up movement)
	public bool clipVelocity = true;
	Vector2 drag = Vector2.zero;
	float knockbackConstant = 20f;
	public bool stunned = false;
	public void Knockback(float knockbackpower){
		RevertSpeed ();
		Vector2 dir = (rt.anchoredPosition/* - bulletpos*/).normalized;
		Vector2 knockForce = dir*knockbackConstant*knockbackpower;
		vel += knockForce;
		clipVelocity = false;
		drag = -knockForce/16f;
		stunned = false;
	}
	public void Slow(float speed){
		maxSpeed = referenceMaxSpeed*speed;
		clipVelocity = true;
		stunned = false;
	}
	public void RevertSpeed(){
		maxSpeed = referenceMaxSpeed;
	}
	public void Stun(){
		stunned = true;
		clipVelocity = true;
		RevertSpeed();
	}
	public void ExternalForceUpdate(System.Object caller,Vector2 force){
		if(externalForces.ContainsKey(caller)){
			externalForces[caller] = force;
		}else{
			externalForces.Add(caller,force);
		}
	}
	public void SpeedBoost(float multiplier, float seconds){
        Debug.Log("applied speedboost of " + multiplier);
		speedMult *= multiplier; //allow for stacking
		boostDuration = seconds;
		boostTimer = new Timer();		
	}
	#endregion

}
