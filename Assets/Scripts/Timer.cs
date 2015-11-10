using System;
using UnityEngine;
using System.Collections;
public class Timer
{
	float startTime;
	bool isPaused = false;
	long milliHolder;
	float secHolder;
	public Timer(){
		startTime = Time.fixedTime;
	}

	public void Restart(){
		startTime = Time.fixedTime;
	}
	public long TimeElapsedMillis(){
		if (!isPaused) {
			//hold time in variable to stop timer from returning 0 when paused
			milliHolder = (long)(int)((Time.fixedTime-startTime)*1000);
			return milliHolder;
		} else {
			return milliHolder;
		}
	}
	public float TimeElapsedSecs(){
		if (!isPaused) {
			secHolder = Time.fixedTime-startTime;
			return secHolder;
		} else {
			return secHolder;
		}
	}
	void Update(){
		isPaused = GamePause.paused;
		if (!isPaused) {
			Restart ();
		}
	}
}

