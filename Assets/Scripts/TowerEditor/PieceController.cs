using UnityEngine;
using System.IO;
using System.Collections.Generic;
using MiniJSON;

public class PieceController : MonoBehaviour, EventHandler{

	public static readonly int NORTHWEST_CODE = 2;
	public static readonly int NORTHEAST_CODE = 3;
	public static readonly int SOUTHEAST_CODE = 4;
	public static readonly int SOUTHWEST_CODE = 5;
	
	int rotation = 0;
	int[,] codes;
	
	bool lockedToGrid = false;
	bool moving = false;
	Vector3 dragPoint = new Vector3(0,0,0);
	
	public void Start(){
		EventManager em = EventManager.Instance ();
		em.RegisterForEventType ("tap", this);
		em.RegisterForEventType ("mouse_click", this);		
		em.RegisterForEventType ("mouse_release", this);
	}
	public void Update(){
		if(moving){
			transform.position = InputWatcher.GetInputPosition() - dragPoint;
		}
	}
	
	public void HandleEvent(GameEvent ge){
		Vector3 pos = (Vector3)ge.args[0];
		if(ge.type.Equals("tap")){
			if(TouchIsOnMe(pos)){
				GameEvent tapped = new GameEvent("piece_tapped");
				tapped.addArgument(this);
				EventManager.Instance().RaiseEvent(tapped);				
			}
		}else if(ge.type.Equals("mouse_click")){
			if(TouchIsOnMe(pos) && !lockedToGrid){
				dragPoint = pos - transform.position;
				moving = true;
			}
		}else if(ge.type.Equals("mouse_release")){
			moving = false;
		}
	}
	bool TouchIsOnMe(Vector3 touchpos){
		SpriteRenderer sr = transform.gameObject.GetComponent<SpriteRenderer>();
		return sr.bounds.IntersectRay(new Ray(touchpos,transform.forward));
		//will need more complicated logic for concave pieces
	}
	
	public void ConfigureFromJSON(string filename){
		FileLoader fl = new FileLoader ("JSONData" + Path.DirectorySeparatorChar + "Pieces",filename);
		string json = fl.Read ();
		Dictionary<string,System.Object> data = (Dictionary<string,System.Object>)Json.Deserialize (json);
		
		string imgfilename = (string)data["imgfile"];
		Debug.Log (imgfilename);
		SpriteRenderer img = transform.gameObject.GetComponent<SpriteRenderer> ();
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
		//PrintArray(GetArray ());
	}
	public void SetRotation(int rot){
		rotation = rot;
		rot = (360-rot)%360;
		transform.eulerAngles = new Vector3(transform.rotation.eulerAngles.x,transform.rotation.eulerAngles.y,rot);
	}
	public void SetGridLock(bool glock){
		lockedToGrid = glock;
	}
	public bool GetGridLock(){
		return lockedToGrid;
	}
	public Vector3 GetTopLeftCorner(){
		SpriteRenderer sprite = transform.gameObject.GetComponent<SpriteRenderer>();
		return new Vector3(sprite.bounds.min.x,sprite.bounds.max.y,transform.position.z);
		//all this stuff was supposed to compensate for rotation, but it apparently doesn't matter
		/*if(rotation == 0){
			return sprite.bounds.min;
		}else if(rotation == 90){
			return new Vector3(sprite.bounds.min.x,sprite.bounds.max.y,0f);
		}else if(rotation == 180){
			return sprite.bounds.max;
		}else if(rotation == 270){
			return new Vector3(sprite.bounds.max.x,sprite.bounds.max.y,0f);
		}else{
			Debug.Log ("piece has rotation that should be impossible");
			return sprite.bounds.min;
		}*/
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
					int ycoord = codes.GetLength(0)-1-j;
					int xcoord = i;
					//Debug.Log(ycoord + ", " + xcoord);
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
			int[,] result = new int[codes.GetLength(0),codes.GetLength(1)];
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
					int codeAtLocation = codes[j,codes.GetLength(1)-1-i];
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