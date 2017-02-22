using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections.Generic;

public class GameWatcher : MonoBehaviour, EventHandler{

	Dictionary<string,float> enemyDamages;
	Dictionary<string,int> pieceCounts;
	GameObject winPanel = null;
	GameObject losePanel = null;

	private static GameWatcher self = null;
	void Start(){ 
		//if(self == null){ //there's only one, but it might disappear and need to be recreated
			self = this;
			EventManager.Instance().RegisterForEventType("enemy_finished",self);
			EventManager.Instance().RegisterForEventType("boss_hit",self);
			EventManager.Instance().RegisterForEventType("piece_obtained",self);
			EventManager.Instance().RegisterForEventType("game_over",self);
            EventManager.Instance().RegisterForEventType("you_won", self);
			enemyDamages = new Dictionary<string,float>();
			pieceCounts = new Dictionary<string,int>();
			winPanel = GameObject.Find("WinPanel");
			losePanel = GameObject.Find("GameOverPanel");
		//}
	}
	public static GameWatcher Instance(){
		return self;
	}

	public void HandleEvent(GameEvent ge){
		if(ge.type.Equals("enemy_finished") || ge.type.Equals("boss_hit")){
			string key = (string)ge.args [0];
			float damage = (float)ge.args[1];

			if (ge.type.Equals("boss_hit")) {
				key = "BOSSLOG" + key; //bosses don't have filenames, so mark this entry as special
			}

			if(enemyDamages.ContainsKey(key)){ //if this is an enemy type that's already hit
				float newdmg = damage + enemyDamages[key]; //add new damage to old damage
				enemyDamages[key] = newdmg; //update
			}else{ //a new enemy type
				enemyDamages.Add(key,damage); //make new entry
			}
		}else if(ge.type.Equals("piece_obtained")){
			string key = (string)ge.args[0];
			if(pieceCounts.ContainsKey(key)){
				pieceCounts[key]++;
			}else{
				pieceCounts.Add(key,1);
			}
		}else if(ge.type.Equals("game_over")){
			GameOver();
		}else if (ge.type.Equals("you_won")) {
            YouWon();
        }
	}

	void GameOver(){
		losePanel.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
	}
	public void YouWon(){
		winPanel.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
	}
	public void EditTowers(){
		Application.LoadLevel("TowerSelect");
	}
	public void MainMenu(){
		Application.LoadLevel("MainMenu");
	}
	public void ReplayLevel(){
		Application.LoadLevel("MainGameCanvas");
	}
	public void NextLevel(){
		WorldData.LoadNextLevel();
		Application.LoadLevel("MainGameCanvas");
	}



}
