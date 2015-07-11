using UnityEngine;

public class SineMover : EnemyMover{
	
	EnemyController parent;
	float pies = 4.0f;
	float devWidth = 11.0f * Mathf.Deg2Rad;
	
	public SineMover(EnemyController ec){
		parent = ec;
	}
	
	public override Vector2 PositionFromProgress (float progress){
		float angle = RealRadiansOfEnemy (parent);
		float scaledProgress = progress * pies * Mathf.PI;
		float deviation = Mathf.Sin (scaledProgress);
		angle += devWidth * deviation;

		float lineDistance = progress * DialController.TRACK_LENGTH;
		float distFromCenter = DialController.FULL_LENGTH - lineDistance;
		float x = distFromCenter * Mathf.Cos (angle);
		float y = distFromCenter * Mathf.Sin (angle);
		return new Vector2 (x, y);
	}
}

