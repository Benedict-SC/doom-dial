using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using MiniJSON;

public class EnemyController : MonoBehaviour,EventHandler {

	public readonly float DIAL_RADIUS = 1.5f; //hard coded to avoid constantly querying dial
	//if dial size ever needs to change, replace references to this with calls to a getter

	long spawntime = 0;
	bool warnedFor = false;
	int trackID = 0;
	int trackLane = 0;

	float maxhp = 100.0f;
	float hp = 100.0f;
	string srcFileName;

	//float ySpeed;
	//float xSpeed;

	Timer timer = new Timer();
	EnemyMover mover;
	bool moving = false;
	float progress = 0.0f;
	float progressModifier = 1.0f;


	float impactTime; //"speed"
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
		//timer = new Timer ();
		mover = new SineMover (this);
		//Debug.Log ("enemy radius is " + radius);

	}
	public void StartMoving(){
		//ConfigureEnemy (); //moved to Wave.cs during wave creation

		//some scaling- could maybe be done through transform.scale, but I don't trust Unity to handle the collider
		SpriteRenderer sr = transform.gameObject.GetComponent<SpriteRenderer> ();
		float scalefactor = (radius * 2) / sr.bounds.size.x;
		transform.localScale = new Vector3 (scalefactor, scalefactor, 1);
		CircleCollider2D collider = transform.gameObject.GetComponent<CircleCollider2D> ();
		collider.radius = radius;

		timer.Restart ();
		moving = true;

		//float angle = Mathf.Atan2(transform.position.y , transform.position.x);
		//ySpeed = Mathf.Sin (angle) * speed;
		//xSpeed = Mathf.Cos (angle) * speed;
	}
	public void ConfigureEnemy(){
		FileLoader fl = new FileLoader ("JSONData" + Path.DirectorySeparatorChar + "Bestiary",srcFileName);
		string json = fl.Read ();
		//Debug.Log (json.Length);
		Dictionary<string,System.Object> data = (Dictionary<string,System.Object>)Json.Deserialize (json);
		maxhp = (float)(double)data ["maxHP"];
		hp = (float)(double)data ["HP"];
		impactDamage = (float)(double)data ["damage"];
		impactTime = (float)(double)data ["impactTime"];
		radius = (float)(double)data ["size"];
		maxShields = (float)(double)data ["maxShields"];
		shields = (float)(double)data ["shields"];
	}
	
	// Update is called once per frame
	void Update () {
		if (!moving)
			return;
		//make progress
		float secsPassed = timer.TimeElapsedSecs ();
		timer.Restart ();
		float progressIncrement = secsPassed / impactTime;
		progressIncrement *= progressModifier;
		progress += progressIncrement;

		//transform.position = new Vector3 (transform.position.x - xSpeed, transform.position.y - ySpeed, transform.position.z);
		Vector2 point = mover.PositionFromProgress(progress);
		transform.position = new Vector3 (point.x, point.y, transform.position.z);
		float distanceFromCenter = Mathf.Sqrt ((transform.position.x) * (transform.position.x) + (transform.position.y) * (transform.position.y));
		if ( distanceFromCenter < DIAL_RADIUS ) {
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
		//Debug.Log ("a collision happened!");
		if (coll.gameObject.tag == "Bullet") //if it's a bullet
		{
			BulletController bc = coll.gameObject.GetComponent<BulletController> ();
			if (bc != null) {
				if (bc.CheckActive()) //if we get a Yes, this bullet/trap/shield is active
				{
					StartCoroutine (StatusEffectsBullet (bc));
					hp -= bc.dmg;
					bc.Collide();
					if(hp <= 0){
						Die ();
					}
				}
			}
		}
		else if (coll.gameObject.tag == "Trap") //if it's a trap
		{
			TrapController tc = coll.gameObject.GetComponent<TrapController> ();
			if (tc != null) {
				if (tc.CheckActive()) //if we get a Yes, this bullet/trap/shield is active
				{
					StartCoroutine (StatusEffectsTrap (tc));
					hp -= tc.dmg;
					tc.Collide();
					if(hp <= 0){
						Die ();
					}
				}
			}
		}
		else if (coll.gameObject.tag == "Shield") //if it's a shield
		{
			//shield actions are handled in DialController
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
	public void SetTrackID(int id){
		trackID = id;
	}
	public int GetTrackID(){
		return trackID;
	}
	public void SetTrackLane(int lane){
		trackLane = lane;
	}
	public int GetTrackLane(){
		return trackLane;
	}
	public float GetDamage(){
		return impactDamage;
	}
	public bool HasWarned(){
		return warnedFor;
	}
	public void Warn(){
		warnedFor = true;
	}
	public float GetImpactTime(){
		return impactTime;
	}

	/*Coroutines for Status Effects*/
	IEnumerator StatusEffectsBullet(BulletController bc)
	{
		float lifeDrain = bc.lifeDrain; //lifedrain on enemy
		float poison = bc.poison; //poison damage on enemy
		float knockback = bc.knockback; //knockback
		float stun = bc.stun; //amount (time?) of enemy stun
		float slowdown = bc.slowdown; //enemy slowdown

		//Life Drain - immediate
		if (lifeDrain != 0)
		{
			//IMPLEMENT
		}

		//Poison - begins immediately, continues This coroutine w/o waiting to end
		if (poison != 0)
		{
			//IMPLEMENT
		}

		//Knockback - priority 0
		if (knockback != 0)
		{
			//IMPLEMENT
		}

		//Stun - priority 1
		if (stun != 0)
		{
			//IMPLEMENT
		}

		//Slowdown - priority 2
		if (slowdown != 0)
		{
			//IMPLEMENT
		}

		yield break;
	}

	IEnumerator StatusEffectsTrap(TrapController tc)
	{
		float lifeDrain = tc.lifeDrain; //lifedrain on enemy
		float poison = tc.poison; //poison damage on enemy
		float knockback = tc.knockback; //knockback
		float stun = tc.stun; //amount (time?) of enemy stun
		float slowdown = tc.slowdown; //enemy slowdown

		yield break;
	}

	//modifies enemy speed for a given duration -- use for stun, and slowdown
	IEnumerator ChangeSpeed(float duration, float value)
	{
		progressModifier = value;
		yield return new WaitForSeconds(duration);
		progressModifier = 1.0f;
	}
}
