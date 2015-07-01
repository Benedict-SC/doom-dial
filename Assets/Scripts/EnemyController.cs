using UnityEngine;
using System.Collections;

public class EnemyController : MonoBehaviour,EventHandler {

	float hp = 100;
	long spawntime = 0;
	//these fields are just for basic movement, to show that waves are working. actual enemy movement will be more complicated- tear this out.
	float temporaryHardCodedSpeed = 0.02f;
	float tempYSpeed;
	float tempXSpeed;

	// Use this for initialization
	void Start () {
		EventManager.Instance ().RegisterForEventType ("shot_collided", this);
	}
	public void StartMoving(){
		float angle = Mathf.Atan2(transform.position.y , transform.position.x);
		tempYSpeed = Mathf.Sin (angle) * temporaryHardCodedSpeed;
		tempXSpeed = Mathf.Cos (angle) * temporaryHardCodedSpeed;
	}
	
	// Update is called once per frame
	void Update () {
		transform.position = new Vector3 (transform.position.x - tempXSpeed, transform.position.y - tempYSpeed, transform.position.z);
		if (Mathf.Sqrt ((transform.position.x)*(transform.position.x)+(transform.position.y)*(transform.position.y)) < 1.0f) {
			//this whole conditional is just to stop them from going out the other side
			//so it looks better when I show it off to Joe
			//delete it and replace it with actual "what happens when it reaches the dial" logic
			tempXSpeed = 0;
			tempYSpeed = 0;
		}
	}

	public void HandleEvent(GameEvent ge){
		//unpack shot location argument, check for collision, if it collided with you take damage from it
	}
	public long GetSpawnTime(){
		return spawntime;
	}
	public void SetSpawnTime(long time){
		spawntime = time;
	}
}
