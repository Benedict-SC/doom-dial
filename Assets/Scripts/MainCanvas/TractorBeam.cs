using UnityEngine;
using UnityEngine.UI;

public class TractorBeam : MonoBehaviour{

	GameObject target;
	GameObject head;
	GameObject beam;
	
	enum states{SLEEPING,REACHING,GRABBING,PULLING,DONE};
	states state = states.SLEEPING;
	
	Timer grabTimer;
	
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

	public void Start(){
		head = transform.FindChild("TractorBeamHead").gameObject;
		beam = transform.FindChild("TractorBeamBeam").gameObject;
		grabTimer = new Timer();
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
			PositionHead(dist);
		}else if(state == states.GRABBING){
			if(grabTimer.TimeElapsedSecs() > GRAB_TIME){
				grabTimer.Restart();
				RectTransform dropRT = (RectTransform)target.transform;
				target.transform.eulerAngles = new Vector3(0,0,0);
				target.transform.SetParent(head.transform,false);
				dropRT.anchoredPosition = new Vector2(0,0);	
				state = states.PULLING;
			}
		}else if(state == states.PULLING){
			float progress = 1f- (grabTimer.TimeElapsedSecs() / reachTime);
			float dist;
			
			if(progress > SLOW_THRESH){
				float subProgress = (progress-SLOW_THRESH)/(1f-SLOW_THRESH);
				dist = headMinimum + fastLegDist + mediLegDist + subProgress*slowLegDist;
			}else if(progress > MEDI_THRESH){
				float subProgress = (progress-MEDI_THRESH)/(SLOW_THRESH-MEDI_THRESH); //how much of the medium speed leg has it travered?
				dist = headMinimum + fastLegDist + subProgress*mediLegDist; //get actual distance
			}else if(progress > 0f){
				float subProgress = progress/MEDI_THRESH; //how much of the fast leg has it traversed?
				dist = headMinimum + subProgress*fastLegDist; //get actual distance from center to put the thing
			}else{
				state = states.DONE;
				dist = headMinimum;
			}
			PositionHead(dist);
		}else if(state == states.DONE){
			Drop seized = target.GetComponent<Drop>();
			seized.AddPieceToInventory();
			Destroy (seized.gameObject);
			Destroy (gameObject);
		}
	}
	void PositionHead(float dist){
		RectTransform headRT = (RectTransform)head.transform;
		headRT.anchoredPosition = new Vector2(0,dist);//new Vector2(dist*Mathf.Cos(radians),dist*Mathf.Sin(radians));
		RectTransform beamRT = (RectTransform)beam.transform;
		float beamLength = dist - headRadius;
		beamRT.localScale = new Vector3(beamRT.localScale.x,beamLength / beamRT.sizeDelta.y,beamRT.localScale.z);
	}
	
	public void SetTarget(GameObject drop){ //also starts the tractor beam moving
		target = drop;
		RectTransform dropRT = (RectTransform)drop.transform;
		
		radians = (-Mathf.PI/2)+Mathf.Atan2(dropRT.anchoredPosition.y,dropRT.anchoredPosition.x); //what angle's it at?
		transform.eulerAngles = new Vector3(transform.eulerAngles.x,transform.eulerAngles.y,radians*Mathf.Rad2Deg); //match angle
		distance = Mathf.Sqrt(dropRT.anchoredPosition.x*dropRT.anchoredPosition.x + dropRT.anchoredPosition.y*dropRT.anchoredPosition.y);
	
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
