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
		if (isPaused) {
			Restart ();
		}
		isPaused = !isPaused;

	}
}

