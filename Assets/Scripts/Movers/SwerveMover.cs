using UnityEngine;

public class SwerveMover : EnemyMover{
	
	EnemyController parent;
	
	bool mirrored = false;
	float devWidth = 14.0f * Mathf.Deg2Rad;
	float swerveEndProg = 0.5f;
	float conversionFactor;
	
	public SwerveMover(EnemyController ec){
		parent = ec;
		conversionFactor = devWidth/(swerveEndProg*swerveEndProg);
	}
	
	public override Vector2 PositionFromProgress (float progress){
		float angle = RealRadiansOfEnemy (parent);
		angle += radiansOffset*Mathf.Deg2Rad;
		float deviation = 0f;
		
		if(progress <= swerveEndProg){
			float squareProg = progress*progress;
			deviation = conversionFactor * squareProg;
		}else{
			deviation = devWidth;
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


