using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Collections.Generic;
using MiniJSON;

public class PieceController : MonoBehaviour, EventHandler{

	public static readonly int NORTHWEST_CODE = 2;
	public static readonly int NORTHEAST_CODE = 3;
	public static readonly int SOUTHEAST_CODE = 4;
	public static readonly int SOUTHWEST_CODE = 5;

	public static readonly bool TWO_FINGER = true;

	string file = "";
			
	int rotation = 0;
	bool flipped;
	int fliprotation;
	//Vector3 boundsmin;
	//Vector3 boundsmax;
	int[,] codes;
	
	bool lockedToGrid = false;
	bool moving = false;
	Vector3 dragPoint = new Vector3(0,0,0);
	
	public static float gridSquareWidth = 1.7f;

	public Dictionary<string,bool> validTypes = new Dictionary<string,bool>();
	
	public void Start(){
		EventManager em = EventManager.Instance ();
		em.RegisterForEventType ("tap", this);
		em.RegisterForEventType ("mouse_click", this);		
		em.RegisterForEventType ("mouse_release", this);
	}
	public void Update(){
		if(moving && !externalMovement){
			Vector3 inputPos = InputWatcher.GetInputPosition();
			transform.position = inputPos - dragPoint;
			transform.position = new Vector3(transform.position.x,transform.position.y,-1.0f);
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
			if(!lockedToGrid && TouchIsOnMe(pos)){
				dragPoint = pos - transform.position;
				moving = true;
			}
		}else if(ge.type.Equals("mouse_release")){
			if(!lockedToGrid){ //TouchIsOnMe(pos) && 
				//check if hovering over left of screen
				if(transform.position.x < 0.0){
					GameEvent dropped = new GameEvent("piece_dropped_on_inventory");
					dropped.addArgument(this);
					EventManager.Instance().RaiseEvent(dropped);
				}
					//if so: do a tap event on it
				moving = false;
			}
		}		
	}
	bool externalMovement = false;
	public void SetTwoFingerMovement(bool mov){
		externalMovement = mov;
	}
	public void LockRotation(){
		
		float rotation = transform.eulerAngles.z;
		rotation = 360f-rotation;
		if(rotation > 360f){
			rotation -= 360f;
		}
		float lockRot = Mathf.Round(rotation /90.0f)*90;
		SetRotation(lockRot);
	}
	public void RotateClockwise(bool clock){
		float clockrotation = transform.eulerAngles.z;//(float)(360-rotation);
		Debug.Log("euler angles are " + clockrotation);
		float dir = 90f;
		if(clock)
			dir *= -1;
		Debug.Log ("dir is " + dir);
		clockrotation += dir;
		Debug.Log ("euler angles should be " + clockrotation);
		SetRotation(360-clockrotation);
		Debug.Log ("final rotation is " + transform.eulerAngles.z);
	}
    public void Flip()
    {
        flipped = !flipped;
		if(rotation == 90 || rotation == 270){ //vertical flip
			transform.localScale = new Vector3(transform.localScale.x,transform.localScale.y*-1,transform.localScale.z);
		}else{ //horizontal flip
			transform.localScale = new Vector3(transform.localScale.x*-1,transform.localScale.y,transform.localScale.z);
		}
		if(transform.localScale.y < 0 && transform.localScale.x < 0){
			transform.localScale = new Vector3(transform.localScale.x*-1,transform.localScale.y*-1,transform.localScale.z);
			SetRotation((float)((rotation + 180)%360));
		}
		fliprotation = rotation;
    }
	public bool TouchIsOnMe(Vector3 touchpos){
		PieceController floatcheck = EditorController.GetFloatingPiece();
		if(EditorController.finger2down)
			return false;
		if(EditorController.IsClearPanelOpen())
			return false;
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
		int[,] contents = GetArray ();
		
		Vector3 worldSize = translate.WorldSize(rt);	
		//Debug.Log("Rsize: " + rt.rect.size.x + "," + rt.rect.size.y);
		//Debug.Log("worldsize: " + worldSize.x + "," + worldSize.y);
		float squareWidth = worldSize.y / (float)contents.GetLength(0);
		
		Vector3[] corners = new Vector3[4];
		rt.GetWorldCorners(corners);
		Vector3 rCorner = translate.WorldTopLeftCorner(rt);
			
		Vector3 relativePos = 	new Vector3(touchpos.x - rCorner.x, 
											rCorner.y - touchpos.y,
											transform.position.z);
		
		//Debug.Log (file + " touchpos: " + touchpos.x + "," + touchpos.y);
		//Debug.Log (file + " rcorner: " + rCorner.x + "," + rCorner.y);
		//Debug.Log (file + " relative: " + relativePos.x + "," + relativePos.y);
		//Debug.Log (squareWidth);
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
		//Debug.Log(file + rotation + " x and y: " + xcount + ", " + ycount);
		Debug.Log("ycount: " + ycount + ", xcount: " + xcount);
		if(xcount < 0){
			xcount = xcount * -1;
		}
		if(ycount < 0){
			ycount = ycount * -1;
		}
		int code = contents[ycount,xcount];
		bool result = TouchHelper (relativePos,squareWidth,code);
			
		return result;
		
	}
	private bool TouchHelper(Vector3 relativePos, float squareWidth, int code){
		if(code == 0)
			return false;
		if(code == 1)
			return true;
		Vector3 internalPos = new Vector3(relativePos.x + squareWidth, relativePos.y + squareWidth, relativePos.z);
		if(code == NORTHWEST_CODE){
			return (internalPos.x + internalPos.y < squareWidth);
		}
		if(code == NORTHEAST_CODE){
			return (internalPos.x + (squareWidth - internalPos.y) > squareWidth);
		}
		if(code == SOUTHEAST_CODE){
			return (internalPos.x + internalPos.y > squareWidth);
		}
		if(code == SOUTHWEST_CODE){
			return (internalPos.x + (squareWidth - internalPos.y) < squareWidth);
		}
		Debug.Log ("the shape code is wrong");
		return false;
	}
	
