using UnityEngine;
using UnityEngine.UI;

public class Lifespan : MonoBehaviour{
	bool activated = false;
	float lifespan = 0f; //seconds
	Timer t = null;
	
	Image img;
	bool fades = false;
	
	public void BeginLiving(float lifespanInSeconds){
		lifespan = lifespanInSeconds;
		t = new Timer();
		activated = true;
		img = GetComponent<Image>(); //null if no image
	}
	public void Update(){
		if(activated){
			if(t.TimeElapsedSecs() > lifespan){
				Destroy (gameObject);
			}
		}
		if(fades){
			img.color = new Color(img.color.r,img.color.g,img.color.b,1.0f-PercentLived());
		}
	}
	public float PercentLived(){
		if(activated){
			return t.TimeElapsedSecs()/lifespan;
		}
		return 0f;
	}
	
	public void SetImageToFade(bool fading){
		fades = fading;
	}
}
