using UnityEngine;
using UnityEngine.UI;
using MiniJSON;
using System.IO;
using System.Collections.Generic;

public class EnemyTemplateController : MonoBehaviour,EventHandler{

	string filename = "";
	string enemyname = "$$$$";
	string listname = "@@@@";
	string columnname = "####";
	public int fontsize = 18;
	string imgfilename = "Placeholder";
	int pointValue = 0;
	
	public readonly float maxheight = 80f;
	public readonly float maxwidth = 100f;
	
	Timer holdTimer;
	bool beingHeld = false;

	public void Start(){
		holdTimer = new Timer();
		
		EventManager.Instance().RegisterForEventType("mouse_click",this);
		EventManager.Instance().RegisterForEventType("mouse_release",this);
	}
	public void Update(){
		if(beingHeld){
			if(!TouchIsOnMe(InputWatcher.GetInputPosition())){
				beingHeld = false;
				return;
			}
			if(holdTimer.TimeElapsedMillis() > 120){
				beingHeld = false;
				WaveEditorController.singleton.AttachEnemy(this);
			}
		}
	}
	public void HandleEvent(GameEvent ge){
		Vector3 pos = (Vector3)ge.args[0];
		if(ge.type.Equals("mouse_click")){
			if(TouchIsOnMe(pos)){
				beingHeld = true;
				holdTimer.Restart();
			}
		}
		if(ge.type.Equals("mouse_release")){
			beingHeld = false;
		}
	}
	public bool TouchIsOnMe(Vector3 touchpos){
		if(BossTabController.open){
			return false;
		}
		if(WaveEditorController.singleton.panelOpen){
			return false;
		}
		if(WaveEditorController.singleton.IsMoving()){
			return false;
		}
		RectTransform rt = (RectTransform)transform;
		Vector3 newpoint = rt.InverseTransformPoint(new Vector2(touchpos.x,touchpos.y));
		/*Debug.Log ( "old point is " + touchpos.x + ", " + touchpos.y + "\n" +
					"newpoint is " + newpoint.x + ", " + newpoint.y + "\n" +
					"rect is from " + rt.rect.x + ", " + rt.rect.y + " to " + 
					(rt.rect.x+rt.rect.width) + ", " + (rt.rect.y + rt.rect.height));*/
		bool rectangleOverlap = rt.rect.Contains(newpoint);
		return rectangleOverlap;
	}
	
	public void ConfigureFromJSON(string efn){
		filename = efn;
		FileLoader fl = new FileLoader ("JSONData" + Path.DirectorySeparatorChar + "Bestiary",efn);
		string json = fl.Read ();
		Dictionary<string,System.Object> data = (Dictionary<string,System.Object>)Json.Deserialize (json);
		
		imgfilename = (string)data["imgfile"];
		Image img = transform.gameObject.GetComponent<Image> ();
		Texture2D decal = Resources.Load<Texture2D> ("Sprites/EnemySprites/" + imgfilename);
		if (decal == null)
			Debug.Log("decal is null");
		img.sprite = UnityEngine.Sprite.Create (
			decal,
			new Rect(0,0,decal.width,decal.height),
			new Vector2(0.5f,0.5f),
			100f);
		//set point value	
		pointValue = (int)(long)data["pointValue"];
		Transform pTransform = transform.Find("PointValue");
		Text pointText = pTransform.gameObject.GetComponent<Text>();
		pointText.text = "" + pointValue;
		//set name
		enemyname = (string)data["name"];
		if(data.ContainsKey("listName")){
			listname = (string)data["listName"];
		}else{
			listname = enemyname;
		}
		if(data.ContainsKey("columnName")){
			columnname = (string)data["columnName"];
			//Debug.Log("column name: " + columnname);
		}else{
			//Debug.Log (enemyname);
			columnname = enemyname;
		}
		if(data.ContainsKey("fontSize")){
			fontsize = (int)(long)data["fontSize"];
		}
		Transform nTransform = transform.Find("EnemyName");
		Text nameText = nTransform.gameObject.GetComponent<Text>();
		nameText.fontSize = fontsize;
		nameText.text = listname;
		
		//set size stuff
		RectTransform rt = (RectTransform)transform;
		Debug.Log(decal.width + ", " + decal.height);
		//figure out how much to scale it by to fit
		float xratio = ((float)decal.width)/maxwidth;
		float yratio = ((float)decal.height)/maxheight;
		float highestratio;
		if(xratio >= yratio){
			highestratio = xratio;
		}else{
			highestratio = yratio;
		}
		float newWidth = ((float)decal.width) / highestratio;
		float newHeight = ((float)decal.height) / highestratio;
		
		Vector3 sizeStuff = new Vector3(newWidth,newHeight,transform.position.z);
		rt.sizeDelta = sizeStuff;
	}
	public int GetPointValue(){
		return pointValue;
	}
	public string GetImgFileName(){
		return imgfilename;
	}
	public string GetName(){
		return enemyname;
	}
	public string GetEnemyListName(){
		return listname;
	}
	public string GetEditorColumnName(){
		return columnname;
	}
	public string GetSrcFileName(){
		return filename;
	}
}
