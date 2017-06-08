using UnityEngine;
using UnityEngine.UI;

public class BigBulkShieldEater : MonoBehaviour{
	GameObject target;
	RectTransform targetRT;
	
	EnemyShield targetShield;
	float initialHP = 0;
	
	GameObject head;
	GameObject beam;
	Image headImg;
	Image beamImg;
	
	BigBulk bulk = null;
	RectTransform bossRT = null;
	
	enum states{SLEEPING,REACHING,GRABBING,DRAINING,FADING,DONE};
	states state = states.SLEEPING;
	
	Timer grabTimer;
	Timer drainTimer;
	float drainTime = 2.5f;
	
	float radians = 0f;
	float distance = 0f;
	float reachTime = 1f;
	
	float headMinimum = 30f;
	float headRadius = 18;
	
	float fastLegDist;
	float mediLegDist;
	float slowLegDist;
	
	//these are for one leg of the journey- total time is x2
	readonly float MIN_REACH_TIME = .5f;
	readonly float MAX_BONUS_TIME = .4f;
	readonly float GRAB_TIME = .3f;
	//thresholds of time to produce a slowing effect
	readonly float SLOW_THRESH = 0.66f;
	readonly float MEDI_THRESH = 0.33f;
	//distance percent intervals
	readonly float FAST_LEG = 0.7f;
	readonly float MEDI_LEG = 0.2f;
	readonly float SLOW_LEG = 0.1f;
	
	RectTransform rt;
	
