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
	public float referenceMaxAccel = .5f;
	public int lookAhead = 1;	
	
	void Update(){
		if(enemy == null){return;}
		if(!enemy.moving){
			return;
		}if(enemy.IsFrozen()){
			return;
		}if(!allowedToMove){
			return;
		}if(stunned){
			return;
		}
		if(enemyPath != null){
			FollowPath(enemyPath);
		}
		
		vel += acc;
		
		if(vel.magnitude > maxSpeed && clipVelocity){
			vel.Normalize();
			vel *= maxSpeed;
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
		Seek (target);
	}
	public void StartFollowingPath(AIPath path){
		enemyPath = path;
	}
	
	#region StatusEffects (when status fucks up movement)
	public bool clipVelocity = true;
	float knockbackConstant = 9f;
	public bool stunned = false;
	public void Knockback(Vector2 bulletpos,float knockbackpower){
		RevertSpeed ();
		Vector2 dir = (rt.anchoredPosition/* - bulletpos*/).normalized;
		Vector2 knockForce = dir*knockbackConstant*knockbackpower;
		vel += knockForce;
		clipVelocity = false;
		maxAccel = knockForce.magnitude / 20f;
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
	#endregion

}
