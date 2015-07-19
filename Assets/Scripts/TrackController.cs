using UnityEngine;
using System.Collections;
using System;

public class TrackController : MonoBehaviour, EventHandler {

	SpriteRenderer[] tracks = new SpriteRenderer[6];
	int[] intensityCeilings = new int[6];
	int[] intensityFloors = new int[6];
	int[] flashes = new int[6];
	long[] lengthsInMillis = new long[6];
	//long[] advanceWarningMillis = new long[6];

	public static readonly int DEF_INTENSITY_CEILING = 128;
	public static readonly int DEF_INTENSITY_FLOOR = 64;
	public static readonly int DEF_FLASHES = 3;
	public static readonly long DEF_LENGTH_IN_MILLIS = 3000;
	//long DEF_ADVANCE_WARNING = 3000;

	public static readonly float NORMAL_SPEED = 8.0f;

	long[] startTimes = new long[6];

	// Use this for initialization
	void Start () {
		//get tracks
		tracks [0] = transform.FindChild ("WarningN").gameObject.GetComponent<SpriteRenderer>();
		tracks [1] = transform.FindChild ("WarningNE").gameObject.GetComponent<SpriteRenderer>();
		tracks [2] = transform.FindChild ("WarningSE").gameObject.GetComponent<SpriteRenderer>();
		tracks [3] = transform.FindChild ("WarningS").gameObject.GetComponent<SpriteRenderer>();
		tracks [4] = transform.FindChild ("WarningSW").gameObject.GetComponent<SpriteRenderer>();
		tracks [5] = transform.FindChild ("WarningNW").gameObject.GetComponent<SpriteRenderer>();
		for (int i = 0; i < 6; i++) {
			//set defaults
			intensityCeilings[i] = DEF_INTENSITY_CEILING;
			intensityFloors[i] = DEF_INTENSITY_FLOOR;
			flashes[i] = DEF_FLASHES;
			lengthsInMillis[i] = DEF_LENGTH_IN_MILLIS;
			//advanceWarningMillis[i] = DEF_ADVANCE_WARNING;
			startTimes[i] = -1;
		}
		EventManager.Instance ().RegisterForEventType ("warning", this);
	}
	
	// Update is called once per frame
	void Update () {
		for(int i = 0; i < 6; i++){
			if(startTimes[i] != -1){
				//this lane is flashing
				TimeSpan currentTime = DateTime.Now - new DateTime(1970, 1, 1);
				long elapsed = Convert.ToInt64(currentTime.TotalMilliseconds) - startTimes[i];
				//we have elapsed time since warning started- convert this to an alpha value for the overlay

				int alpha = 0;

				//calculate interval size
				//this is the time in between going from the intensity ceiling to the floor, and vice versa
				long interval = lengthsInMillis[i] / (2*(flashes[i]+1));
				//Debug.Log("interval is " + interval + " (" + lengthsInMillis[i] + "/2*" + flashes[i] + ")");
				int position = (int)(elapsed/interval);
				long innerPosition = elapsed % interval;
				long endcapPosition = elapsed % (interval*2);

				//calculate alpha
				if(position == 0 || position == 1){ //ramp up to ceiling from 0
					alpha = (int)(intensityCeilings[i] * ( (float)endcapPosition / ((float)interval*2) ));
				}else if(position >= flashes[i] * 2){ //ramp down to 0
					//Debug.Log ("are we even getting here");
					alpha = (int)(intensityCeilings[i] * (1.0f-( (float)endcapPosition / ((float)interval*2) )));
				}else if(position % 2 == 0){ //ramp down to floor
					float factor = (float)innerPosition / (float)interval;
					int alphaRange = intensityCeilings[i]-intensityFloors[i];
					int alphaOffset = (int)(alphaRange * factor);
					alpha = intensityCeilings[i] - alphaOffset;
				}else if(position % 2 == 1){ //ramp up to ceiling
					float factor = (float)innerPosition / (float)interval;
					int alphaRange = intensityCeilings[i]-intensityFloors[i];
					int alphaOffset = (int)(alphaRange * factor);
					alpha = intensityFloors[i] + alphaOffset;
				}
				//Debug.Log("alpha for track " + (i+1) + " is " + alpha);
				//Debug.Log("position for track " + (i+1) + " is " + alpha);
				//we've calculated an alpha- plug this into the sprite renderer's color
				float floatAlpha = alpha / 255.0f;
				tracks[i].color = new Color(tracks[i].color.r,tracks[i].color.g,tracks[i].color.b,floatAlpha);

				if(elapsed > lengthsInMillis[i]){
					startTimes[i] = -1;
					tracks[i].color = new Color(tracks[i].color.r,tracks[i].color.g,tracks[i].color.b,0);
				}
			}
		}
	}

	public void HandleEvent(GameEvent ge){
		EnemyController ec = ge.args [0] as EnemyController;
		int trackID = ec.GetTrackID () - 1;
		if (startTimes [trackID] != -1)
			return; //don't warn if the track is warning for something else already
		if (ec.GetSpawnTime () < lengthsInMillis[trackID]){
			//Debug.Log("an enemy is spawning before there's warning time");
			return; //don't warn if there's no time to warn
		}

		//set up the track to warn
		//determine flash speed
		float flashfactor = NORMAL_SPEED / ec.GetImpactTime();
		//Debug.Log("flashfactor is " + flashfactor + " (" + ec.GetSpeed() + "/" + NORMAL_SPEED + ")");
		flashes [trackID] = (int)(flashfactor * DEF_FLASHES);
		if (flashes [trackID] <= 0)
			flashes [trackID] = 1;
		//determine intensity range
		//TBD
		//determine length of flash
		//TBD

		//start warning
		startTimes[trackID] = Convert.ToInt64((DateTime.Now - new DateTime(1970, 1, 1)).TotalMilliseconds);
	}

	public long GetHeadStartOfTrack(int id){
		if (id < 1 || id > 6) {
			//Debug.Log("id was " + id);
			return -1;
		}
		return lengthsInMillis[id-1];
	}


}
