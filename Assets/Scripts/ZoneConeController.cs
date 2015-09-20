using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ZoneConeController : MonoBehaviour {

	public bool isLethal; //if true, it kills normal enemies
						  //and does big damage to bosses
	public int zoneID; //number of This zonecone's zone
	public ArrayList enemiesInside;
	WaveManager waveMan;

	// Use this for initialization
	void Start () {
		isLethal = false;
		enemiesInside = new ArrayList();
		waveMan = GameObject.Find ("GameManager").GetComponent<WaveManager>();
	}
	
	// Update is called once per frame
	void Update () {
		if (isLethal)
		{
			Debug.Log ("this zone is lethal!");
			Debug.Log ("ZoneCone enemiesOnscreen: " + waveMan.enemies.Count);
			foreach (GameObject enemy in waveMan.enemies)
			{
				if (enemy != null)
				{
					Debug.Log ("trying an enemy");
					EnemyController ec = enemy.GetComponent<EnemyController>();
					if (ec.GetCurrentTrackID() == zoneID) //if it's in This zone
					{
						ec.Die ();
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
