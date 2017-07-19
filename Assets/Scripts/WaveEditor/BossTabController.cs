using UnityEngine;
using UnityEngine.UI;

public class BossTabController : MonoBehaviour{
	RectTransform rt; //it's just the transform but unity forces you to cast it
	Text bossText;
	
	enum states{CLOSED,RISING,GROWING,OPEN,SHRINKING,FALLING};
	states state = states.CLOSED;
	public static bool open = false;
	int bossIndex = 0;
	
	float startingY;
	float maxHeight = 84f;
	float height = 0;
	
	GameObject bossbg;
	float scale = 0.001f;
	float maxScale = 0.65f;

	public void Start(){
		rt = (RectTransform)transform;
		startingY = rt.anchoredPosition.y;
		bossbg = GameObject.Find("BossBG");
		bossText = transform.Find("Text").GetComponent<Text>();
		bossbg.SetActive(false);
	}
	public void Update(){
		switch(state){
			case states.CLOSED:
				break; //do nothing
			case states.RISING:
				if(height < 60f)
					height += 10;
				else if(height <78f)
					height += 5;
				else if(height < maxHeight)
					height += 2;
				else{
					height = maxHeight;
					state = states.GROWING;
					bossbg.SetActive(true);
					//RectTransform bg = (RectTransform)bossbg.transform;
					//bg.sizeDelta = new Vector2(0.01f,0.01f);
				}
				rt.anchoredPosition = new Vector2(rt.anchoredPosition.x,startingY + height);
				break;
			case states.GROWING:
				scale += 0.08f;
				if(scale >= maxScale){
					scale = maxScale;
					state = states.OPEN;
					open = true;
					//fill with boss options
				}
				RectTransform bgrt = (RectTransform)bossbg.transform;
				bgrt.localScale = new Vector3(scale,scale,1f);
				break;
			case states.OPEN:
				break;
			case states.SHRINKING:
				scale -= 0.08f;
				if(scale <= 0){
					scale = 0.01f;
					bossbg.SetActive(false);
					state = states.FALLING;
				}
				RectTransform bgrt2 = (RectTransform)bossbg.transform;
				bgrt2.localScale = new Vector3(scale,scale,1f);
				break;
			case states.FALLING:
				if(height >24f)
					height -= 10;
				else if(height >9f)
					height -= 5;
				else if(height >0)
					height -= 2;
				else{
					height = 0;
					state = states.CLOSED;
				}
				rt.anchoredPosition = new Vector2(rt.anchoredPosition.x,startingY + height);
				break;
		}
	}
	
	public void PopUp(){
		if(state == states.CLOSED)
			state = states.RISING;
		//move up
		//disable rest of UI
		//grow select thing
		//fill select thing with bosses
		//change text to OK
	}
	public void Okay(){
		if(state == states.OPEN){
			open = false;
			state = states.SHRINKING;
		}
		//shrink select thing
		//move down
		//reenable UI
	}
	public int GetBossIndex(){
		return bossIndex;
	}
	public void SetBossIndex(int idx){
		if(idx > 6 || idx < 0)
			return;
		bossIndex = idx;
		switch(bossIndex){
			case 0: 
				bossText.text = "No Boss";
				break;
			case 1: 
				bossText.text = "S.Master";
				break;
			case 2: 
				bossText.text = "Megaboid";
				break;
			case 3: 
				bossText.text = "Big Bulk";
				break;
			case 4: 
				bossText.text = "Skizzard";
				break;
			case 5: 
				bossText.text = "???";
				break;
			case 6: 
				bossText.text = "???";
				break;
			default:
				break;
		}
	}
}
