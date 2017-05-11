/*Thom*/

using UnityEngine;
using System.Collections;

public class AoE : MonoBehaviour {
	
	float LERPDUR = 400; //number of frames the splash effect lasts
	float lerpTime = 0; //amt of time it takes to expand to max size
	public float scale;
	bool isPaused;
	public string parent;
	public float aoeBulletDamage;
	public Trap aoeTrapCon;
	
	Collider collide;
	float colRad;
	float orRad;
	
	float currentLerpTime = 0f;
	
	Vector3 originalScale;
	
	// Use this for initialization
	void Start () {
		Debug.Log ("started AoEController");
		originalScale = transform.localScale;
		collide = GetComponent<Collider>();
		colRad = gameObject.transform.localScale.x / 2;
		orRad = colRad; //original radius
	}
	
	// Update is called once per frame
	void Update () {
		//keeps track on if the game pauses
		isPaused = Pause.paused;
		if (!isPaused) {
			transform.localScale = Vector3.Lerp (transform.localScale, transform.localScale * scale, lerpTime);
			colRad = Mathf.Lerp (transform.localScale.x + 0.5f, transform.localScale.x * scale, lerpTime);
			//Debug.Log ("colRad: " + colRad);
			if (transform.localScale.x >= originalScale.x * scale) {
				Destroy (gameObject);
			}
			lerpTime += 1f / LERPDUR;
		}
	}
	
	public void ScaleProps(float pcent)
	{
		if (aoeTrapCon != null)
		{
			Trap tc = aoeTrapCon;
			tc.dmg *= pcent;
		}
	}
	
}
