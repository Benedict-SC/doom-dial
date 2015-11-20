using UnityEngine;

public class SineMover : EnemyMover{
	
	Enemy parent;
	float pies = 4.0f;
	float devWidth = 11.0f * Mathf.Deg2Rad;
	
	bool mirrored = false;
	
	public SineMover(Enemy ec){
		parent = ec;
	}
	
	public override Vector2 PositionFromProgress (float progress){
		float angle = RealRadiansOfEnemy (parent);
		float scaledProgress = progress * pies * Mathf.PI;
		float deviation = Mathf.Sin (scaledProgress);
		if(mirrored)
			deviation *= -1.0f;
		angle += devWidth * deviation;

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

