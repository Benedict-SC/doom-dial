using UnityEngine;

public class GridController : MonoBehaviour{

	public static readonly int GRID_SIZE = 7;
	
	public static readonly int NORTHWEST_CODE = 2;
	public static readonly int NORTHEAST_CODE = 3;
	public static readonly int SOUTHEAST_CODE = 4;
	public static readonly int SOUTHWEST_CODE = 5;

	private class Occupancy{
		public PieceController north = null;
		public PieceController east = null;
		public PieceController south = null;
		public PieceController west = null;
		
		public bool IsEmpty(){
			return north==null && east==null && south==null && west==null;
		}
		public bool IsFull(){
			return north!=null && east!=null && south!=null && west!=null;
		}
	}
	
	Occupancy[,] grid = new Occupancy[GRID_SIZE,GRID_SIZE];
	SpriteRenderer[,] overlays = new SpriteRenderer[GRID_SIZE,GRID_SIZE];
	
	int xcounter = -1;
	int ycounter = -1;
	
	float squareWidth;
	
	Sprite blank;
	Sprite full;
	Sprite nw;
	Sprite ne;
	Sprite se;
	Sprite sw;
	
	public void Start(){
		SpriteRenderer gridSprite = transform.gameObject.GetComponent<SpriteRenderer>();
		squareWidth = gridSprite.bounds.size.x / (float)GRID_SIZE;
		for(int i = 0; i < GRID_SIZE; i++){
			for(int j = 0; j < GRID_SIZE; j++){
				grid[i,j] = new Occupancy();
				GameObject go = Instantiate (Resources.Load ("Prefabs/ValidOverlay")) as GameObject;
				go.transform.position = new Vector3(gridSprite.bounds.min.x + (squareWidth/2) + j*squareWidth,
				                           gridSprite.bounds.min.y + (squareWidth/2) + i*squareWidth,
				                           go.transform.position.z);
				overlays[i,j] = go.GetComponent<SpriteRenderer>();
			}
		}
		
		string blankfile = "GridEmptyOverlay";
		Texture2D tBlank = Resources.Load<Texture2D> ("Sprites/" + blankfile);
		blank = UnityEngine.Sprite.Create (
			tBlank,
			new Rect(0,0,tBlank.width,tBlank.height),
			new Vector2(0.5f,0.5f),
			80f);
		string fullfile = "GridSquareOverlay";
		Texture2D tFull = Resources.Load<Texture2D> ("Sprites/" + fullfile);
		full = UnityEngine.Sprite.Create (
			tFull,
			new Rect(0,0,tFull.width,tFull.height),
			new Vector2(0.5f,0.5f),
			80f);
		string nwfile = "GridNWOverlay";
		Texture2D tNW = Resources.Load<Texture2D> ("Sprites/" + nwfile);
		nw = UnityEngine.Sprite.Create (
			tNW,
			new Rect(0,0,tNW.width,tNW.height),
			new Vector2(0.5f,0.5f),
			80f);
		string nefile = "GridNEOverlay";
		Texture2D tNE = Resources.Load<Texture2D> ("Sprites/" + nefile);
		ne = UnityEngine.Sprite.Create (
			tNE,
			new Rect(0,0,tNE.width,tNE.height),
			new Vector2(0.5f,0.5f),
			80f);
		string sefile = "GridSEOverlay";
		Texture2D tSE = Resources.Load<Texture2D> ("Sprites/" + sefile);
		se = UnityEngine.Sprite.Create (
			tSE,
			new Rect(0,0,tSE.width,tSE.height),
			new Vector2(0.5f,0.5f),
			80f);
		string swfile = "GridSWOverlay";
		Texture2D tSW = Resources.Load<Texture2D> ("Sprites/" + swfile);
		sw = UnityEngine.Sprite.Create (
			tSW,
			new Rect(0,0,tSW.width,tSW.height),
			new Vector2(0.5f,0.5f),
			80f);
	}
	public void Update(){
		GameObject piece = GameObject.Find ("Piece"); //this is not how we will get the piece. doing a find every timestep is a bad idea
		PieceController p = piece.GetComponent<PieceController>();
		int[,] pieceValues = p.GetArray(); //do it here so we only have to call this once
		if(PieceFits (piece,pieceValues)){
			//xcounter and ycounter should be the grid coordinates of the piece drop location
			for(int i = 0; i < GRID_SIZE; i++){
				for(int j = 0; j < GRID_SIZE; j++){
					int blockType = PieceBlockHelper(i,j,ycounter,xcounter,pieceValues);
					if(blockType == 0){
						overlays[i,j].sprite = blank;
					}else if(blockType == 1){
						overlays[i,j].sprite = full;
					}else if(blockType == NORTHWEST_CODE){
						overlays[i,j].sprite = nw;
					}else if(blockType == NORTHEAST_CODE){
						overlays[i,j].sprite = ne;
					}else if(blockType == SOUTHEAST_CODE){
						overlays[i,j].sprite = se;
					}else if(blockType == SOUTHWEST_CODE){
						overlays[i,j].sprite = sw;
					}
					overlays[i,j].color = Color.green;
				}
			}
		}else{
			for(int i = 0; i < GRID_SIZE; i++){
				for(int j = 0; j < GRID_SIZE; j++){
					int blockType = PieceBlockHelper(i,j,ycounter,xcounter,pieceValues);
					if(blockType == 0){
						overlays[i,j].sprite = blank;
					}else if(blockType == 1){
						overlays[i,j].sprite = full;
					}else if(blockType == NORTHWEST_CODE){
						overlays[i,j].sprite = nw;
					}else if(blockType == NORTHEAST_CODE){
						overlays[i,j].sprite = ne;
					}else if(blockType == SOUTHEAST_CODE){
						overlays[i,j].sprite = se;
					}else if(blockType == SOUTHWEST_CODE){
						overlays[i,j].sprite = sw;
					}
					overlays[i,j].color = Color.red;
				}
			}
		}
	}
	//for the active piece, finds the what the value of the ith,jth block would be if you dropped it
	public int PieceBlockHelper(int i, int j, int y, int x, int[,] pieceValues){
		//x and y should be the grid coordinates of the piece drop location
		//Debug.Log((Time.frameCount));
		if(i < y || j < x){
			return 0;
		}else if(i >= y + pieceValues.GetLength(0) || j >= x + pieceValues.GetLength(1)){
			return 0;
		}else{
			//Debug.Log("i-y:" + i + "-" + y + " j-x:" + j + "-" + x);
			return pieceValues[pieceValues.GetLength (0)-1-(i-y),j-x];
			//WARNING: I HAVE NO IDEA WHY I HAVE TO MIRROR THE Y COORDINATE HERE. 
			//IT SHOULDN'T HAPPEN AND INDICATES THAT SOMETHING MAY BE CRITICALLY FUCKED UP.
		}
	}
	