	public void ConfigureFromJSON(string filename){
		file = filename;
		FileLoader fl = new FileLoader ("JSONData" + Path.DirectorySeparatorChar + "Pieces",filename);
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

		string[] textTypes = {"bulletText","trapText","shieldText","bulletTrapText","bulletShieldText","trapShieldText"};
		foreach(string s in textTypes){
			validTypes[s] = data.ContainsKey(s);
		}
		
		RectTransform rt = (RectTransform)transform;
		//float squareWidth = img.sprite.bounds.size.x / (float)width * 1.7f; //arbitrary multiplier to fit things...?
		Vector3 sizeStuff = new Vector3(width*gridSquareWidth,height*gridSquareWidth,transform.position.z);
		sizeStuff = rt.InverseTransformVector(sizeStuff);
		rt.sizeDelta = sizeStuff;
		//PrintArray(GetArray ());
	}
	public void SetRotation(int rot){
		if(rot == 360)
			rot = 0;
		rotation = rot;
		rot = (360-rot)%360;
		transform.eulerAngles = new Vector3(transform.rotation.eulerAngles.x,transform.rotation.eulerAngles.y,rot);
		/*RectTransform rt = (RectTransform)transform;
		Vector3 eulerSave = rt.eulerAngles;
		rt.eulerAngles = new Vector3(0f,0f,0f);
		boundsmin = rt.TransformPoint(rt.rect.min);
		boundsmax = rt.TransformPoint(rt.rect.max);
		rt.eulerAngles = eulerSave;*/
	}
	public void SetRotation(float fRot){
		int rot = (int)fRot;
		while(rot < 0)
			rot += 360;
		while(rot > 360)
			rot -= 360;
		if(rot > 88 && rot < 92){
			rot = 90;
		}else if(rot > 178 && rot < 182){
			rot = 180;
		}else if(rot > 268 && rot < 272){
			rot = 270;
		}else if(rot > 358 || rot < 2){
			rot = 0;
		}
		rotation = rot;
		rot = (360-rot)%360;
		transform.eulerAngles = new Vector3(transform.rotation.eulerAngles.x,transform.rotation.eulerAngles.y,rot);
		/*RectTransform rt = (RectTransform)transform;
		Vector3 eulerSave = rt.eulerAngles;
		rt.eulerAngles = new Vector3(0f,0f,0f);
		boundsmin = rt.TransformPoint(rt.rect.min);
		boundsmax = rt.TransformPoint(rt.rect.max);
		rt.eulerAngles = eulerSave;*/
	}
	public void SetGridLock(bool glock){
		lockedToGrid = glock;
	}
	public bool GetGridLock(){
		return lockedToGrid;
	}
	public Vector3 GetTopLeftCorner(){
		RectTransform rt = (RectTransform)transform;
		UIRectTranslator translate = transform.gameObject.GetComponent<UIRectTranslator>();
		return translate.WorldTopLeftCorner(rt);
		//return new Vector3(boundsmin.x,boundsmax.y,transform.position.z);
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
	public bool GetFlipped(){
		return flipped;
	}
	public int GetFlipRotation(){
		return fliprotation;
	}
	public string GetFilename(){
		return file;
	}
	
	public int[,] GetArray(){
		int[,] flippedCodes = new int[codes.GetLength(0),codes.GetLength(1)];
		if(!flipped){
			for(int i = 0; i < codes.GetLength(0); i++){
				for(int j = 0; j < codes.GetLength(1); j++){
					flippedCodes[i,j] = codes[i,j];
				}
			}
		}else if(fliprotation == 90 || fliprotation == 270){ //vertical flip
			for(int i = 0; i < codes.GetLength(0); i++){
				for(int j = 0; j < codes.GetLength(1); j++){
					int flipcode = codes[(codes.GetLength(0)-1)-i,j];
					if(flipcode == NORTHWEST_CODE){
						flipcode = SOUTHWEST_CODE;
					}else if(flipcode == SOUTHWEST_CODE){
						flipcode = NORTHWEST_CODE;
					}else if(flipcode == SOUTHEAST_CODE){
						flipcode = NORTHEAST_CODE;
					}else if(flipcode == NORTHEAST_CODE){
						flipcode = SOUTHEAST_CODE;
					}
					flippedCodes[i,j] = flipcode;
				}
			}
		}else{ //horizontal flip
			for(int i = 0; i < codes.GetLength(0); i++){
				for(int j = 0; j < codes.GetLength(1); j++){
					int flipcode = codes[i,(codes.GetLength(1)-1)-j];
					if(flipcode == NORTHWEST_CODE){
						flipcode = NORTHEAST_CODE;
					}else if(flipcode == NORTHEAST_CODE){
						flipcode = NORTHWEST_CODE;
					}else if(flipcode == SOUTHEAST_CODE){
						flipcode = SOUTHWEST_CODE;
					}else if(flipcode == SOUTHWEST_CODE){
						flipcode = SOUTHEAST_CODE;
					}
					flippedCodes[i,j] = flipcode;
				}
			}
		}
		//now handle rotations
		if(rotation == 0){
			int[,] result = new int[codes.GetLength(0),codes.GetLength(1)];
			for(int i = 0; i < codes.GetLength(0); i++){
				for(int j = 0; j < codes.GetLength(1); j++){
					result[i,j] = flippedCodes[i,j];
				}
			}
			Debug.Log("codes length: " + result.GetLength(0) + " high " + result.GetLength(1) + " wide");
			return result;
		}else if(rotation == 90){
			int[,] result = new int[codes.GetLength(1),codes.GetLength(0)];
			for(int i = 0; i < result.GetLength(0); i++){
				for(int j = 0; j < result.GetLength(1); j++){
					int ycoord = codes.GetLength(0)-1-j;
					int xcoord = i;
					//Debug.Log(ycoord + ", " + xcoord);
					int codeAtLocation = flippedCodes[ycoord,xcoord];
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
					int codeAtLocation = flippedCodes[codes.GetLength(0)-1-i,codes.GetLength(1)-1-j];
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
					int codeAtLocation = flippedCodes[j,codes.GetLength(1)-1-i];
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
	public bool IsMoving(){
		return moving;
	}
	public void SetMoving(bool mov){
		moving = mov;
	}
	public void SetFlippedLoadOnly(bool f){
		flipped = f;
	}
	public void SetFlipRotationLoadOnly(int fr){
		fliprotation = fr;
	}
	public void CalibrateFlip(){
		if(!flipped){
			return;
		}
		if(fliprotation == 90 || fliprotation == 270){ //vertical flip
			transform.localScale = new Vector3(transform.localScale.x,transform.localScale.y*-1,transform.localScale.z);
		}else{ //horizontal flip
			transform.localScale = new Vector3(transform.localScale.x*-1,transform.localScale.y,transform.localScale.z);
		}
		if(transform.localScale.y < 0 && transform.localScale.x < 0){
			transform.localScale = new Vector3(transform.localScale.x*-1,transform.localScale.y*-1,transform.localScale.z);
			SetRotation((float)((rotation + 180)%360));
		}
	}
}