using UnityEngine;

public class WinkMover : EnemyMover{
	
	Enemy parent;
	
	bool mirrored = false;
	float devWidth = 15.0f * Mathf.Deg2Rad;
	
	public WinkMover(Enemy ec){
		parent = ec;
	}
	
	public override Vector2 PositionFromProgress (float progress){
		float angle = RealRadiansOfEnemy (parent);
		float deviation = 0f;
		
		float outerBarrier = (Dial.ENEMY_SPAWN_LENGTH-Dial.middle_radius)/Dial.ENEMY_TRACK_LENGTH;
		float middleBarrier = (Dial.ENEMY_SPAWN_LENGTH-Dial.inner_radius)/Dial.ENEMY_TRACK_LENGTH;
		
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
		
		float lineDistance = progress * Dial.ENEMY_TRACK_LENGTH;
		float distFromCenter = Dial.ENEMY_SPAWN_LENGTH - lineDistance;
		float x = distFromCenter * Mathf.Cos (angle);
		float y = distFromCenter * Mathf.Sin (angle);
		return new Vector2 (x, y);
	}
	
	public void Mirror(){
		mirrored = !mirrored;
	}
}




