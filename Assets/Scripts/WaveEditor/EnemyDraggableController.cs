using UnityEngine;
using UnityEngine.UI;

public class EnemyDraggableController : MonoBehaviour{

	EnemyTemplateController etc = null;

	public void Start(){
		
	}
	public void Update(){
		Vector3 inputPos = InputWatcher.GetInputPosition();
		transform.position = new Vector3(inputPos.x,inputPos.y,-1.0f);
	}
	public void ConfigureFromTemplate(EnemyTemplateController etc){
		this.etc = etc;
		Image img = transform.gameObject.GetComponent<Image> ();
		Texture2D decal = Resources.Load<Texture2D> ("Sprites/EnemySprites/" + etc.GetImgFileName());
		if (decal == null)
			Debug.Log("decal is null");
		img.sprite = UnityEngine.Sprite.Create (
			decal,
			new Rect(0,0,decal.width,decal.height),
			new Vector2(0.5f,0.5f),
			100f);
		img.color = new Color(img.color.r,img.color.g,img.color.b,0.5f);//.a = 0.5f;
		
		//set size stuff
		RectTransform rt = (RectTransform)transform;
		//figure out how much to scale it by to fit
		float xratio = ((float)decal.width)/etc.maxwidth;
		float yratio = ((float)decal.height)/etc.maxheight;
		float highestratio;
		if(xratio >= yratio){
			highestratio = xratio;
		}else{
			highestratio = yratio;
		}
		float newWidth = ((float)decal.width) / highestratio;
		float newHeight = ((float)decal.height) / highestratio;
		
		Vector3 sizeStuff = new Vector3(newWidth,newHeight,transform.position.z);
		rt.sizeDelta = sizeStuff;
	}
	
	public EnemyTemplateController GetEnemyTemplate(){
		return etc;
	}
}