	public bool PieceFits(GameObject piece,int[,] pieceValues){
		PieceController p = piece.GetComponent<PieceController>();
		
		//figure out where it's going to snap to on the grid
			//find top left corner of piece sprite as vector3
		Vector3 topLeftCorner = new Vector3(0f,0f,0f);
		SpriteRenderer sprite = piece.GetComponent<SpriteRenderer>();
		if(p.GetRotation() == 0){
			topLeftCorner = sprite.bounds.min;
		}else if(p.GetRotation() == 90){
			topLeftCorner = new Vector3(sprite.bounds.min.x,sprite.bounds.max.y,0f);
		}else if(p.GetRotation() == 180){
			topLeftCorner = sprite.bounds.max;
		}else if(p.GetRotation() == 270){
			topLeftCorner = new Vector3(sprite.bounds.max.x,sprite.bounds.max.y,0f);
		}
		
			//find x and y distance of that corner from top left corner of grid
		SpriteRenderer gridSprite = transform.gameObject.GetComponent<SpriteRenderer>();
		Vector3 gridCorner = gridSprite.bounds.min;
		float xdist = topLeftCorner.x - gridCorner.x;
		float ydist = topLeftCorner.y - gridCorner.y;
			//produce vector3 corresponding to position relative to top left of grid
		Vector3 relativePos = new Vector3(xdist,ydist,topLeftCorner.z);
			//add half grid square length to x and y of vector for rounding purposes
		relativePos = new Vector3(relativePos.x + squareWidth/2, relativePos.y + squareWidth/2, relativePos.z);
			
			//while said vector x isn't negative:
				//subtract grid square width and increment an int counter (starting at -1)
		xcounter = -1;
		while(relativePos.x >= 0){
			relativePos = new Vector3(relativePos.x - squareWidth,relativePos.y,relativePos.z);
			xcounter++;
		}
		ycounter = -1;
		while(relativePos.y >= 0){ //ditto y
			relativePos = new Vector3(relativePos.x,relativePos.y - squareWidth,relativePos.z);
			ycounter++;
		}
		
		//if either dimension is negative, return false
		if(ycounter < 0 || xcounter < 0)
			return false;
		//counters are now the grid coordinates, probably
		
			//make sure its width/height actually fit
		if (xcounter + pieceValues.GetLength(1) > GRID_SIZE)
			return false;
		if (ycounter + pieceValues.GetLength(0) > GRID_SIZE)
			return false;
		//Debug.Log("ycounter is " + ycounter + " and xcounter is " + xcounter);
		//at this point we know it at least fits within the bounds of the grid
		
		//compare each grid square
		for(int i = 0; i < pieceValues.GetLength(0); i++){
			for(int j = 0; j < pieceValues.GetLength(1); j++){
				//Debug.Log("i is " + i + " and j is " + j);
				Occupancy o = grid[ycounter + i,xcounter + j];
				int shapecode = pieceValues[i,j];
				
				//do some simple checks
				if(shapecode == 0 || o.IsEmpty()){
					continue;
				}if(shapecode == 1 && !o.IsEmpty()){
					return false;
				}
				if(shapecode != 0 && o.IsFull()){
					return false;
				}
				
				//do more complicated stuff
				if(shapecode == NORTHWEST_CODE && !(o.north == null && o.west == null)){
					return false;
				}
				if(shapecode == NORTHEAST_CODE && !(o.north == null && o.east == null)){
					return false;
				}
				if(shapecode == SOUTHEAST_CODE && !(o.south == null && o.east == null)){
					return false;
				}
				if(shapecode == SOUTHWEST_CODE && !(o.south == null && o.west == null)){
					return false;
				}
			}
		}
		//we've gone through and there've been no collisions
		return true;
	}
	
	

}