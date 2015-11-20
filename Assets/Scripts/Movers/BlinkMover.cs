using UnityEngine;

public class BlinkMover : EnemyMover{
	
	Enemy parent;
	
	bool mirrored = false;
	float devWidth = 15.0f * Mathf.Deg2Rad;
	
	public BlinkMover(Enemy ec){
		parent = ec;
	}
	
	public override Vector2 PositionFromProgress (float progress){
		float angle = RealRadiansOfEnemy (parent);
		float deviation = 0f;
		
		float outerBarrier = (Dial.FULL_LENGTH-Dial.middle_radius)/Dial.TRACK_LENGTH;
		float middleBarrier = (Dial.FULL_LENGTH-Dial.inner_radius)/Dial.TRACK_LENGTH;
		
		if(progress < outerBarrier){
			deviation = devWidth;
		}else if(progress < middleBarrier){
			deviation = 0f;
		}else{
			deviation = -devWidth;
		}
		
		if(!mirrored)
			deviation *= -1.0f;
		angle += deviation;
		
		float lineDistance = progress * Dial.TRACK_LENGTH;
		float distFromCenter = Dial.FULL_LENGTH - lineDistance;
		float x = distFromCenter * Mathf.Cos (angle);
		float y = distFromCenter * Mathf.Sin (angle);
		return new Vector2 (x, y);
	}
	
	public void Mirror(){
		mirrored = !mirrored;
	}
}



