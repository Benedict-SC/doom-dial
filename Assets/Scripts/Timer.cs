using System;

public class Timer
{
	DateTime startTime;

	public Timer(){
		startTime = DateTime.Now;
	}

	public void Restart(){
		startTime = DateTime.Now;
	}
	public long TimeElapsedMillis(){
		return Convert.ToInt64 ((DateTime.Now - startTime).TotalMilliseconds);
	}
	public float TimeElapsedSecs(){
		return (float)((DateTime.Now - startTime).TotalSeconds);
	}
}

