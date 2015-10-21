using UnityEngine;

public class SlowingLinearMover : EnemyMover{
	
	EnemyController parent;
	float logmin = .2f;
	float logmax = 1.2f;
	float logscale;
	float logoffset;
	
	public SlowingLinearMover(EnemyController ec){
		parent = ec;
		logscale = Mathf.Log (logmax)-Mathf.Log (logmin);
		logoffset = -Mathf.Log (logmin);
	}
	
	public override Vector2 PositionFromProgress (float progress){
		float angle = RealRadiansOfEnemy (parent);
		angle += radiansOffset*Mathf.Deg2Rad;
		float logprog = Mathf.Log(progress + logmin) + logoffset;
		logprog /= logscale;
		
		float travelDistance = logprog * DialController.TRACK_LENGTH;
		float distFromCenter = DialController.FULL_LENGTH - travelDistance;
		float x = distFromCenter * Mathf.Cos (angle);
		float y = distFromCenter * Mathf.Sin (angle);
		return new Vector2 (x, y);
	}
}

