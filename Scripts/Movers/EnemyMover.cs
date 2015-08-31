using UnityEngine;

public abstract class EnemyMover{

	public abstract Vector2 PositionFromProgress (float progress);

	public float RealRadiansOfEnemy(EnemyController ec){
		float degrees = (ec.GetTrackID()-1)*60; //clockwise of y-axis
		degrees += 15*ec.GetTrackLane(); //negative trackpos is left side, positive is right side, 0 is middle
		degrees = ((360-degrees) + 90)%360; //convert to counterclockwise of x axis
		degrees *= Mathf.Deg2Rad;
		return degrees;
	}

}
