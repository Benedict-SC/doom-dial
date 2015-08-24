using UnityEngine;
using UnityEngine.UI;
using MiniJSON;
using System.IO;
using System.Collections.Generic;

public class PieceTemplateController : MonoBehaviour{
	
	float maxHeight = 1.7f;
	float maxWidth = 2.2f;
	
	int[,] codes;
	
	public void Start(){
	
	}
	public void Update(){
	
	}
	public void ConfigureFromJSON(string pfn){
		FileLoader fl = new FileLoader ("JSONData" + Path.DirectorySeparatorChar + "Pieces",pfn);
		string json = fl.Read ();
		Dictionary<string,System.Object> data = (Dictionary<string,System.Object>)Json.Deserialize (json);
		
		string imgfilename = (string)data["imgfile"];
		Debug.Log (imgfilename);
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
		if(rt.rect.size.x > rt.rect.size.y){
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
}
