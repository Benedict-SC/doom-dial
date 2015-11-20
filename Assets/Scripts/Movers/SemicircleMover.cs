using UnityEngine;

public class SemicircleMover : EnemyMover{
	
	Enemy parent;
	
	bool mirrored = false;
	float devWidth = 16.0f * Mathf.Deg2Rad;
	float circleProg = 0.8f;
	
	public SemicircleMover(Enemy ec){
		parent = ec;
	}
	
	public override Vector2 PositionFromProgress (float progress){
		float angle = RealRadiansOfEnemy (parent);
		float deviation = 0f;
		
		if(progress <= circleProg/2){
			float adjustedProg = (circleProg/2)-progress;
			float percentProg = (1.0f/(circleProg/2))*adjustedProg;
			deviation = devWidth * (Mathf.Sqrt(1.0f - (percentProg*percentProg)));
		}else if(progress <= circleProg){
			float adjustedProg = progress - (circleProg/2);
			float percentProg = (1.0f/(circleProg/2))*adjustedProg;
			deviation = devWidth * (Mathf.Sqrt(1.0f - (percentProg*percentProg)));
		}else{
			deviation = 0f;
		}
		
		if(mirrored)
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


