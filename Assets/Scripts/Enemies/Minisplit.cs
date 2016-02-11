using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class Minisplit : Enemy{
	
	Timer snapTimer;
	bool snapping = false;
	float snapTime = 1f;
	
	float startX = 0f;
	float startY = 0f;
	float targX = 0f;
	float targY = 0f;
	
	public override void Start(){
		base.Start ();
		//snapTimer = new Timer();
	}
	
	public void SetPolarTarget(float thetaDegrees,float destinationRadius,Megasplit parent){
		return;
		rt = (RectTransform)transform;
		startX = ((RectTransform)(parent.transform)).anchoredPosition.x;
		startY = ((RectTransform)(parent.transform)).anchoredPosition.y;
		
		//get the track info
		float degrees = (trackID-1)*60; //clockwise of y-axis
		degrees += 15*trackLane; //negative trackpos is left side, positive is right side, 0 is middle
		degrees = ((360-degrees) + 90)%360; //convert to counterclockwise of x axis
		degrees *= Mathf.Deg2Rad;
		
		targX = destinationRadius*Mathf.Cos(degrees + thetaDegrees*Mathf.Deg2Rad);
		targY = destinationRadius*Mathf.Sin(degrees + thetaDegrees*Mathf.Deg2Rad);
		
		snapping = true;
	} 
	
	public override void AddToBonus(List<System.Object> bonusList){
		//don't add minisplits to bonus
	}
	
	public override void Update(){
		/*if(snapping){
			if(snapTimer.TimeElapsedSecs() > snapTime){
				snapping = false;
			}else{
				float snapProg = snapTimer.TimeElapsedSecs() / snapTime;
				float xPos = startX + snapProg*(targX-startX);
				float yPos = startY + snapProg*(targY-startY);
				rt.anchoredPosition = new Vector2(xPos,yPos);
			}
		}else{
			base.Update();
		}*/
		base.Update ();
	}
	
}
