using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using MiniJSON;

public class EnemyController : MonoBehaviour,EventHandler {

	float DIAL_RADIUS = 1.5f; //hard coded to avoid constantly querying dial
	//if dial size ever needs to change, replace references to this with calls to a getter

	float maxhp = 100;
	float hp = 100;
	long spawntime = 0;
	string srcFileName;

	float ySpeed;
	float xSpeed;

	float speed;
	float impactDamage;
	float radius;
	float maxShields;
	float shields;
	//ability?
	//weakness?

	// Use this for initialization
	void Start () {
		EventManager.Instance ().RegisterForEventType ("shot_collided", this);
		SpriteRenderer sr = transform.gameObject.GetComponent<SpriteRenderer> ();
		float rad = sr.bounds.size.x / 2;
		CircleCollider2D collider = transform.gameObject.GetComponent<CircleCollider2D> ();
		collider.radius = rad;
		//Debug.Log ("enemy radius is " + radius);
	}
	public void StartMoving(){
		ConfigureEnemy ();

		//some scaling- could maybe be done through transform.scale, but I don't trust Unity to handle the collider
		SpriteRenderer sr = transform.gameObject.GetComponent<SpriteRenderer> ();
		float scalefactor = (radius * 2) / sr.bounds.size.x;
		transform.localScale = new Vector3 (scalefactor, scalefactor, 1);
		CircleCollider2D collider = transform.gameObject.GetComponent<CircleCollider2D> ();
		collider.radius = radius;

		float angle = Mathf.Atan2(transform.position.y , transform.position.x);
		ySpeed = Mathf.Sin (angle) * speed;
		xSpeed = Mathf.Cos (angle) * speed;
	}
	public void ConfigureEnemy(){
		FileLoader fl = new FileLoader ("JSONData" + Path.DirectorySeparatorChar + "Bestiary",srcFileName);
		string json = fl.Read ();
		Debug.Log (json.Length);
		Dictionary<string,System.Object> data = (Dictionary<string,System.Object>)Json.Deserialize (json);
		maxhp = (float)(double)data ["maxHP"];
		hp = (float)(double)data ["HP"];
		impactDamage = (float)(double)data ["damage"];
		speed = (float)(double)data ["speed"];
		radius = (float)(double)data ["size"];
		maxShields = (float)(double)data ["maxShields"];
		shields = (float)(double)data ["shields"];
	}
	
	// Update is called once per frame
	void Update () {
		transform.position = new Vector3 (transform.position.x - xSpeed, transform.position.y - ySpeed, transform.position.z);
		float distanceFromCenter = Mathf.Sqrt ((transform.position.x) * (transform.position.x) + (transform.position.y) * (transform.position.y));
		if ( distanceFromCenter < DIAL_RADIUS ) {
			xSpeed = 0;
			ySpeed = 0;
			GameEvent ge = new GameEvent("enemy_arrived");
			ge.addArgument(transform.gameObject);
			EventManager.Instance().RaiseEvent(ge);
		}
		GameObject healthCircle = transform.Find ("Health").gameObject;
		healthCircle.transform.localScale = new Vector3 (hp / maxhp, hp / maxhp, 1);
	}

	public void HandleEvent(GameEvent ge){
		//unpack shot location argument, check for collision, if it collided with you take damage from it
		//actually never mind that, Unity has its own collision detection system!
	}
	void OnTriggerEnter2D(Collider2D coll){ //this is said system.
		Debug.Log ("a collision happened!");
		BulletController bc = coll.gameObject.GetComponent<BulletController> ();
		if (bc != null) {
			if (bc.CheckActive()) //if we get a Yes, this bullet/trap/shield is active
			{
				hp -= bc.dmg;
				bc.Collide();
				if(hp <= 0){
					Die ();
				}
			}
		}
	}
	public void Die(){
		//put more dying functionality here
		Destroy (this.gameObject);
	}
	public long GetSpawnTime(){
		return spawntime;
	}
	public void SetSpawnTime(long time){
		spawntime = time;
	}
	public void SetSrcFileName(string filename){
		srcFileName = filename;
	}
	public float GetDamage(){
		return impactDamage;
	}
}
