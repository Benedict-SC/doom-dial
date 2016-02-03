using UnityEngine;
using UnityEngine.UI;

public class PathLink : MonoBehaviour{
	public GameObject pointA = null;
	public GameObject pointB = null;
	RectTransform rt;
	void Start(){rt = GetComponent<RectTransform>();}
	
	void Update(){
		if(pointA != null && pointB != null){
			Vector2 a = pointA.GetComponent<RectTransform>().anchoredPosition;
			Vector2 b = pointB.GetComponent<RectTransform>().anchoredPosition;
			float angle = Mathf.Atan2 (b.y-a.y,b.x-a.x) + (Mathf.PI/2);
			float dist = (b-a).magnitude;
			rt.anchoredPosition = (a+b)/2f;
			rt.sizeDelta = new Vector2(2f,dist);
			transform.eulerAngles = new Vector3(0f,0f,angle*Mathf.Rad2Deg);
		}
	}
}