	public void Start(){
		rt = (RectTransform)transform;
		head = transform.Find("ShieldEaterHead").gameObject;
		beam = transform.Find("ShieldEaterBeam").gameObject;
		headImg = head.GetComponent<Image>();
		beamImg = beam.GetComponent<Image>();
		grabTimer = new Timer();
		drainTimer = new Timer();
	}
	public void Update(){
		if(state == states.REACHING){
			float progress = grabTimer.TimeElapsedSecs() / reachTime;
			float dist;
			
			if(progress >= 1f){ //if we've reached past
				state = states.GRABBING;
				dist = distance;
				grabTimer.Restart();
			}else if(progress > SLOW_THRESH){
				float subProgress = (progress-SLOW_THRESH)/(1f-SLOW_THRESH);
				dist = headMinimum + fastLegDist + mediLegDist + subProgress*slowLegDist;
			}else if(progress > MEDI_THRESH){
				float subProgress = (progress-MEDI_THRESH)/(SLOW_THRESH-MEDI_THRESH); //how much of the medium speed leg has it travered?
				dist = headMinimum + fastLegDist + subProgress*mediLegDist; //get actual distance
			}else{
				float subProgress = progress/MEDI_THRESH; //how much of the fast leg has it traversed?
				dist = headMinimum + subProgress*fastLegDist; //get actual distance from center to put the thing
				
			}
			if(progress >= 1f)
				PositionHead();
			else
				PositionHead(dist);
		}else if(state == states.GRABBING){
			drainTimer.Restart();
			state = states.DRAINING;
		}else if(state == states.DRAINING){
			float drainProg = drainTimer.TimeElapsedSecs() / drainTime;
			
			if(targetShield != null){
				float targetHP = initialHP - (initialHP*drainProg);
				float currentHP = targetShield.GetCurrentHP();
				float drainedAmount = targetShield.Drain(currentHP - targetHP);
				bulk.ReceiveDrainedShields(drainedAmount);
			}
			
			PositionHead();
			if(drainTimer.TimeElapsedSecs() > drainTime){
				targetShield.Drain(targetShield.GetBaseHP());//drain any remaining shield
				state = states.FADING;
			}
		}else if(state == states.FADING){
			float progress = 1f- (grabTimer.TimeElapsedSecs() / reachTime);
			headImg.color = new Color(headImg.color.r,headImg.color.g,headImg.color.b,progress);
			beamImg.color = new Color(beamImg.color.r,beamImg.color.g,beamImg.color.b,progress);
			float dist = headMinimum + fastLegDist + mediLegDist + slowLegDist;
			PositionHead(dist);
			if(progress < 0){
				target.GetComponent<Enemy>().UndoBulkDrainTarget();
				state = states.DONE;
			}
		}else if(state == states.DONE){
			Destroy (gameObject);
		}
	}
	void PositionHead(float dist){
		if(target == null){
			state = states.FADING;
			return;
		}
		radians = Mathf.Atan2(targetRT.anchoredPosition.y-bossRT.anchoredPosition.y,targetRT.anchoredPosition.x-bossRT.anchoredPosition.x) - (Mathf.PI/2); //what angle's it at?
		transform.eulerAngles = new Vector3(transform.eulerAngles.x,transform.eulerAngles.y,
		                                    Rotations.RadiansCounterclockwiseFromXAxisToEulerAngles(radians));//-bulk.transform.eulerAngles.z); //match angle
		distance = Mathf.Sqrt((targetRT.anchoredPosition.x-bossRT.anchoredPosition.x)*(targetRT.anchoredPosition.x-bossRT.anchoredPosition.x) + 
		                      (targetRT.anchoredPosition.y-bossRT.anchoredPosition.y)*(targetRT.anchoredPosition.y-bossRT.anchoredPosition.y));
		
		float zoneDist = distance - headMinimum; //how far is it from the hidden head of the beam, not the center of the dial?
		fastLegDist = FAST_LEG*zoneDist;
		mediLegDist = MEDI_LEG*zoneDist;
		slowLegDist = SLOW_LEG*zoneDist;
	
		RectTransform headRT = (RectTransform)head.transform;
		headRT.anchoredPosition = new Vector2(0,dist);//new Vector2(dist*Mathf.Cos(radians),dist*Mathf.Sin(radians));
		RectTransform beamRT = (RectTransform)beam.transform;
		float beamLength = dist - headRadius;
		beamRT.localScale = new Vector3(beamRT.localScale.x,beamLength / beamRT.sizeDelta.y,beamRT.localScale.z);
	}
	void PositionHead(){
		if(target == null){
			state = states.FADING;
			return;
		}
		radians = Mathf.Atan2(targetRT.anchoredPosition.y-bossRT.anchoredPosition.y,targetRT.anchoredPosition.x-bossRT.anchoredPosition.x) - (Mathf.PI/2); //what angle's it at?
		transform.eulerAngles = new Vector3(transform.eulerAngles.x,transform.eulerAngles.y,
		                                    Rotations.RadiansCounterclockwiseFromXAxisToEulerAngles(radians));//-bulk.transform.eulerAngles.z); //match angle
		distance = Mathf.Sqrt((targetRT.anchoredPosition.x-bossRT.anchoredPosition.x)*(targetRT.anchoredPosition.x-bossRT.anchoredPosition.x) + 
		                      (targetRT.anchoredPosition.y-bossRT.anchoredPosition.y)*(targetRT.anchoredPosition.y-bossRT.anchoredPosition.y));
		
		float zoneDist = distance - headMinimum; //how far is it from the hidden head of the beam, not the center of the dial?
		fastLegDist = FAST_LEG*zoneDist;
		mediLegDist = MEDI_LEG*zoneDist;
		slowLegDist = SLOW_LEG*zoneDist;
		
		RectTransform headRT = (RectTransform)head.transform;
		headRT.anchoredPosition = new Vector2(0,distance);//new Vector2(dist*Mathf.Cos(radians),dist*Mathf.Sin(radians));
		RectTransform beamRT = (RectTransform)beam.transform;
		float beamLength = distance - headRadius;
		beamRT.localScale = new Vector3(beamRT.localScale.x,beamLength / beamRT.sizeDelta.y,beamRT.localScale.z);
	}
	public void SetBigBulk(BigBulk bb){
		bulk = bb;
		bossRT = (RectTransform)bb.transform;
	}
	public void SetTarget(GameObject t){ //also starts the tractor beam moving
		target = t;
		targetRT = (RectTransform)t.transform;
		targetShield = t.GetComponent<Enemy>().GetShield();
		initialHP = targetShield.GetCurrentHP();
		
		radians = Mathf.Atan2(targetRT.anchoredPosition.y-bossRT.anchoredPosition.y,targetRT.anchoredPosition.x-bossRT.anchoredPosition.x); //what angle's it at?
		transform.eulerAngles = new Vector3(transform.eulerAngles.x,transform.eulerAngles.y,
											Rotations.RadiansCounterclockwiseFromXAxisToEulerAngles(radians)-bulk.transform.eulerAngles.z); //match angle
		distance = Mathf.Sqrt((targetRT.anchoredPosition.x-bossRT.anchoredPosition.x)*(targetRT.anchoredPosition.x-bossRT.anchoredPosition.x) + 
		                      (targetRT.anchoredPosition.y-bossRT.anchoredPosition.y)*(targetRT.anchoredPosition.y-bossRT.anchoredPosition.y));
		
		float zoneDist = distance - headMinimum; //how far is it from the hidden head of the beam, not the center of the dial?
		fastLegDist = FAST_LEG*zoneDist;
		mediLegDist = MEDI_LEG*zoneDist;
		slowLegDist = SLOW_LEG*zoneDist;
		
		float bonusTimeMultiplier = 0; //replace with actual calculation later, between 0 and 1 based on distance
		reachTime = MIN_REACH_TIME + (bonusTimeMultiplier*MAX_BONUS_TIME); //set how long it's going to take to reach the drop
		
		state = states.REACHING;
		grabTimer = new Timer();
	}
}
