using UnityEngine;
using UnityEngine.UI;

public class ScrollArrowController : MonoBehaviour{

	public GameObject leftArrow;
	public GameObject rightArrow;
	ScrollRect sr;
	
	public void Start(){
		sr = gameObject.GetComponent<ScrollRect>();
	}
	public void Update(){
		if(sr.horizontalNormalizedPosition <= 0.01){
			leftArrow.SetActive(false);
		}else{
			leftArrow.SetActive(true);
		}
		if(sr.horizontalNormalizedPosition >= 0.99){
			rightArrow.SetActive(false);
		}else{
			rightArrow.SetActive(true);
		}
	}

}
