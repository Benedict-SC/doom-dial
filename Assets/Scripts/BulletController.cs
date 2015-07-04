using UnityEngine;
using System.Collections;
using System;

public class BulletController : MonoBehaviour {
	//For bullet types (as opposed to traps and shield types)

	/* The following are stats/skills for bullets
	 * They are passed to the bullet object from the customized tower
	 */
	public float dmg; //damage dealt out
	public float speed; //speed of the bullet
	public float range; //range -- expressed in percent of the length of the lane
	public float knockback; //knockback
	public float lifeDrain; //lifedrain on enemy
	public float poison; //poison damage on enemy
	public float splash; //radius of splash damage
	public float stun; //amount (time?) of enemy stun
	public float slowdown; //enemy slowdown
	public float penetration; //ignores this amount of enemy shield
	public float shieldShred; //lowers enemy shield's max value by this

	public int spread; //number of shots fired at once, default should be 1

	public bool doesSplit; //whether it splits in 2 at the end of its path/collision
	public bool isHoming; //whether it homes in on nearest enemy
	public bool doesArc; //whether it arcs (travels over enemies until it hits the ground at max range)
	/*
	 * End of attributes passed from tower
	 */

	private bool isActive; //for use by arcing projectiles

	public float vx;
	public float vy;
	public float spawnx;
	public float spawny;

	// Use this for initialization
	void Start () {
		dmg = 34; //test value

		SpriteRenderer sr = transform.gameObject.GetComponent<SpriteRenderer> ();
		float radius = sr.bounds.size.x / 2;
		CircleCollider2D collider = transform.gameObject.GetComponent<CircleCollider2D> ();
		collider.radius = radius;
		//Debug.Log ("bullet radius is: " + radius);
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
			return;
		}

		//after moving, check collision with enemies

	}
	//called when the bullet hits something, from the OnCollisionEnter in EnemyController
	public void Collide(){
		Destroy (this.gameObject);
	}

	public bool CheckActive(){
		return true;
	}

}
