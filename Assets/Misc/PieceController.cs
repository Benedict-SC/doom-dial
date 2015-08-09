public class PieceController : MonoBehavior{

	public static readonly int NORTHWEST_CODE = 2;
	public static readonly int NORTHEAST_CODE = 3;
	public static readonly int SOUTHEAST_CODE = 4;
	public static readonly int SOUTHWEST_CODE = 5;
	
	int rotation = 0;
	int[][] codes;
	
	public void Start(){
	
	}
	public void Update(){
	
	}
	
	public int[][] GetArray(){
		if(rotation == 0){
			int[][] result = new int[codes.length][codes[0].length];
			for(int i = 0; i < codes.length; i++){
				for(int j = 0; j < codes[0].length; j++){
					result[i][j] = codes[i][j];
				}
			}
			return result;
		}else if(rotation == 90){
			int[][] result = new int[codes[0].length][codes.length];
			for(int i = 0; i < result.length; i++){
				for(int j = 0; j < result[0].length; j++){
					int codeAtLocation = codes[codes[0].length-1-j][i];
					//rotate half blocks
					if(codeAtLocation > 1){
						codeAtLocation++;
					}
					if(codeAtLocation > SOUTHWEST_CODE){
						codeAtLocation -= 4;
					}
					result[i][j] = codeAtLocation;
				}
			}
			return result;
		}else if(rotation == 180){
			int[][] result = new int[codes.length][codes[0].length];
			for(int i = 0; i < result.length; i++){
				for(int j = 0; j < result[0].length; j++){
					int codeAtLocation = codes[codes.length-1-i][codes[0].length-1-j];
					//rotate half blocks
					if(codeAtLocation > 1){
						codeAtLocation += 2;
					}
					if(codeAtLocation > SOUTHWEST_CODE){
						codeAtLocation -= 4;
					}
					result[i][j] = codeAtLocation;
				}
			}
			return result;
		}else if(rotation == 270){
			int[][] result = new int[codes[0].length][codes.length];
			for(int i = 0; i < result.length; i++){
				for(int j = 0; j < result[0].length; j++){
					int codeAtLocation = codes[j][codes.length-1-i];
					//rotate half blocks
					if(codeAtLocation > 1){
						codeAtLocation += 3;
					}
					if(codeAtLocation > SOUTHWEST_CODE){
						codeAtLocation -= 4;
					}
					result[i][j] = codeAtLocation;
				}
			}
			return result;
		}
		else return null;
	}

}