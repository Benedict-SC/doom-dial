using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ShieldFragment : MonoBehaviour {

	Image img = null;
	float arc = -1f; //default
	RectTransform rt;
	EnemyShield manager = null;
	
	void Start () {
		rt = (RectTransform)transform;
		img = gameObject.GetComponent<Image>();
		img.type = Image.Type.Filled;
		img.fillMethod = Image.FillMethod.Radial360;
		if(arc == -1f){
			SetArc (.33f);
		}else{
			SetArc(arc);
		}
	}
	public void SetArc(float newarc){
		if(newarc < 0 || newarc > 1){
			return;
			Debug.Log("attempted to set invalid shield size");
		}else{
			arc = newarc;
			if(img != null)
				img.fillAmount = arc;
		}
	}
	public void SetArcDegrees(float degs){
		degs = Rotations.ClipDegrees(degs);
		arc = degs/360f;
		if(img != null)
			img.fillAmount = arc;
	}
	public void SetManager(EnemyShield es){
		transform.SetParent(es.transform,false);
		es.AddFragment(this);
	}
	void Update () {
	
	}
	void OnTriggerEnter2D(Collider2D coll){
		if(!IsShieldableTag(coll)){
			return;
		}
		RectTransform bulletRT = (RectTransform)coll.transform;
		Vector2 bulletVector = rt.anchoredPosition - bulletRT.anchoredPosition;
		float hitAngleRadians = Mathf.Atan2(bulletVector.y,bulletVector.x);
		hitAngleRadians = Rotations.ClipRadians(hitAngleRadians);
		float orientation = Rotations.EulerAnglesToRadiansCounterclockwiseFromXAxis(transform.rotation.eulerAngles.z+180f);//the 180 is because the sprite fill is from the 180 mark for some reason
		float bound = orientation - arc*Rotations.TWO_PI;
		Debug.Log ("arc spans " + (bound*Mathf.Rad2Deg) + " through " + (orientation*Mathf.Rad2Deg));
		Debug.Log ("hit angle is " + (hitAngleRadians*Mathf.Rad2Deg));
		bool hitUs = false;
		if(bound >= 0){
			hitUs = (hitAngleRadians < orientation && hitAngleRadians > bound);
		}else{
			float bigbound = bound + Rotations.TWO_PI;
			hitUs = (hitAngleRadians < orientation || hitAngleRadians > bigbound);
		}
		manager.GetHitBy(coll);
	}
	bool IsShieldableTag(Collider2D coll){
		string colltag = coll.gameObject.tag;
		return colltag == "Bullet" || colltag == "AoE" || colltag == "Trap";
	}
}
