using UnityEngine;

public class SidestepMover : EnemyMover{
	
	Enemy parent;
	
	float strafeDeviation = 14f;
	
	bool mirrored = false;
	
	float[] legs = {.2f,.4f,.6f,.8f,1.0f};
	
	public SidestepMover(Enemy ec){
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
			float ringWidth = Dial.FULL_LENGTH - Dial.middle_radius;
			float travelDist = ProgressThroughLeg(progress,0)*(ringWidth); //how far in you've gone, in unity units
			distFromCenter = Dial.FULL_LENGTH - travelDist;
		}else if(progress < legs[1]){//strafe right
			distFromCenter = Dial.middle_radius;
			deviation = ProgressThroughLeg(progress,1)*strafeDeviation;
		}else if(progress < legs[2]){//move to edge of middle ring
			deviation = strafeDeviation;
			float ringWidth = Dial.middle_radius-Dial.inner_radius;
			float travelDist = ProgressThroughLeg(progress,2)*(ringWidth);
			distFromCenter = Dial.middle_radius - travelDist;
		}else if(progress < legs[3]){//strafe left
			distFromCenter = Dial.inner_radius;
			deviation = strafeDeviation - ProgressThroughLeg(progress,3)*strafeDeviation;
		}else{//head to dial
			deviation = 0.0f;
			float ringWidth = Dial.inner_radius - Dial.DIAL_RADIUS;
			float travelDist = ProgressThroughLeg(progress,4)*(ringWidth);
			distFromCenter = Dial.inner_radius - travelDist;
		}
		
		if(mirrored)
			deviation *= -1.0f;
		angle += deviation * Mathf.Deg2Rad;
		
		//float lineDistance = progress * Dial.TRACK_LENGTH;
		//float distFromCenter = Dial.FULL_LENGTH - lineDistance;
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


