using UnityEngine;
using System.Collections;

public class ZoneConeController : MonoBehaviour {

	public bool isLethal; //if true, it kills normal enemies
						  //and does big damage to bosses
	public ArrayList enemiesInside;

	// Use this for initialization
	void Start () {
		isLethal = false;
		enemiesInside = new ArrayList();
	}
	
	// Update is called once per frame
	void Update () {
		if (isLethal)
		{
			Debug.Log ("this zone is lethal!");
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
