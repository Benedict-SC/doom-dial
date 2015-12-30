using UnityEngine;

public class MegaboidFacsimile : MonoBehaviour{

	Vector3 spinVector = new Vector3(0f,0f,12f);
	Timer timer = new Timer();
	float mergeTime = 3f;
	Vector2 dest = new Vector2(0f,0f);
	Vector2 src = new Vector2(0f,0f);

	public void Update(){
		RectTransform rt = (RectTransform)transform;
		transform.eulerAngles += spinVector; //spin
		float prog = timer.TimeElapsedSecs()/mergeTime; //how far are we towards center? in %
		if(prog > 1f)
			prog = 1f;
		rt.anchoredPosition = new Vector2(src.x+(dest.x-src.x)*prog,src.y+(dest.y-src.y)*prog); //move
	}
	public void InitializeMovement(Vector2 center, Timer boidTimer){
		src = ((RectTransform)transform).anchoredPosition;
		dest = center;
		timer = boidTimer;	
	}

}