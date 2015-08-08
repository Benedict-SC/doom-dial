using UnityEngine;
using System.Collections;

public class AoEController : MonoBehaviour {

	float LERPDUR = 400; //number of frames the splash effect lasts
	float lerpTime = 0; //amt of time it takes to expand to max size
	public float scale;

	public string parent;
	public BulletController aoeBulletCon;
	public TrapController aoeTrapCon;

	Collider collide;
	float colRad;
	float orRad;

	float currentLerpTime = 0f;

	Vector3 originalScale;

	// Use this for initialization
	void Start () {
	
		//Debug.Log ("started AoEController");
		originalScale = transform.localScale;
		collide = GetComponent<Collider>();
		colRad = gameObject.transform.localScale.x / 2;
		orRad = colRad; //original radius
	}
	
	// Update is called once per frame
	void Update () {
		transform.localScale = Vector3.Lerp (transform.localScale, transform.localScale * scale, lerpTime);
		colRad = Mathf.Lerp (transform.localScale.x + 0.5f, transform.localScale.x * scale, lerpTime);
		//Debug.Log ("colRad: " + colRad);
		if (transform.localScale.x >= originalScale.x * scale)
		{
			Destroy (gameObject);
		}
		lerpTime += 1f / LERPDUR;
	}

	public void ScaleProps(float pcent)
	{
		Debug.Log ("called ScaleProps");
		if (aoeBulletCon != null)
		{
			BulletController bc = aoeBulletCon;
			Debug.Log ("old slowdown: " + bc.slowdown);
			bc.dmg *= pcent;
			bc.poison *= pcent;
			bc.lifeDrain *= pcent;
			bc.slowdown *= pcent;
			bc.shieldShred *= pcent;
			Debug.Log ("new slowdown: " + bc.slowdown);
		}
		else if (aoeTrapCon != null)
		{
			TrapController tc = aoeTrapCon;
			tc.dmg *= pcent;
			tc.poison *= pcent;
			tc.lifeDrain *= pcent;
			tc.slowdown *= pcent;
			tc.shieldShred *= pcent;
		}
	}

}
