using UnityEngine;
using UnityEngine.UI;

public class WaveMessageBox : MonoBehaviour,EventHandler{

	Text box;
	int upcomingWaveNumber = 0;
	
	//lot of reused code from zoneWarning
	float flashTime; //the amount of time to flash for
	readonly float BRIGHTNESS_MIN = 128f;
	readonly float BRIGHTNESS_MAX = 255f;
	float rampTime;
	
	float brightness_mid; //midpoint of flash intensity when not ramping up/down
	float brightness_radius; //how far to oscillate around the midpoint
	Timer flashTimer;
	bool flashing = false;
	int flashCount = 6; //default number of flashes
	
	public void Start(){
		box = gameObject.GetComponent<Text>();
		EventManager.Instance().RegisterForEventType("wave_message_flash",this);
		
		flashTimer = new Timer();
		brightness_mid = (BRIGHTNESS_MIN+BRIGHTNESS_MAX)/2f;
		brightness_radius = brightness_mid-BRIGHTNESS_MIN;
	}
	public void Update(){
		if(flashing){
			float time = flashTimer.TimeElapsedSecs();
			if(time > flashTime){
				flashing = false;
				return;
			}
			if(time < rampTime){ //it's ramp-up
				float percent = time/rampTime;
				int alpha = (int)(percent*BRIGHTNESS_MIN);
				SetAlpha (alpha);
			}else if(time > flashTime-rampTime){ //it's ramp-down
				float downtime = time - (flashTime-rampTime);
				//Debug.Log("downtime: " + downtime);
				//Debug.Log("ramptime: " + rampTime);
				float percent = 1f - downtime/rampTime;
				//Debug.Log("downtime/ramptime: " + (downtime/rampTime));
				int alpha = (int)(percent*BRIGHTNESS_MIN);
				//Debug.Log ("alpha: " + alpha);
				SetAlpha (alpha);
			}else{//we're in sin wave mode
				float sinPercent = (time-rampTime)/(flashTime-(2*rampTime)); //what percent of the non-ramp phase we're through
				float piScale = Mathf.PI * (flashCount*2 - 1); //the range of values for the sin function
				float sinPosition = Mathf.Sin (sinPercent*piScale);
				float decimalAlpha = brightness_mid + sinPosition*brightness_radius;
				int alpha = (int)decimalAlpha;
				SetAlpha (alpha);
			}
		}
		int number = (int)(flashTime - flashTimer.TimeElapsedSecs());
		string message = "Incoming! The enemy's last-ditch attack!";
		if(upcomingWaveNumber != -1) //if it's not the bonus wave, have a countdown
			message = "Wave " + upcomingWaveNumber + " in " + (number+1) + " seconds!";
		box.text = message;
	}
	public void HandleEvent(GameEvent ge){
		//only event is a message flash
		string message = (string)ge.args[0];
		float seconds = (float)ge.args[1];
		upcomingWaveNumber = (int)ge.args[2];
		
		box.text = message;
		flashTime = seconds;
		rampTime = .075f*seconds;
		
		flashing = true;
		flashTimer.Restart();
	}
	void SetAlpha(int eightbit){
		float alpha = ((float)eightbit)/255f;
		box.color = new Color(box.color.r,box.color.g,box.color.b,alpha);
	}
	
	public static void StandardWarning(int waveNumber){
		GameEvent ge = new GameEvent("wave_message_flash");
		string message = "Wave " + waveNumber + " in " + WaveManager2.BREATHER_SECONDS + " seconds!";
		ge.addArgument(message);
		ge.addArgument((float)WaveManager2.BREATHER_SECONDS);
		ge.addArgument(waveNumber);
		EventManager.Instance().RaiseEvent(ge);
	}
}
