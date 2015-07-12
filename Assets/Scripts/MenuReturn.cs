using UnityEngine;
using System.Collections;

public class MenuReturn : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		if(Input.GetMouseButtonUp(0)){
				RaycastHit targetFind;
				Ray targetSeek = Camera.main.ScreenPointToRay (Input.mousePosition);
				if (Physics.Raycast (targetSeek, out targetFind)) {
					//gets stats of clicked building, triggers the GUI popups
					if (targetFind.collider.gameObject.tag == "Button") {
					Application.LoadLevel ("MenuTest");
					}
	}
}
	}
}