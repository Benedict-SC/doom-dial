using UnityEngine;
using System.Collections.Generic;

public class GameEvent{
	public string type;
	public List<object> args;

	public GameEvent (string eventtype){
		type = eventtype;
		args = new List<object> ();
	}
	public void addArgument(object o){
		args.Add (o);
	}
}

