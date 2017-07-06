using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class WaveMenuOption : MonoBehaviour{
	
    Text name;
    Image img;
    LoadPanel lp;
	public void Start(){
        lp = GameObject.Find("LoadPanel").GetComponent<LoadPanel>();
		name = transform.Find("Text").GetComponent<Text>();
        img = GetComponent<Image>();
	}
	public void Update(){
		
	}
    public void HighlightOption(){
        foreach(Transform child in transform.parent){
            child.GetComponent<Image>().color = new Color(1,1,1);
        }
        img.color = new Color(0.4f,0.75f,1f);
        lp.pickedFilename = name.text;
    }
}
