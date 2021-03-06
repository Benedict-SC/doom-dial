using UnityEngine;
using UnityEngine.UI;

public class UIRectTranslator : MonoBehaviour{

	//public static Canvas canvas = null;
	
	public void Start(){
		//canvas = GameObject.Find ("Canvas").GetComponent<Canvas>();
	}
	public void Update(){
		
	}
	
	public Vector3 WorldTopLeftCorner(RectTransform rt){
		Vector3[] corners = new Vector3[4];
		rt.GetWorldCorners(corners);

		Vector3 farNW = new Vector3(-2000,2000,0);
		Vector3 topLeftiest = corners[0];
		for(int i=1;i<4;i++){
			Vector3 corner = corners[i];
			Vector3 dist = corner - farNW;
			Vector3 oldDist = topLeftiest - farNW;
			if(dist.magnitude < oldDist.magnitude){
				topLeftiest = corner;
			}
		}
		return topLeftiest;
		
		/*int rot = (int)rt.eulerAngles.z;
		//Vector3 eulerSave = rt.eulerAngles;
		if(rot > 88 && rot < 92){
			rot = 270;
		}else if(rot > 178 && rot < 182){
			rot = 180;
		}else if(rot > 268 && rot < 272){
			rot = 90;
		}else if(rot > 358 || rot < 2){
			rot = 0;
		}
		//rot is now clockwise and right-angled
		//rt.eulerAngles = new Vector3(0f,0f,0f);
		Vector3 result = new Vector3(0f,0f,0f);
		if(rot == 0){
			result = corners[1];
			//result = rt.TransformPoint(new Vector3(rt.rect.xMin,rt.rect.yMin,rt.position.z));
		}else if(rot == 90){
			result = corners[0];//rt.TransformPoint(new Vector3(rt.rect.xMin,rt.rect.yMax,rt.position.z));
		}else if(rot == 180){
			result = corners[3];//rt.TransformPoint(new Vector3(rt.rect.xMax,rt.rect.yMax,rt.position.z));
		}else if(rot == 270){
			result = corners[2];//rt.TransformPoint(new Vector3(rt.rect.xMax,rt.rect.yMin,rt.position.z));
		}
		//rt.eulerAngles = eulerSave;*/
		//return result;
	}
	public Vector3 WorldBottomRightCorner(RectTransform rt){
		Vector3[] corners = new Vector3[4];
		rt.GetWorldCorners(corners);

		Vector3 farSE = new Vector3(2000,-2000,0);
		Vector3 bottomRightiest = corners[0];
		for(int i=1;i<4;i++){
			Vector3 corner = corners[i];
			Vector3 dist = corner - farSE;
			Vector3 oldDist = bottomRightiest - farSE;
			if(dist.magnitude < oldDist.magnitude){
				bottomRightiest = corner;
			}
		}
		return bottomRightiest;

		/*int rot = (int)rt.eulerAngles.z;
		//Vector3 eulerSave = rt.eulerAngles;
		if(rot > 88 && rot < 92){
			rot = 270;
		}else if(rot > 178 && rot < 182){
			rot = 180;
		}else if(rot > 268 && rot < 272){
			rot = 90;
		}else if(rot > 358 || rot < 2){
			rot = 0;
		}
		//rot is now clockwise and right-angled
		//rt.eulerAngles = new Vector3(0f,0f,0f);
		Vector3 result = new Vector3(0f,0f,0f);
		if(rot == 0){
			result = corners[3];//t.TransformPoint(new Vector3(rt.rect.xMin,rt.rect.yMin,rt.position.z));
		}else if(rot == 90){
			result = corners[2];//rt.TransformPoint(new Vector3(rt.rect.xMin,rt.rect.yMax,rt.position.z));
		}else if(rot == 180){
			result = corners[1];//rt.TransformPoint(new Vector3(rt.rect.xMax,rt.rect.yMax,rt.position.z));
		}else if(rot == 270){
			result = corners[0];//rt.TransformPoint(new Vector3(rt.rect.xMax,rt.rect.yMin,rt.position.z));
		}
		//rt.eulerAngles = eulerSave;
		return result;*/
	}
	public Vector3 WorldSize(RectTransform rt){
	
		int rot = (int)rt.eulerAngles.z;
		Vector3 eulerSave = rt.eulerAngles;
		if(rot > 88 && rot < 92){
			rot = 270;
		}else if(rot > 178 && rot < 182){
			rot = 180;
		}else if(rot > 268 && rot < 272){
			rot = 90;
		}else if(rot > 358 || rot < 2){
			rot = 0;
		}
		//rot is now clockwise and right-angled
		rt.eulerAngles = new Vector3(0f,0f,0f);
		Vector3 result = new Vector3(0f,0f,0f);
		if(rot == 0 || rot == 180){
			result = rt.TransformVector(rt.rect.size);
		}else if(rot == 90 || rot == 270){
			result = rt.TransformVector(new Vector3(rt.rect.size.y,rt.rect.size.x,0f));
		}
		if(result.x < 0){
			result = new Vector3(result.x*-1,result.y,0f);
		}
		if(result.y < 0){
			result = new Vector3(result.x,result.y*-1,0f);
		}
		rt.eulerAngles = eulerSave;
		return result;
	}
	
}