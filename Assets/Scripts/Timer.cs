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
		startTime = GamePause.pausableTime;//Time.fixedTime;
	}

	public void Restart(){
		startTime = GamePause.pausableTime;//Time.fixedTime;
	}
	public long TimeElapsedMillis(){
		return (long)(int)((GamePause.pausableTime-startTime)*1000);
		/*
		if (!GamePause.paused) {
			//hold time in variable to stop timer from returning 0 when paused
			milliHolder = (long)(int)((Time.fixedTime-startTime)*1000);
			return milliHolder;
		} else {
			return milliHolder;
		}*/
	}
	public float TimeElapsedSecs(){
		return GamePause.pausableTime-startTime;
		/*if (!GamePause.paused) {
			secHolder = Time.fixedTime-startTime;
			return secHolder;
		} else {
			return secHolder;
		}*/
	}
	/*void Update(){
		isPaused = GamePause.paused;
		if (!isPaused) {
			Restart ();
		}
	}*/
}

