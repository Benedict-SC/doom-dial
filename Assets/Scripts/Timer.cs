using System;
using UnityEngine;
using System.Collections;
public class Timer
{
	DateTime startTime;
	DateTime oldTime;
	bool isPaused = false;
	long milliHolder;
	float secHolder;
	public Timer(){
		startTime = DateTime.Now;
	}

	public void Restart(){
		startTime = DateTime.Now;
	}
	public long TimeElapsedMillis(){
		if (!isPaused) {
			//hold time in variable to stop timer from returning 0 when paused
			milliHolder = Convert.ToInt64 ((DateTime.Now - startTime).TotalMilliseconds);
			return milliHolder;
		} else {
			return milliHolder;
		}
	}
	public float TimeElapsedSecs(){
		if (!isPaused) {
			secHolder = (float)((DateTime.Now - startTime).TotalSeconds);
			return secHolder;
		} else {
			return secHolder;
		}
	}
	public void PauseTrigger(){
		//restart timer because otherwise all enemies will jump forward base on how long game is paused
		if (isPaused) {
			Restart ();
		}
		isPaused = !isPaused;

	}
}

