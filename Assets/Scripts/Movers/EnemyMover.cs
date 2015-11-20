using UnityEngine;

public abstract class EnemyMover{

	protected float radiansOffset = 0;
	public abstract Vector2 PositionFromProgress (float progress);

	public float RealRadiansOfEnemy(EnemyController ec){ //returns angle of WHERE IT STARTED, not current angle
		float degrees = (ec.GetTrackID()-1)*60; //clockwise of y-axis
		degrees += 15*ec.GetTrackLane(); //negative trackpos is left side, positive is right side, 0 is middle
		degrees = ((360-degrees) + 90)%360; //convert to counterclockwise of x axis
		degrees *= Mathf.Deg2Rad;
		return degrees;
	}
	public float RealRadiansOfEnemy(Enemy ec){ //returns angle of WHERE IT STARTED, not current angle
		float degrees = (ec.GetTrackID()-1)*60; //clockwise of y-axis
		degrees += 15*ec.GetTrackLane(); //negative trackpos is left side, positive is right side, 0 is middle
		degrees = ((360-degrees) + 90)%360; //convert to counterclockwise of x axis
		degrees *= Mathf.Deg2Rad;
		return degrees;
	}
	public void PutInLeftLane(){
		radiansOffset = -15f;
	}
	public void PutInRightLane(){
		radiansOffset = 15f;
	}
	public void LeftOffset(float f){
		radiansOffset -= f;
	}
	public void RightOffset(float f){
		radiansOffset += f;
	}

}
