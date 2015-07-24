using UnityEngine;
using System.Collections;

public class AoEController : MonoBehaviour {

	float LERPDUR = 400; //number of frames the splash effect lasts
	float lerpTime = 0; //amt of time it takes to expand to max size
	public float scale;

	public string parent;
	public BulletController aoeBulletCon;
	public TrapController aoeTrapCon;

	CircleCollider2D collide;
	float colRad;
	float orRad;

	float currentLerpTime = 0f;

	Vector3 originalScale;

	// Use this for initialization
	void Start () {
	
		originalScale = transform.localScale;
		collide = GetComponent<CircleCollider2D>();
		colRad = collide.radius;
		orRad = colRad; //original radius
	}
	
	// Update is called once per frame
	void Update () {

		transform.localScale = Vector3.Lerp (transform.localScale, transform.localScale * scale, lerpTime);
		colRad = Mathf.Lerp (transform.localScale.x, transform.localScale.x * scale, lerpTime);
		Debug.Log ("colRad: " + colRad);
		if (transform.localScale.x >= originalScale.x * scale)
		{
			Destroy (gameObject);
		}
		lerpTime += 1f / LERPDUR;
	}
	
}
