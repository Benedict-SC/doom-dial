using UnityEngine;
using UnityEngine.UI;

public class GunButton : MonoBehaviour{
	public Gun gun;
	bool decalSet = false;
	RectTransform rt;
	float radius;
	GameObject img;
	public void Start(){
		rt = (RectTransform)transform;
		img = transform.FindChild("Image").gameObject;
		radius = rt.rect.size.x/2;
		SetDecalFromTower(gun);
		RectTransform imageRect = (RectTransform)img.transform;
		imageRect.sizeDelta = new Vector2(50f,50f);
	}
	public void Update(){
		if(!decalSet){
			if(gun.decalSet){
				SetDecalFromTower(gun);
				decalSet = true;
			}
		}
	}
	
	public bool TouchIsOnMe(Vector2 canvasPoint){
		//Debug.Log ("button pos is " + rt.anchoredPosition.x + ", " + rt.anchoredPosition.y);
		//Debug.Log ("touchpos is " + canvasPoint.x + ", " + canvasPoint.y);
		float distance = Mathf.Sqrt(	((rt.anchoredPosition.x-canvasPoint.x)*(rt.anchoredPosition.x-canvasPoint.x))+
										((rt.anchoredPosition.y-canvasPoint.y)*(rt.anchoredPosition.y-canvasPoint.y))	);
		//Debug.Log ("distance is " + distance);
		return distance <= radius;
	}
	public void SetDecalFromTower(Gun gc){
		Sprite s = gc.transform.FindChild("Label").gameObject.GetComponent<Image>().sprite;
		Image sr = img.GetComponent<Image>();
		sr.sprite = s;
	}
}
