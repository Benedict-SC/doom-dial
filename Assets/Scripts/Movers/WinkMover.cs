using UnityEngine;

public class WinkMover : EnemyMover{
	
	EnemyController parent;
	
	bool mirrored = false;
	float devWidth = 15.0f * Mathf.Deg2Rad;
	
	public WinkMover(EnemyController ec){
		parent = ec;
	}
	
	public override Vector2 PositionFromProgress (float progress){
		float angle = RealRadiansOfEnemy (parent);
		float deviation = 0f;
		
		float outerBarrier = (DialController.FULL_LENGTH-DialController.middle_radius)/DialController.TRACK_LENGTH;
		float middleBarrier = (DialController.FULL_LENGTH-DialController.inner_radius)/DialController.TRACK_LENGTH;
		
		if(progress < outerBarrier){
			deviation = 0f;
		}else if(progress < middleBarrier){
			deviation = devWidth;
		}else{
			deviation = 0f;
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




