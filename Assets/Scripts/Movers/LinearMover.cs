using UnityEngine;

public class LinearMover : EnemyMover{

	EnemyController parent;

	public LinearMover(EnemyController ec){
		parent = ec;
	}

	public override Vector2 PositionFromProgress (float progress){
		float angle = RealRadiansOfEnemy (parent);
		angle += radiansOffset*Mathf.Deg2Rad;
		float travelDistance = progress * DialController.TRACK_LENGTH;
		float distFromCenter = DialController.FULL_LENGTH - travelDistance;
		float x = distFromCenter * Mathf.Cos (angle);
		float y = distFromCenter * Mathf.Sin (angle);
		return new Vector2 (x, y);
	}
}
