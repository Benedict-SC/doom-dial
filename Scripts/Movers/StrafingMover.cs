using UnityEngine;

public class StrafingMover : EnemyMover{
	
	EnemyController parent;
	
	float strafeDeviation = 12.5f;
		
	bool mirrored = false;
	
	float[] legs = {.0625f,.1875f,.25f,.4375f,.5f,.625f,.6875f,.8125f,.875f,.9375f,1.0f};
	
	public StrafingMover(EnemyController ec){
		parent = ec;
	}
	
	public override Vector2 PositionFromProgress (float progress){
		float angle = RealRadiansOfEnemy(parent);
		
		float deviation = 0.0f;
		float distFromCenter = 0.0f;
		//note from duncan: there are eleven legs to this path
		//and I've decided to represent all of them with an if-else chain
		//because i am a good and professional programmer
		//who just happens to be up at 2am without having eaten in eight hours
		if(progress < legs[0]){ //move to halfway through outer ring
			deviation = 0.0f;
			float ringWidth = DialController.FULL_LENGTH - DialController.middle_radius;
			float travelDist = ProgressThroughLeg(progress,0)*(ringWidth/2.0f); //how far in you've gone, in unity units
			distFromCenter = DialController.FULL_LENGTH - travelDist;
		}else if(progress < legs[1]){//strafe right
			distFromCenter = (DialController.FULL_LENGTH + DialController.middle_radius)/2.0f;
			deviation = ProgressThroughLeg(progress,1)*strafeDeviation;
		}else if(progress < legs[2]){//move to edge of middle ring
			deviation = strafeDeviation;
			float ringWidth = DialController.FULL_LENGTH - DialController.middle_radius;
			float travelDist = (ringWidth/2.0f) + ProgressThroughLeg(progress,2)*(ringWidth/2.0f);
			distFromCenter = DialController.FULL_LENGTH - travelDist;
		}else if(progress < legs[3]){//strafe left
			distFromCenter = DialController.middle_radius;
			deviation = strafeDeviation - ProgressThroughLeg(progress,3)*2*strafeDeviation;
		}else if(progress < legs[4]){//move to halfway through middle ring
			deviation = -strafeDeviation;
			float ringWidth = DialController.middle_radius - DialController.inner_radius;
			float travelDist = ProgressThroughLeg(progress,4)*(ringWidth/2.0f);
			distFromCenter = DialController.middle_radius - travelDist;
		}else if(progress < legs[5]){//strafe right
			distFromCenter = (DialController.middle_radius + DialController.inner_radius)/2.0f;
			deviation = -strafeDeviation + ProgressThroughLeg(progress,5)*2*strafeDeviation;
		}else if(progress < legs[6]){//move to edge of inner ring
			deviation = strafeDeviation;
			float ringWidth = DialController.middle_radius - DialController.inner_radius;
			float travelDist = (ringWidth/2.0f) + ProgressThroughLeg(progress,6)*(ringWidth/2.0f);
			distFromCenter = DialController.middle_radius - travelDist;
		}else if(progress < legs[7]){//strafe left
			distFromCenter = DialController.inner_radius;
			deviation = strafeDeviation - ProgressThroughLeg(progress,7)*2*strafeDeviation;
		}else if(progress < legs[8]){//move to halfway through inner ring
			deviation = -strafeDeviation;
			float ringWidth = DialController.inner_radius - DialController.DIAL_RADIUS;
			float travelDist = ProgressThroughLeg(progress,8)*(ringWidth/2.0f);
			distFromCenter = DialController.inner_radius - travelDist;
		}else if(progress < legs[9]){//strafe right
			distFromCenter = (DialController.inner_radius + DialController.DIAL_RADIUS)/2.0f;
			deviation = -strafeDeviation + ProgressThroughLeg(progress,9)*strafeDeviation;
		}else{//head to dial
			deviation = 0.0f;
			float ringWidth = DialController.inner_radius - DialController.DIAL_RADIUS;
			float travelDist = (ringWidth/2.0f) + ProgressThroughLeg(progress,10)*(ringWidth/2.0f);
			distFromCenter = DialController.inner_radius - travelDist;
		}
		
		if(mirrored)
			deviation *= -1.0f;
		angle += deviation * Mathf.Deg2Rad;
		
		//float lineDistance = progress * DialController.TRACK_LENGTH;
		//float distFromCenter = DialController.FULL_LENGTH - lineDistance;
		float x = distFromCenter * Mathf.Cos (angle);
		float y = distFromCenter * Mathf.Sin (angle);
		return new Vector2 (x, y);
	}
	
	public float ProgressThroughLeg(float progress, int leg){ 
		float length = 0.0f;
		if(leg == 0){
			length = legs[0];
		}else{
			length = legs[leg]-legs[leg-1];
			progress -= legs[leg-1];
		}
		return progress/length;
	}
	
	public void Mirror(){
		mirrored = !mirrored;
	}
}

