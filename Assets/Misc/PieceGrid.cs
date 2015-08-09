public class PieceGrid{

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
		
		public void IsEmpty(){
			return north==null && east==null && south==null && west==null;
		}
		public void IsFull(){
			return north!=null && east!=null && south!=null && west!=null;
		}
	}
	
	Occupancy[][] grid = new Occupancy[GRID_SIZE][GRID_SIZE];
	
	//idk if this will use Start() or a constructor
	start(){
		for(int i = 0; i < GRID_SIZE; i++){
			for(int j = 0; j < GRID_SIZE; j++){
				grid[i][j] = new Occupancy();
			}
		}
	}
	
	public boolean PieceFits(GameObject piece){
		PieceController p = piece.GetComponent<Piece>();
		
		//figure out where it's going to snap to on the grid
			//find top left corner of piece sprite as vector3
		Vector3 topLeftCorner = piece.GetComponent<SpriteRenderer>.bounds.min; //this doesn't account for rotation- fix it
			//find x and y distance of that corner from top left corner of grid
		float xdist; //ACTUALLY GET
		float ydist; //ACTUALLY GET
			//produce vector3 corresponding to position relative to top left of grid
		Vector3 relativePos = new Vector3(topLeftCorner.x-xdist,topLeftCorner.y-ydist,topLeftCorner.z);
			//add half grid square length to x and y of vector for rounding purposes
		float squareWidth; //ACTUALLY GET
		relativePos = new Vector3(relativePos.x + squareWidth/2, relativePos.y + squareWidth/2, relativePos.z);
			//if either dimension is negative, return false
		if(relativePos.x < 0.0f || relativePos.y < 0.0f)
			return false;
			//while said vector x isn't negative:
				//subtract grid square width and increment an int counter (starting at -1)
		int xcounter = -1;
		while(relativePos.x >= 0){
			relativePos = new Vector3(relativePos.x - squareWidth,relativePos.y,relativePos.z);
		}
		int ycounter = -1;
		while(relativePos.y >= 0){ //ditto y
			relativePos = new Vector3(relativePos.x,relativePos.y - squareWidth,relativePos.z);
		}
		//counters are now the grid coordinates, probably
			
		//get 2d int array from piece- ideally by calling an internal method that returns
		//	an appropriately sized array based on the piece's internal rotation
		int[][] pieceValues = p.GetArray();
		
		//make sure its width/height actually fit
			//if xcounter + length > GRID_SIZE, return false
			//if ycounter + height > GRID_SIZE, return false
			
		//at this point we know it at least fits within the bounds of the grid
		
		//compare each grid square
		for(int i = 0; i < pieceValues.length; i++){
			for(int j = 0; j < pieceValues[0].length; j++){
				Occupancy o = grid[ycounter + i][xcounter + j];
				int shapecode = pieceValues[i][j];
				
				//do some simple checks
				if(shapecode == 0 || o.IsEmpty())
					continue;
				if(shapecode == 1 && !o.IsEmpty(){
					return false;
				}
				if(shapecode != 0 && o.IsFull(){
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