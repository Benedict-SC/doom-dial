using UnityEngine;

public class MoverToPathConverter : MonoBehaviour{
	public string moveString = "None";
	public int pointResolution = 30;
	public void ProducePath(PathEditor pe){
		Enemy e = GenerateTestEnemy();
	
		EnemyMover mover;
		if(moveString.Equals("Linear")){
			mover = new LinearMover(e);
		}else if(moveString.Equals("Linear_Right")){
			mover = new LinearMover(e);
			mover.PutInRightLane();
		}else if(moveString.Equals("Linear_Left")){
			mover = new LinearMover(e);
			mover.PutInLeftLane();
		}else if(moveString.Equals("Slowing_Linear")){
			mover = new SlowingLinearMover(e);
		}else if(moveString.Equals("Slowing_Linear_Right")){
			mover = new SlowingLinearMover(e);
			mover.PutInRightLane();
		}else if(moveString.Equals("Slowing_Linear_Left")){
			mover = new SlowingLinearMover(e);
			mover.PutInLeftLane();
		}else if(moveString.Equals ("Zigzag")){
			mover = new ZigzagMover(e);
		}else if(moveString.Equals ("Zigzag_Mirror")){
			ZigzagMover zm = new ZigzagMover(e);
			zm.Mirror();
			mover = zm;
		}else if(moveString.Equals ("Strafing")){
			mover = new StrafingMover(e);
		}else if(moveString.Equals ("Strafing_Mirror")){
			StrafingMover sm = new StrafingMover(e);
			sm.Mirror();
			mover = sm;
		}else if(moveString.Equals ("Sine")){
			mover = new SineMover(e);
		}else if(moveString.Equals ("Sine_Mirror")){
			SineMover sm = new SineMover(e);
			sm.Mirror();
			mover = sm;
		}else if(moveString.Equals ("Swerve_Right")){
			mover = new SwerveMover(e);
		}else if(moveString.Equals ("Swerve_Left")){
			SwerveMover sm = new SwerveMover(e);
			sm.Mirror();
			mover = sm;
		}else if(moveString.Equals ("Swerve_In_Left")){
			SwerveMover sm = new SwerveMover(e);
			sm.PutInLeftLane();
			mover = sm;
		}else if(moveString.Equals ("Swerve_In_Right")){
			SwerveMover sm = new SwerveMover(e);
			sm.PutInRightLane();
			sm.Mirror();
			mover = sm;
		}else if(moveString.Equals ("Semicircle")){
			mover = new SemicircleMover(e);
		}else if(moveString.Equals ("Semicircle_Mirror")){
			SemicircleMover sm = new SemicircleMover(e);
			sm.Mirror();
			mover = sm;
		}else if(moveString.Equals ("Sidestep")){
			mover = new SidestepMover(e);
		}else if(moveString.Equals ("Sidestep_Mirror")){
			SidestepMover sm = new SidestepMover(e);
			sm.Mirror();
			mover = sm;
		}else{
			Debug.Log ("Invalid mover type!");
			Destroy(e.gameObject);
			return;
		}
		mover.LeftOffset(30f);
		Vector2[] pointset = new Vector2[pointResolution];
		for(int i = 0; i < pointResolution; i++){
			float prog = ((float)i)/((float)(pointResolution-1));
			pointset[i] = mover.PositionFromProgress(prog);
		}
		Destroy (e.gameObject);
		//convert vector set to point-based path
		pe.GeneratePathFromVectorArray(pointset);
	}
	
	Enemy GenerateTestEnemy(){
		string filename = "testenemy";
		GameObject enemyspawn = GameObject.Instantiate (Resources.Load ("Prefabs/MainCanvas/Enemy")) as GameObject;
		Enemy ec = enemyspawn.GetComponent<Enemy>();
		ec.SetTrackID(1);
		ec.SetTrackLane(0);
		return ec;
	}
	
}
