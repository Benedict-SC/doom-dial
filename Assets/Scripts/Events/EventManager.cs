using UnityEngine;
using System.Collections.Generic;

/* List of known events, for reference. (Update when you add a new event type, please)
 * 
 * mouse_click
 * mouse_release
 * shot_fired
 * shot_collided
 */

public class EventManager{
	private Dictionary<string,Queue<GameEvent>> eventQueues;
	private Dictionary<string,List<EventHandler>> registry;
	
	private static EventManager self = null;
	private EventManager(){
		eventQueues = new Dictionary<string,Queue<GameEvent>>();
		registry = new Dictionary<string,List<EventHandler>>();
	}
	public static EventManager Instance(){
		if(self == null)
			self = new EventManager();
		return self;
	}
	
	public void RegisterForEventType(string type,EventHandler listener){
		if (registry.ContainsKey (type)) {
			List<EventHandler> listeners = null;
			bool worked = registry.TryGetValue(type,out listeners);
			if(worked)
				listeners.Add(listener);
		} else {
			List<EventHandler> newlist = new List<EventHandler>();
			newlist.Add(listener);
			registry.Add(type,newlist);
		}
	}
	public void RaiseEvent(GameEvent ge){
		string type = ge.type;
		if (eventQueues.ContainsKey (type)) {
			Queue<GameEvent> q = null;
			eventQueues.TryGetValue (type, out q);
			q.Enqueue (ge);
		} else {
			Queue<GameEvent> q = new Queue<GameEvent>();
			q.Enqueue(ge);
			eventQueues.Add(type,q);
		}
	}

	public void Update(){
		List<string> keys = new List<string>();
		foreach (string s in eventQueues.Keys) {
			keys.Add(s); //don't ask me why we can't just iterate over eventQueues.Keys. some kinda sync error
		}
		foreach (string s in keys) {
			Queue<GameEvent> q = null;
			eventQueues.TryGetValue(s,out q);
			while(q.Count > 0){
				GameEvent ge = q.Dequeue();
				List<EventHandler> handlers = null;
				bool hasHandlers = registry.TryGetValue(s, out handlers);
				if(!hasHandlers){
					continue;
				}
				foreach(EventHandler eh in handlers){
					if(eh != null) //in case a listener has been destroyed
						eh.HandleEvent(ge);
				}
			}
		}
	}
}