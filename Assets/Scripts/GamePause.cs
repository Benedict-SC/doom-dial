using UnityEngine;
using System.Collections;

public class GamePause : MonoBehaviour, EventHandler {
	public bool isPaused = false;
	public GameObject WM;
	public GameObject tintBox;
	public GameObject returnButton;
	public GameObject[] anchorPoints;
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
						this.gameObject.transform.position = anchorPoints[0].gameObject.transform.position;
						returnButton.transform.position = anchorPoints[2].gameObject.transform.position;
						GetComponentInChildren<TextMesh>().text = "Resume";
						tintBox.GetComponent<Renderer> ().material.color = new Color (0.0f, 0.0f, 0.0f, 0.5f);
					}else{
						this.gameObject.transform.position = anchorPoints[1].gameObject.transform.position;
						returnButton.transform.position = anchorPoints[3].gameObject.transform.position;
						GetComponentInChildren<TextMesh>().text = "Pause";
						tintBox.GetComponent<Renderer> ().material.color = new Color (0.0f, 0.0f, 0.0f, 0.0f);
					}
					WM.GetComponent<WaveManager>().triggerFreeze();

					enemies = GameObject.FindGameObjectsWithTag("Enemy");
					if(enemies.Length > 0){
						foreach (GameObject enemy in enemies) {
							EnemyController e = enemy.GetComponent<EnemyController>();
							e.Freeze();
						}
					}
					bullets = GameObject.FindGameObjectsWithTag("Bullet");
					if(bullets.Length > 0){
						foreach (GameObject bullet in bullets) {
							BulletController b = bullet.GetComponent<BulletController>();
							b.TriggerPause();
						}
					}
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
