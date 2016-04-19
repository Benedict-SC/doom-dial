using UnityEngine;
using System.Collections.Generic;

/* List of known events, for reference. (Update when you add a new event type, please)
 * 
 * mouse_click
 * mouse_release
 * shot_fired
 * shot_collided
 * enemy_arrived
 * enemy_finished (string filename, float damage)
 * boss_hit (string bossname, float damage)
 * dial_damaged
 * warning (Enemy e)
 * piece_dropped
 * piece_obtained (string filename)
 * wave_message_flash (string message, float seconds, int waveNumber)
 *
 * tap
 * piece_tapped
 * template_tapped
 * piece_dropped_on_inventory
 * readout_update
 * alt_click
 * alt_release
 * alt_tap
 *
 * wave_editor_changed
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
	public void ClearAllEvents(){
		eventQueues.Clear();
		//registry.Clear();
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
					if(eh.Equals (null)){
						Debug.Log ("Null handler for event type " + s);
						continue;
					}
					//Debug.Log (eh.ToString());
					GameObject go = ((MonoBehaviour)eh).gameObject;
					if(go != null && !go.Equals(null)){ //in case a listener has been destroyed
						eh.HandleEvent(ge);
					}
				}
			}
		}
		//clear any null handlers
		List<string> rKeys = new List<string>();
		foreach (string s in registry.Keys) {
			rKeys.Add(s);
		}
		foreach(string s in rKeys){
			List<EventHandler> handlers = registry[s];
			for(int i = 0; i < handlers.Count; i++){
				EventHandler eh = handlers[i];
				if(eh.Equals (null)){
					handlers.RemoveAt(i);
					i--;
				}
			}
		}
	}
}