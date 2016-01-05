using UnityEngine;
using UnityEngine.UI;

public class Rotations{
	public readonly static float TWO_PI = Mathf.PI*2f;
	public static float EulerAnglesToRadiansCounterclockwiseFromXAxis(float eulerangles){
		eulerangles += 90f;
		eulerangles = ClipDegrees(eulerangles);
		return eulerangles*Mathf.Deg2Rad;
	}
	public static float RadiansCounterclockwiseFromXAxisToEulerAngles(float radians){
		radians -= TWO_PI;
		radians = ClipRadians(radians);
		return radians*Mathf.Rad2Deg;
	}
	public static float ClipDegrees(float degrees){
		while(degrees > 360){
			degrees -= 360f;
		}
		while(degrees < 0){
			degrees += 360f;
		}
		return degrees;
	}
	public static float ClipRadians(float radians){
		while(radians > TWO_PI){
			radians -= TWO_PI;
		}
		while(radians < 0){
			radians += TWO_PI;
		}
		return radians;
	}
}