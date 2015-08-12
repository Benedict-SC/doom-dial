using UnityEngine;
using System.IO;
using System.Collections.Generic;
using MiniJSON;

public class PieceController : MonoBehaviour{

	public static readonly int NORTHWEST_CODE = 2;
	public static readonly int NORTHEAST_CODE = 3;
	public static readonly int SOUTHEAST_CODE = 4;
	public static readonly int SOUTHWEST_CODE = 5;
	
	int rotation = 0;
	int[,] codes;
	
	public void Start(){
		ConfigureFromJSON("slow_normal");
	}
	public void Update(){
	
	}
	
	public void ConfigureFromJSON(string filename){
		FileLoader fl = new FileLoader ("JSONData" + Path.DirectorySeparatorChar + "Pieces",filename);
		string json = fl.Read ();
		Dictionary<string,System.Object> data = (Dictionary<string,System.Object>)Json.Deserialize (json);
		
		string imgfilename = (string)data["imgfile"];
		Debug.Log (imgfilename);
		SpriteRenderer img = transform.gameObject.GetComponent<SpriteRenderer> ();
		Texture2D decal = Resources.Load<Texture2D> ("Sprites/" + imgfilename);
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
		//PrintArray(GetArray ());
	}
	public static void PrintArray(int[,] array){
		string arrayPrint = "[\n";
		for(int i = 0; i < array.GetLength(0); i++){
			arrayPrint += "[";
			for(int j = 0; j < array.GetLength(1); j++){
				arrayPrint += array[i,j] + ",";
			}
			arrayPrint += "]\n";
		}
		arrayPrint += "]";
		Debug.Log(arrayPrint);
	}
	public int GetRotation(){
		return rotation;
	}
	
	public int[,] GetArray(){
		if(rotation == 0){
			int[,] result = new int[codes.GetLength(0),codes.GetLength(1)];
			for(int i = 0; i < codes.GetLength(0); i++){
				for(int j = 0; j < codes.GetLength(1); j++){
					result[i,j] = codes[i,j];
				}
			}
			return result;
		}else if(rotation == 90){
			int[,] result = new int[codes.GetLength(1),codes.GetLength(0)];
			for(int i = 0; i < result.GetLength(0); i++){
				for(int j = 0; j < result.GetLength(1); j++){
					int ycoord = codes.GetLength(1)-1-j;
					int xcoord = i;
					int codeAtLocation = codes[ycoord,xcoord];
					//Debug.Log ("i=" + i + ",j="+j + " is " + ycoord + ", " + xcoord + " (" + codeAtLocation + ")");
					//rotate half blocks
					if(codeAtLocation > 1){
						codeAtLocation++;
					}
					if(codeAtLocation > SOUTHWEST_CODE){
						codeAtLocation -= 4;
					}
					result[i,j] = codeAtLocation;
				}
			}
			return result;
		}else if(rotation == 180){
			int[,] result = new int[codes.GetLength(1),codes.GetLength(0)];
			for(int i = 0; i < result.GetLength(0); i++){
				for(int j = 0; j < result.GetLength(1); j++){
					int codeAtLocation = codes[codes.GetLength(0)-1-i,codes.GetLength(1)-1-j];
					//rotate half blocks
					if(codeAtLocation > 1){
						codeAtLocation += 2;
					}
					if(codeAtLocation > SOUTHWEST_CODE){
						codeAtLocation -= 4;
					}
					result[i,j] = codeAtLocation;
				}
			}
			return result;
		}else if(rotation == 270){
			int[,] result = new int[codes.GetLength(1),codes.GetLength(0)];
			for(int i = 0; i < result.GetLength(0); i++){
				for(int j = 0; j < result.GetLength(1); j++){
					int codeAtLocation = codes[j,codes.GetLength(0)-1-i];
					//rotate half blocks
					if(codeAtLocation > 1){
						codeAtLocation += 3;
					}
					if(codeAtLocation > SOUTHWEST_CODE){
						codeAtLocation -= 4;
					}
					result[i,j] = codeAtLocation;
				}
			}
			return result;
		}
		else return null;
	}

}