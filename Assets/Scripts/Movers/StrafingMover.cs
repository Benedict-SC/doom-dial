using UnityEngine;

public class StrafingMover : EnemyMover{
	
	Enemy parent;
	
	float strafeDeviation = 12.5f;
		
	bool mirrored = false;
	
	float[] legs = {.0625f,.1875f,.25f,.4375f,.5f,.625f,.6875f,.8125f,.875f,.9375f,1.0f};
	
	public StrafingMover(Enemy ec){
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
			float ringWidth = Dial.ENEMY_SPAWN_LENGTH - Dial.middle_radius;
			float travelDist = ProgressThroughLeg(progress,0)*(ringWidth/2.0f); //how far in you've gone, in unity units
			distFromCenter = Dial.ENEMY_SPAWN_LENGTH - travelDist;
		}else if(progress < legs[1]){//strafe right
			distFromCenter = (Dial.ENEMY_SPAWN_LENGTH + Dial.middle_radius)/2.0f;
			deviation = ProgressThroughLeg(progress,1)*strafeDeviation;
		}else if(progress < legs[2]){//move to edge of middle ring
			deviation = strafeDeviation;
			float ringWidth = Dial.ENEMY_SPAWN_LENGTH - Dial.middle_radius;
			float travelDist = (ringWidth/2.0f) + ProgressThroughLeg(progress,2)*(ringWidth/2.0f);
			distFromCenter = Dial.ENEMY_SPAWN_LENGTH - travelDist;
		}else if(progress < legs[3]){//strafe left
			distFromCenter = Dial.middle_radius;
			deviation = strafeDeviation - ProgressThroughLeg(progress,3)*2*strafeDeviation;
		}else if(progress < legs[4]){//move to halfway through middle ring
			deviation = -strafeDeviation;
			float ringWidth = Dial.middle_radius - Dial.inner_radius;
			float travelDist = ProgressThroughLeg(progress,4)*(ringWidth/2.0f);
			distFromCenter = Dial.middle_radius - travelDist;
		}else if(progress < legs[5]){//strafe right
			distFromCenter = (Dial.middle_radius + Dial.inner_radius)/2.0f;
			deviation = -strafeDeviation + ProgressThroughLeg(progress,5)*2*strafeDeviation;
		}else if(progress < legs[6]){//move to edge of inner ring
			deviation = strafeDeviation;
			float ringWidth = Dial.middle_radius - Dial.inner_radius;
			float travelDist = (ringWidth/2.0f) + ProgressThroughLeg(progress,6)*(ringWidth/2.0f);
			distFromCenter = Dial.middle_radius - travelDist;
		}else if(progress < legs[7]){//strafe left
			distFromCenter = Dial.inner_radius;
			deviation = strafeDeviation - ProgressThroughLeg(progress,7)*2*strafeDeviation;
		}else if(progress < legs[8]){//move to halfway through inner ring
			deviation = -strafeDeviation;
			float ringWidth = Dial.inner_radius - Dial.DIAL_RADIUS;
			float travelDist = ProgressThroughLeg(progress,8)*(ringWidth/2.0f);
			distFromCenter = Dial.inner_radius - travelDist;
		}else if(progress < legs[9]){//strafe right
			distFromCenter = (Dial.inner_radius + Dial.DIAL_RADIUS)/2.0f;
			deviation = -strafeDeviation + ProgressThroughLeg(progress,9)*strafeDeviation;
		}else{//head to dial
			deviation = 0.0f;
			float ringWidth = Dial.inner_radius - Dial.DIAL_RADIUS;
			float travelDist = (ringWidth/2.0f) + ProgressThroughLeg(progress,10)*(ringWidth/2.0f);
			distFromCenter = Dial.inner_radius - travelDist;
		}
		
		if(mirrored)
			deviation *= -1.0f;
		angle += deviation * Mathf.Deg2Rad;
		
		//float lineDistance = progress * Dial.ENEMY_TRACK_LENGTH;
		//float distFromCenter = Dial.ENEMY_SPAWN_LENGTH - lineDistance;
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

