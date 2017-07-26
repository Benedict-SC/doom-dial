using UnityEngine;
using UnityEngine.UI;

//this was before we realized canvas UI had a Button component already
public class GunButton : MonoBehaviour{
	public Gun gun;
	
	Gun heldGun;
	Timer holdTime;
	bool heldOnCool = false;
	GameObject laserHandle;

	bool decalSet = false;
	RectTransform rt;
	float radius;
	GameObject img;
	Image cooldown;
	public void Start(){
		rt = (RectTransform)transform;
		img = transform.Find("Image").gameObject;
		cooldown = transform.Find("CooldownOverlay").gameObject.GetComponent<Image>();
		cooldown.type = Image.Type.Filled;
		cooldown.fillMethod = Image.FillMethod.Radial360;
		cooldown.fillClockwise = false;
		radius = rt.rect.size.x/2;
		SetDecalFromTower(gun);
		RectTransform imageRect = (RectTransform)img.transform;
		imageRect.sizeDelta = new Vector2(50f,50f);
		holdTime = new Timer();
	}
	public void Update(){
		if(!decalSet){
			if(gun.decalSet){
				SetDecalFromTower(gun);
				decalSet = true;
			}
		}
		if (gun == null)
			return;
		if (gun.GetCooldown () > 0) {
			float ratio = gun.GetCooldownRatio();
			cooldown.fillAmount = ratio;
		}else{
			//Debug.Log ("cooldown < 0");
			cooldown.fillAmount = 0f;
			if(heldOnCool){ //if the button started being held down before it was done being cooled down
				heldOnCool = false;
				StartHold();
			}
		}
		if(gun.held){
			gun.mostRecentChargeTime = holdTime.TimeElapsedSecs();
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
	public void StartHold(){
		if(! (gun.gameObject.activeSelf)){
			return;
		}
		if(gun.GetCooldown() <= 0){
			gun.Hold();
			holdTime.Restart();
			heldGun = gun;
			if(gun.GetTowerType() == "Bullet"  && gun.GetContinuousStrength() > 0){
				laserHandle = gun.SpawnLasers();
			}
		}else{
			heldOnCool = true;
		}
	}
	public void EndHold(){
		if(! (gun.gameObject.activeSelf)){
			return;
		}
		gun.Unhold(holdTime.TimeElapsedSecs());
		heldOnCool = false;
		if(laserHandle != null){
			Destroy(laserHandle);
		}
	}
    public void SetGun(Gun g) {
        gun = g;
        SetDecalFromTower(g);
    }
    public void FireAssociatedGun() { //on button up
		heldOnCool = false;
		if(heldGun != null && !(heldGun.GetContinuousStrength() > 0 && heldGun.GetTowerType() == "Bullet")){ //if it's a normal gun
        	heldGun.Fire(holdTime.TimeElapsedSecs());
		}
    }
	public void SetDecalFromTower(Gun gc){
		if(!gc.gameObject.activeSelf){
			Image sr = img.GetComponent<Image>();
			sr.color = new Color(sr.color.r,sr.color.g,sr.color.b,0f);
			return;
		}
		Sprite s = gc.transform.Find("Label").gameObject.GetComponent<Image>().sprite;
		Image decal = img.GetComponent<Image>();
		decal.color = new Color(decal.color.r,decal.color.g,decal.color.b,1f);
		decal.sprite = s;
	}
}
