using UnityEngine;

public class ZigzagMover : EnemyMover{
	
	EnemyController parent;
	
	float firstEdge = 0.25f;
	float secondEdge = 0.5f;
	float thirdEdge = 0.75f;
	
	
	bool mirrored = false;
	
	public ZigzagMover(EnemyController ec){
		parent = ec;
	}
	
	public override Vector2 PositionFromProgress (float progress){
		float angle = RealRadiansOfEnemy(parent);
		
		float deviation = 0.0f;
		if(progress < firstEdge){
			float scale = 1.0f/firstEdge;
			float progThroughFirstLeg = progress * scale;
			deviation = -30.0f * progThroughFirstLeg;
		}else if(progress < secondEdge){
			float scale = 1.0f/(secondEdge-firstEdge);
			float progThroughSecondLeg = (progress - firstEdge)*scale;
			deviation = -30.0f + (60.0f * progThroughSecondLeg);
		}else if(progress < thirdEdge){
			float scale = 1.0f/(thirdEdge-secondEdge);
			float progThroughThirdLeg = (progress - secondEdge)*scale;
			deviation = 30.0f - (60.0f * progThroughThirdLeg);
		}else{
			float scale = 1.0f/(1.0f-thirdEdge);
			float progThroughLastLeg = (progress - thirdEdge)*scale;
			deviation = -30.0f + (30.0f * progThroughLastLeg);
		}
		
		if(mirrored)
			deviation *= -1.0f;
		
		angle += deviation * Mathf.Deg2Rad;
	
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
