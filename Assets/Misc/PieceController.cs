using UnityEngine;

public class PieceController : MonoBehaviour{

	public static readonly int NORTHWEST_CODE = 2;
	public static readonly int NORTHEAST_CODE = 3;
	public static readonly int SOUTHEAST_CODE = 4;
	public static readonly int SOUTHWEST_CODE = 5;
	
	int rotation = 0;
	int[,] codes;
	
	public void Start(){
	
	}
	public void Update(){
	
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
					int codeAtLocation = codes[codes.GetLength(1)-1-j,i];
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