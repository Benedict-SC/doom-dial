using UnityEngine;
using System.Collections.Generic;
using System.IO;
using MiniJSON;

public class MenuDial : MonoBehaviour,EventHandler {

	public float anchorX = 0;
	public float anchorY = 0;
	
	public float headAngle = 20f;
	public float optionDistance = 210f;
	int slots = 0; //default
	public List<MenuOption> options = null;
	MenuOption selected = null;
    EnemyStatsPanel enemyStatsPanel = null;
	
	public string menuType = "world";
	
	Canvas canvas;
	public Canvas overrideCanvas = null;
	
	bool spinning = false;
	float startingMouseRot;
	float startingDialRot;
	
	public void Start(){
		if(overrideCanvas == null)
			canvas = GameObject.Find("Canvas").GetComponent<Canvas>();
		else
			canvas = overrideCanvas;
		RectTransform rt = (RectTransform)transform;
		EventManager em = EventManager.Instance ();
		em.RegisterForEventType ("mouse_release", this);
		em.RegisterForEventType ("mouse_click", this);
		anchorX = rt.anchoredPosition.x;
		anchorY = rt.anchoredPosition.y;
		transform.rotation = Quaternion.Euler (0, 0, headAngle);

        //if there's an Enemy Stats Panel in the scene
        if (GameObject.Find("Stats Panel"))
        {
            enemyStatsPanel = GameObject.Find("Stats Panel").GetComponent<EnemyStatsPanel>();
        }
        if (slots >= 1)
            RefreshSelectedOption();
    }
	public MenuOption GetSelectedOption(){
		return selected;
	}
	public void ExecuteSelectedOption(){
		//Debug.Log (selected.GetButtonText());
		selected.WhenChosen();
		
	}
	public void RefreshSelectedOption(){
		int index = (int)((options.Count)-(Mathf.Round((transform.rotation.eulerAngles.z-headAngle)/(360f/slots) )));
        //Debug.Log("initial index: " + index);
		while(index >= options.Count){
			index -= options.Count;
            //Debug.Log("index too big, clipped to " + index);
		}
        while (index < 0){
            //Debug.Log("index too small, increased to " + index);
			index += options.Count;
		}
        //Debug.Log("final index: " + index);
		selected = options[index];
        RefreshEnemyStatsPanel();
	}

    public void RefreshEnemyStatsPanel()
    {
        if (enemyStatsPanel != null && selected.enemyFilename != null)
        {
            enemyStatsPanel.SetCurrentEnemy(selected.enemyFilename);
        }
    }

	public void AddOption(MenuOption mo){ //will later have arguments
		if(options == null){
			options = new List<MenuOption>();
			selected = mo;
		}
		slots++;
		options.Add(mo);
		RearrangeOptions();
	}
	public void RemoveOption(MenuOption mo){ //will later have arguments
		if(options == null){
			options = new List<MenuOption>();
			return;
		}
		slots--;
		options.Remove(mo);
		RearrangeOptions();
	}

    public void RemoveAllOptions()
    {
        while (options.Count > 0)
        {
            RemoveOption(options[0]);
        }
    }
	public void RearrangeOptions(){
		for(int i = 0; i < options.Count; i++){
			MenuOption mo = options[i];
			float angle = (360f/slots)*i;
			float x = optionDistance*Mathf.Cos(angle*Mathf.Deg2Rad);
			float y = optionDistance*Mathf.Sin(angle*Mathf.Deg2Rad);
			mo.gameObject.GetComponent<RectTransform>().anchoredPosition = new Vector2(x,y);
			mo.gameObject.GetComponent<MenuScaleEffectCanvas>().RefreshRotOffset();
		}
        RefreshSelectedOption();
	}
	
	public void HandleEvent(GameEvent ge){
		//if(Pause.paused)
		//	return;
		Vector3 mousepos = InputWatcher.GetCanvasInputPosition((RectTransform)canvas.transform);
		if(ge.type.Equals("mouse_click")){
			if(spinning)
				return;
			spinning = true;
			startingDialRot = transform.eulerAngles.z * Mathf.Deg2Rad;
			startingMouseRot = Mathf.Atan2(mousepos.y-anchorY,mousepos.x-anchorX);
		}else if(ge.type.Equals("mouse_release")){
			if(!spinning)
				return;
			spinning = false;
			float mouseAngle = Mathf.Atan2 ((mousepos.y - anchorY) - transform.position.y, (mousepos.x-anchorX) - transform.position.x);
			float angleChange = mouseAngle-startingMouseRot;
			transform.eulerAngles = new Vector3(transform.eulerAngles.x,transform.eulerAngles.y,(startingDialRot + angleChange)*Mathf.Rad2Deg);
			float rotation = transform.eulerAngles.z;
			float lockRot = LockDegrees(rotation); //lock to the right angle based on how many options there are
			transform.rotation = Quaternion.Euler (0, 0, lockRot);
			RefreshSelectedOption();
		}
	}
	public float LockDegrees(float rotation){
		rotation -= headAngle;
		float lockRot = Mathf.Round (rotation / (360f/slots)) * (360f/slots);//snap to thing
		float realAngle = lockRot + headAngle; //convert back to real degrees
		return realAngle;
	}
	public void Update(){
		//if(Pause.paused)
		//	return;
		if(!spinning)
			return;
		Vector3 mousepos = InputWatcher.GetCanvasInputPosition((RectTransform)canvas.transform);
		float mouseAngle = Mathf.Atan2 ((mousepos.y - anchorY) - transform.position.y, (mousepos.x-anchorX) - transform.position.x);
		float angleChange = mouseAngle-startingMouseRot;
		transform.eulerAngles = new Vector3(transform.eulerAngles.x,transform.eulerAngles.y,(startingDialRot + angleChange)*Mathf.Rad2Deg);
	}
	public bool IsSpinning(){
		return spinning;
	}
	public List<MenuOption> GetOptions()
    {
        return options;
    }
}
