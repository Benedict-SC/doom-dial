using UnityEngine;

public class BlinkMover : EnemyMover{
	
	EnemyController parent;
	
	bool mirrored = false;
	float devWidth = 15.0f * Mathf.Deg2Rad;
	
	public BlinkMover(EnemyController ec){
		parent = ec;
	}
	
	public override Vector2 PositionFromProgress (float progress){
		float angle = RealRadiansOfEnemy (parent);
		float deviation = 0f;
		
		float outerBarrier = (DialController.FULL_LENGTH-DialController.middle_radius)/DialController.TRACK_LENGTH;
		float middleBarrier = (DialController.FULL_LENGTH-DialController.inner_radius)/DialController.TRACK_LENGTH;
		
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
		
		float lineDistance = progress * DialController.TRACK_LENGTH;
		float distFromCenter = DialController.FULL_LENGTH - lineDistance;
		float x = distFromCenter * Mathf.Cos (angle);
		float y = distFromCenter * Mathf.Sin (angle);
		return new Vector2 (x, y);
	}
	
	public void Mirror(){
		mirrored = !mirrored;
	}
}



