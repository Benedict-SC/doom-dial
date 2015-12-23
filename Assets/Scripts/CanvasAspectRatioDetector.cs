using UnityEngine;
using System.Collections;

/*
* Code adapted from Stackoverflow answer 
* http://answers.unity3d.com/questions/618058/mobile-device-screen-sizes.html
* by user siddharth3322
*/

public class CanvasAspectRatioDetector : MonoBehaviour {
	
	// Use this for initialization
	void Start()
	{
		// set the desired aspect ratio (the values in this example are
		// hard-coded for 16:9, but you could make them into public
		// variables instead so you can set them at design time)
		float targetaspect = 16.0f / 10.0f;
		
		// determine the game window's current aspect ratio
		float windowaspect = (float)Screen.width / (float)Screen.height;
		
		// current viewport height should be scaled by this amount
		float scaleheight = windowaspect / targetaspect;
		
		// obtain camera component so we can modify its viewport
		Canvas canvas = GetComponent<Canvas>();
		RectTransform rt = (RectTransform)transform;
		
		// if scaled height is less than current height, add letterbox
		if (scaleheight < 1.0f)
		{
			Rect rect = rt.rect;
			
			rect.width = 1.0f;
			rect.height = scaleheight;
			rect.x = 0;
			rect.y = (1.0f - scaleheight) / 2.0f;
			
			rt.sizeDelta = new Vector2(rect.width,rect.height);
			//rt.rect = rect;
		}
		else // add pillarbox
		{
			float scalewidth = 1.0f / scaleheight;
			
			Rect rect = rt.rect;
			
			rect.width = scalewidth;
			rect.height = 1.0f;
			rect.x = (1.0f - scalewidth) / 2.0f;
			rect.y = 0;
			
			rt.sizeDelta = new Vector2(rect.width,rect.height);
			//rt.rect = rect;			
		}
	}
	
}

