using UnityEngine;
using UnityEngine.UI;

public class Steering : MonoBehaviour{

	public RectTransform testTarget;
	public Vector2 vel = Vector2.zero;
	public Vector2 acc = Vector2.zero;
	RectTransform rt;
	void Start(){rt = GetComponent<RectTransform>();}
	
	bool matchOrientationToVelocity = true;
	
	public float maxSpeed = 6f;
	public float maxAccel = 0.5f;
	
	void Update(){
		/*temporary- testing */
		Seek (testTarget);
	
	
		vel += acc;
		if(vel.magnitude > maxSpeed){
			vel.Normalize();
			vel *= maxSpeed;
		}
		rt.anchoredPosition += vel;
		if(matchOrientationToVelocity){
			transform.eulerAngles = new Vector3(0f,0f,Mathf.Atan2(vel.y,vel.x)*Mathf.Rad2Deg);
		}
	}
	
	public void Seek(RectTransform target){
		Vector2 linear = target.anchoredPosition - rt.anchoredPosition;
		linear.Normalize();
		linear *= maxAccel;
		acc = linear;
	}
	public void FollowPath(){
	
	}

}
