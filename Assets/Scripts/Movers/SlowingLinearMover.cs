using UnityEngine;

public class SlowingLinearMover : EnemyMover{
	
	Enemy parent;
	float logmin = .2f;
	float logmax = 1.2f;
	float logscale;
	float logoffset;
	
	public SlowingLinearMover(Enemy ec){
		parent = ec;
		logscale = Mathf.Log (logmax)-Mathf.Log (logmin);
		logoffset = -Mathf.Log (logmin);
	}
	
	public override Vector2 PositionFromProgress (float progress){
		float angle = RealRadiansOfEnemy (parent);
		float logprog = Mathf.Log(progress + logmin) + logoffset;
		logprog /= logscale;
		
		float travelDistance = logprog * Dial.ENEMY_TRACK_LENGTH;
		float distFromCenter = Dial.ENEMY_SPAWN_LENGTH - travelDistance;
		float x = distFromCenter * Mathf.Cos (angle);
		float y = distFromCenter * Mathf.Sin (angle);
		return new Vector2 (x, y);
	}
}

