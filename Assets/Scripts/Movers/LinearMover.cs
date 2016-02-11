using UnityEngine;

public class LinearMover : EnemyMover{

	Enemy parent;

	public LinearMover(Enemy ec){
		parent = ec;
	}

	public override Vector2 PositionFromProgress (float progress){
		float angle = RealRadiansOfEnemy (parent);
		float travelDistance = progress * Dial.ENEMY_TRACK_LENGTH;
		float distFromCenter = Dial.ENEMY_SPAWN_LENGTH - travelDistance;
		float x = distFromCenter * Mathf.Cos (angle);
		float y = distFromCenter * Mathf.Sin (angle);
		return new Vector2 (x, y);
	}
}
