using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SplashLogo : MonoBehaviour {

    Image logoImg;
    Color originalColor;
    Color finalColor;

    public float fadeInSpeed = 1;

	// Use this for initialization
	void Start () {

        logoImg = GetComponent<Image>();
        logoImg.color = new Color(logoImg.color.r, logoImg.color.g, logoImg.color.b, 0f);

        originalColor = logoImg.color;
        finalColor = new Color(originalColor.r, originalColor.g, originalColor.b, 1f);
	}
	
	// Update is called once per frame
	void Update () {

        Color newColor = Color.Lerp(originalColor, finalColor, Time.time / (1/fadeInSpeed));
        //oldTime += Time.deltaTime * 50;
        logoImg.color = newColor;
	}
}
