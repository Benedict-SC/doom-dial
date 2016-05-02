/*??? Not Duncan*/

using UnityEngine;

public class RotationLock : MonoBehaviour{

	public float lockedRotation = 0f;

	public void Start(){}
	public void Update(){
		transform.eulerAngles = new Vector3(transform.eulerAngles.x,transform.eulerAngles.y,lockedRotation);
	}
}
