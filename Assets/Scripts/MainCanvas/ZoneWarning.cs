using UnityEngine;
using UnityEngine.UI;

public class ZoneWarning : MonoBehaviour,EventHandler{

	public static readonly float HEAD_START = 2f; //the amount of time to flash for
	readonly float BRIGHTNESS_MIN = 64f;
	readonly float BRIGHTNESS_MAX = 190f;
	readonly float RAMP_TIME = .15f;
	
	Image overlay;
	
	float brightness_mid; //midpoint of flash intensity when not ramping up/down
	float brightness_radius; //how far to oscillate around the midpoint
	Timer flashTimer;
	bool flashing = false;
	int flashCount = 2; //default number of flashes
	public int id;

    bool doesFlash = true; //is False if the Ambush risk is on

	public void Start(){
		overlay = gameObject.GetComponent<Image>();
	
		flashTimer = new Timer();
		brightness_mid = (BRIGHTNESS_MIN+BRIGHTNESS_MAX)/2f;
		brightness_radius = brightness_mid-BRIGHTNESS_MIN;
		EventManager.Instance().RegisterForEventType("warning",this);

        //if Ambush risk is on, disable zone warnings
        if (PlayerPrefsInfo.Int2Bool(PlayerPrefs.GetInt(PlayerPrefsInfo.s_ambush)))
        {
            doesFlash = false;
        }
	}
	public void Update(){
		if(flashing && doesFlash){
			float time = flashTimer.TimeElapsedSecs();
			if(time > HEAD_START){
				flashing = false;
				return;
			}
			if(time < RAMP_TIME){ //it's ramp-up
				float percent = time/RAMP_TIME;
				int alpha = (int)(percent*BRIGHTNESS_MIN);
				SetAlpha (alpha);
			}else if(time > HEAD_START-RAMP_TIME){ //it's ramp-down
				float downtime = time - (HEAD_START-RAMP_TIME);
				float percent = 1f - downtime/RAMP_TIME;
				int alpha = (int)(percent*BRIGHTNESS_MIN);
				//Debug.Log ("alpha: " + alpha);
				SetAlpha (alpha);
			}else{//we're in sin wave mode
				float sinPercent = (time-RAMP_TIME)/(HEAD_START-(2*RAMP_TIME)); //what percent of the non-ramp phase we're through
				float piScale = Mathf.PI * (flashCount*2 - 1); //the range of values for the sin function
				float sinPosition = Mathf.Sin (sinPercent*piScale);
				float decimalAlpha = brightness_mid + sinPosition*brightness_radius;
				int alpha = (int)decimalAlpha;
				SetAlpha (alpha);
			}
		}
	}
	public void HandleEvent(GameEvent ge){
		Enemy e = (Enemy)ge.args[0];
		int trackID = e.GetCurrentTrackID();	
		if(id == trackID){//the enemy is spawning in this zone
			flashing = true;
			flashTimer.Restart();
		}
	}
	void SetAlpha(int eightbit){
		float alpha = ((float)eightbit)/255f;
		overlay.color = new Color(overlay.color.r,overlay.color.g,overlay.color.b,alpha);
	}
}