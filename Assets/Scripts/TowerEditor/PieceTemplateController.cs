using UnityEngine;
using UnityEngine.UI;
using MiniJSON;
using System.IO;
using System.Collections.Generic;

public class PieceTemplateController : MonoBehaviour,EventHandler{
	
	float maxHeight = 1.7f;
	float maxWidth = 2.2f;
	string filename = "";
	
	int count = 0;
	
	int[,] codes;

	Timer t;
	bool held = false;
	
	public void Start(){
		EventManager em = EventManager.Instance ();
		//em.RegisterForEventType ("tap", this);
		em.RegisterForEventType ("mouse_click", this);
		em.RegisterForEventType ("mouse_release", this);
		t = new Timer();
	}
	public void Update(){
		if(held){
			if(!TouchIsOnMe(InputWatcher.GetInputPosition())){
				held = false;
				return;
			}
			if(t.TimeElapsedSecs() >= 0.2f){
				held = false;
				GameEvent tapped = new GameEvent("template_tapped");
				tapped.addArgument(this);
				tapped.addArgument(InputWatcher.GetInputPosition());
				EventManager.Instance().RaiseEvent(tapped);	
			}
		}
	}
	public void SetCount(int c){
		count = c;
	}
	public int GetCount(){
		return count;
	}
	public void HandleEvent(GameEvent ge){
		Vector3 pos = (Vector3)ge.args[0];
		if(ge.type.Equals("mouse_click")){
			if(TouchIsOnMe(pos)){
				t.Restart();
				held = true;
			}
		}else if(ge.type.Equals("mouse_release")){
			if(TouchIsOnMe(pos)){
				held = false;			
			}
		}else if(ge.type.Equals("tap")){
			if(TouchIsOnMe(pos)){
				GameEvent tapped = new GameEvent("template_tapped");
				tapped.addArgument(this);
				tapped.addArgument(pos);
				EventManager.Instance().RaiseEvent(tapped);				
			}
		}
	}
	public void ConfigureFromJSON(string pfn){
		filename = pfn;
		FileLoader fl = new FileLoader ("JSONData" + Path.DirectorySeparatorChar + "Pieces",pfn);
		string json = fl.Read ();
		Dictionary<string,System.Object> data = (Dictionary<string,System.Object>)Json.Deserialize (json);
		
		string imgfilename = (string)data["imgfile"];
		//Debug.Log (imgfilename);
		Image img = transform.gameObject.GetComponent<Image> ();
		Texture2D decal = Resources.Load<Texture2D> ("Sprites/PieceSprites/" + imgfilename);
		if (decal == null)
			Debug.Log("decal is null");
		img.sprite = UnityEngine.Sprite.Create (
			decal,
			new Rect(0,0,decal.width,decal.height),
			new Vector2(0.5f,0.5f),
			100f);
			
		int width = (int)(long)data["width"];
		int height = (int)(long)data["height"];
		codes = new int[height,width];
		
		List<System.Object> rows = data["blockMap"] as List<System.Object>;
		for(int i = 0; i < rows.Count; i++){
			List<System.Object> values = rows[i] as List<System.Object>;
			for(int j = 0; j < values.Count; j++){
				int number = (int)(long)values[j];
				codes[i,j] = number;
			}
		}
		
		RectTransform rt = (RectTransform)transform;
		//float scale;
		float squareWidth;
		if(width > height){
			squareWidth = maxWidth / width;
		}else{
			squareWidth = maxHeight / height;
		}
		/*if(rt.rect.size.x > rt.rect.size.y){
			float w = rt.TransformVector(rt.rect.size).x;
			scale = maxWidth/w;
		}else{
			Debug.Log ("height time");
			float h = rt.TransformVector(rt.rect.size).y;
			scale = maxHeight/h;
		}*/
		//float squareWidth = rt.TransformVector(rt.rect.size).x / (float)width * scale;
		Vector3 sizeStuff = new Vector3(width*squareWidth,height*squareWidth,transform.position.z);
		sizeStuff = rt.InverseTransformVector(sizeStuff);
		rt.sizeDelta = sizeStuff;
	}
	public bool TouchIsOnMe(Vector3 touchpos){
		PieceController floatcheck = EditorController.GetFloatingPiece();
		if(floatcheck != this && floatcheck != null){
			if(floatcheck.TouchIsOnMe(touchpos))
				return false;
		}	
		RectTransform rt = (RectTransform)transform;
		UIRectTranslator translate = transform.gameObject.GetComponent<UIRectTranslator>();
		
		bool rectangleOverlap = rt.rect.Contains(rt.InverseTransformPoint(new Vector2(touchpos.x,touchpos.y)));//sr.bounds.IntersectRay(new Ray(touchpos,transform.forward));
		
		if(!rectangleOverlap)
			return false;
		//find local x and y of touch
		
		Vector3 worldSize = translate.WorldSize(rt);	
		//Debug.Log("Rsize: " + rt.rect.size.x + "," + rt.rect.size.y);
		//Debug.Log("worldsize: " + worldSize.x + "," + worldSize.y);
		float squareWidth = worldSize.y / (float)codes.GetLength(0);
		
		Vector3[] corners = new Vector3[4];
		rt.GetWorldCorners(corners);
		Vector3 rCorner = corners[1];
		
		Vector3 relativePos = 	new Vector3(touchpos.x - rCorner.x, 
		                                   rCorner.y - touchpos.y,
		                                   transform.position.z);
		
		if(squareWidth <= 0){
			Debug.Log ("BAD BAD TIMES");
			return false;
		}
		int xcount = -1;
		int ycount = -1;
		while(relativePos.x >= 0){
			relativePos = new Vector3(relativePos.x - squareWidth,relativePos.y,relativePos.z);
			xcount++;
		}
		while(relativePos.y >= 0){
			relativePos = new Vector3(relativePos.x,relativePos.y - squareWidth,relativePos.z);
			ycount++;
		}
		int code = codes[ycount,xcount];
		bool result = TouchHelper (relativePos,squareWidth,code);
		
		return result;
		
	}
	private bool TouchHelper(Vector3 relativePos, float squareWidth, int code){
		if(code == 0)
			return false;
		if(code == 1)
			return true;
		Vector3 internalPos = new Vector3(relativePos.x + squareWidth, relativePos.y + squareWidth, relativePos.z);
		if(code == PieceController.NORTHWEST_CODE){
			return (internalPos.x + internalPos.y < squareWidth);
		}
		if(code == PieceController.NORTHEAST_CODE){
			return (internalPos.x + (squareWidth - internalPos.y) > squareWidth);
		}
		if(code == PieceController.SOUTHEAST_CODE){
			return (internalPos.x + internalPos.y > squareWidth);
		}
		if(code == PieceController.SOUTHWEST_CODE){
			return (internalPos.x + (squareWidth - internalPos.y) < squareWidth);
		}
		Debug.Log ("the shape code is wrong");
		return false;
	}
	public string GetFilename(){
		return filename;
	}
}
