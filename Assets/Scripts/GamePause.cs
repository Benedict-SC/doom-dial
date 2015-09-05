using UnityEngine;
using System.Collections;

public class GamePause : MonoBehaviour, EventHandler {
	public bool isPaused = false;
	//accesses the wave manager to pause everything it has access to
	public GameObject WM;
	//darkens the screen when game is paused
	public GameObject tintBox;
	//holds the menu button so it can pop in when paused
	public GameObject returnButton;
	//keeps track of where buttons will go when paused
	public GameObject[] anchorPoints;
	//Keep track of all things needed for pause
	GameObject[] buttons;
	GameObject[] enemies;
	GameObject[] bullets;
	// Use this for initialization
	void Start () {
		EventManager em = EventManager.Instance ();
		em.RegisterForEventType ("mouse_release", this);
		em.RegisterForEventType ("mouse_click", this);
		buttons = GameObject.FindGameObjectsWithTag ("Button");
		tintBox.GetComponent<Renderer> ().material.color = new Color (0.0f, 0.0f, 0.0f, 0.0f);
	}
	public void HandleEvent(GameEvent ge){
		if (ge.type.Equals ("mouse_release")) {
			RaycastHit targetFind;
			
			Ray targetSeek = Camera.main.ScreenPointToRay (InputWatcher.GetTouchPosition ());
			if (Physics.Raycast (targetSeek, out targetFind)) {
				//sees if ray collided with the start button
				if (targetFind.collider.gameObject == this.gameObject) {
					if(!isPaused){
						//moves buttons to set locations, and darkens screen
						this.gameObject.transform.position = anchorPoints[0].gameObject.transform.position;
						returnButton.transform.position = anchorPoints[2].gameObject.transform.position;
						GetComponentInChildren<TextMesh>().text = "Resume";
						tintBox.GetComponent<Renderer> ().material.color = new Color (0.0f, 0.0f, 0.0f, 0.5f);
					}else{
						//moves buttons back, sets screen back to normal color
						this.gameObject.transform.position = anchorPoints[1].gameObject.transform.position;
						returnButton.transform.position = anchorPoints[3].gameObject.transform.position;
						GetComponentInChildren<TextMesh>().text = "Pause";
						tintBox.GetComponent<Renderer> ().material.color = new Color (0.0f, 0.0f, 0.0f, 0.0f);
					}
					//freezes enemy spawning
					WM.GetComponent<WaveManager>().triggerFreeze();

					//Gets all enemies on the field and freezes them
					enemies = GameObject.FindGameObjectsWithTag("Enemy");
					if(enemies.Length > 0){
						foreach (GameObject enemy in enemies) {
							EnemyController e = enemy.GetComponent<EnemyController>();
							e.Freeze();
						}
					}
					//stops all bullets on the field
					bullets = GameObject.FindGameObjectsWithTag("Bullet");
					if(bullets.Length > 0){
						foreach (GameObject bullet in bullets) {
							BulletController b = bullet.GetComponent<BulletController>();
							b.TriggerPause();
						}
					}
					//disables buttons and cooldown
					foreach (GameObject button in buttons){
						ButtonController b = button.GetComponent<ButtonController>();
						b.TriggerPause();
					}
					isPaused = !isPaused;
				}
			}
			
		}
	}
	// Update is called once per frame
	void Update () {
	
	}
}
