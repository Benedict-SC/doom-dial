using UnityEngine;
using UnityEngine.UI;

public class Seeker : MonoBehaviour{

	public RectTransform target = null;
	RectTransform rt;
	
	float speed = 1.8f;
	Vector3 rot = new Vector3(0,0,2f);

	public void Start(){
		if(Random.value < .5f){
			rot *= -1;
		}
		if(target == null)
			target = (RectTransform)transform;
		rt = (RectTransform)transform;
	}

	public void Update(){
		transform.localEulerAngles += rot;
	
		Vector2 dir = target.anchoredPosition - rt.anchoredPosition;
		if(dir.magnitude < 1f){
			//Debug.Log ("dying");
			Destroy(gameObject);
		}else{
			dir.Normalize();
			dir *= speed;
			rt.anchoredPosition += dir;
		}
	}
}