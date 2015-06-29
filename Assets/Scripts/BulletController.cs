using UnityEngine;
using System.Collections;
using System;

public class BulletController : MonoBehaviour {

	public float speed;
	public float range;
	public float vx;
	public float vy;
	public float spawnx;
	public float spawny;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		//position doesn't let you modify individual fields so this is gonna be wordy
		this.transform.position = new Vector3(this.transform.position.x + vx, this.transform.position.y + vy, this.transform.position.z);
		//if bullet exceeds its range, disappear
		float distance = (float)Math.Sqrt ((this.transform.position.x - spawnx) * (this.transform.position.x - spawnx)
						+ (this.transform.position.y - spawny) * (this.transform.position.y - spawny));
		if(distance > range){
			Destroy(this.gameObject);
		}
	}
}
