using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class MenuOption : MonoBehaviour{

	string iconFilename = "TowerPink";
	string dialText = "ERROR";
	string buttonText = "OPTION NOT CONFIGURED";
	Image icon = null;
	Text text = null;

    public string enemyFilename; //for EnemyIndexOption use
	
	public void ConfigureOption(string iconfilename,string dialtext, string buttontext){
		if(icon == null){
			icon = transform.FindChild("Image").gameObject.GetComponent<Image>();
		}if(text == null){
			text = transform.FindChild("Text").gameObject.GetComponent<Text>();
		}
		iconFilename = iconfilename;
		dialText = dialtext;
		buttonText = buttontext;
		
		Texture2D decal = Resources.Load<Texture2D> ("Sprites/" + iconFilename);
		icon.sprite = UnityEngine.Sprite.Create (
			decal,
			new Rect(0,0,decal.width,decal.height),
			new Vector2(0.5f,0.5f),
			100f);
			
		text.text = dialText;
	}
	public string GetButtonText(){
		return buttonText;
	}
	public virtual void WhenChosen(){
		Debug.Log ("this menu option does nothing");
	}
    public void SetDialText(string dText)
    {
        if (text == null)
        {
            text = transform.FindChild("Text").gameObject.GetComponent<Text>();
        }
        text.text = dText;
    }
    public void SizeImage(float height)
    {
        Texture2D decal = Resources.Load<Texture2D>("Sprites/" + iconFilename);
        RectTransform rt = transform.FindChild("Image").GetComponent<RectTransform>();
        float widthPercentOfHeight = (float)decal.width / (float)decal.height;
        rt.sizeDelta = new Vector2(widthPercentOfHeight * height, height);
        Debug.Log(iconFilename + ": " + rt.sizeDelta.ToString());
    }
}