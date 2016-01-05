using UnityEngine;
using System.Collections;

public class EventComponent : MonoBehaviour {

	// Use this for initialization
	void Start () {
		EventManager.Instance().ClearAllEvents(); //events will not persist between scenes
	}
	
	// Update is called once per frame
	void Update () {
		EventManager.Instance ().Update ();
	}
}
