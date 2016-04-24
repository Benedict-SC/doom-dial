using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class MenuScaleEffectCanvas : MonoBehaviour {
	GameObject parent;
	public float rotOffset = 0.0f;
	float scaleFactor = 0.0f;
	float opFactor = 0.0f;
	public float secondaryOp = 0.0f;
	public float rotationSet;
	public float opAdjust = 2.0f;
	Image img;
	Color transparency = new Color(1,1,1,1);
	
	bool hasStarted = false;
	// Use this for initialization
	void Start () {
		img = transform.FindChild("Image").gameObject.GetComponent<Image>();
		parent = transform.parent.gameObject;
        Debug.Log("test123");
        Debug.Log(parent.ToString());
		hasStarted = true;
		RefreshRotOffset();
		
		//rotOffset = -parent.GetComponent<MenuDial>().headAngle;
	}
	public void RefreshRotOffset(){
		if(!hasStarted)
			return;
		RectTransform rt = (RectTransform)transform;
		float angle = Mathf.Atan2(rt.anchoredPosition.y,rt.anchoredPosition.x);// - (Mathf.PI/2);
		rotOffset = angle*Mathf.Rad2Deg - parent.GetComponent<MenuDial>().headAngle; 
		if(rotOffset < 0)
			rotOffset += 360f;
	}
	
	// Update is called once per frame
	void Update () {
		//rotOffset keeps the scale accurate for all icons past the first
		rotationSet = (parent.transform.eulerAngles.z + rotOffset) % 360;
		if(rotationSet >= 180.0f){
			//Shrinks object while it moves up and to the left
			scaleFactor = (rotationSet) / 360.0f;
			opFactor = (rotationSet) / 360.0f;
			secondaryOp = 1.0f - ((rotationSet)/ 360.0f);
			
		}else{
			//Grows object when it hits the halfway point when going left
			scaleFactor = 1.0f - ((rotationSet)/ 360.0f);
			opFactor = 1.0f - ((rotationSet)/ 360.0f);
			secondaryOp = (rotationSet) / 360.0f;
		}
		
		scaleFactor -= 0.3f;
		transform.localScale = new Vector3 (scaleFactor, scaleFactor, 0);
		//Keeps all icons facing down
		transform.localEulerAngles = new Vector3(0,0,(360 - parent.transform.eulerAngles.z));
		transparency = img.color;
		
		transparency.a = opFactor - (secondaryOp *opAdjust);
		img.color = transparency;
	}
}

