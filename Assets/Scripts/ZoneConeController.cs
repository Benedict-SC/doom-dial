using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ZoneConeController : MonoBehaviour {

	public bool isLethal; //if true, it kills normal enemies
						  //and does big damage to bosses
	public int zoneID;

	GameObject gameManager;
	WaveManager waveMan;

	// Use this for initialization
	void Start () {
		gameManager = GameObject.Find ("GameManager");
		if (gameManager == null)
		{
			Debug.Log ("zoneconecontroller couldn't find GameManager!");
		}
		isLethal = false;
		waveMan = gameManager.GetComponent<WaveManager>();
	}
	
	// Update is called once per frame
	void Update () {
		if (isLethal)
		{
			Debug.Log ("is lethal!");
			foreach (GameObject enemy in waveMan.enemiesOnscreen)
			{
				Debug.Log ("got to foreach loop");
				if (enemy != null)
				{
					Debug.Log ("found an enemy");
					EnemyController enemyCon = enemy.GetComponent<EnemyController>();
					if (enemyCon == null)
					{
						Debug.Log ("this enemy's enemyController is null?");
					}
					if (enemyCon.GetTrackID () == zoneID)
					{
						enemyCon.Die ();
					}
				}
			}
		}
	}

	public IEnumerator Detonate()
	{
		isLethal = true;
		yield return new WaitForSeconds(0.1f);
		isLethal = false;
	}

	public void TestMethod()
	{
		Debug.Log ("ran ZCC TestMethod");
	}
}
