using UnityEngine;
using UnityEngine.UI;
using System.IO;
using MiniJSON;
using System.Collections.Generic;

public class InGameMenu : MonoBehaviour{

		MenuDial md;
		public Text buttonText;
		MenuOption selected = null;

		GameObject warningPanel = null;
		Vector2 warningLoc = Vector2.zero;
		GameObject optionsPanel = null;
		Vector2 optionsLoc = Vector2.zero;

		string transitionSceneName = "DebugLoader";

		public void Awake(){
				md = GameObject.Find("MenuDial").gameObject.GetComponent<MenuDial>();
				warningPanel = GameObject.Find("Warning");
				warningLoc = warningPanel.GetComponent<RectTransform>().anchoredPosition;
				optionsPanel = GameObject.Find("Options");
				optionsLoc = optionsPanel.GetComponent<RectTransform>().anchoredPosition;
		}
		public void Start(){
				FillMenu();
		}
		public void Update(){

				MenuOption newselected = md.GetSelectedOption();
				if(newselected != selected){
						selected = newselected;
						buttonText.text = selected.GetButtonText();
				}
		}
		public void FillMenu ()
		{

				for (int i = 0; i < 2; i++) {
						//main menu
						GameObject mainmenu = GameObject.Instantiate (Resources.Load ("Prefabs/Menus/MenuOption")) as GameObject;
						GameObject.Destroy (mainmenu.GetComponent<MenuOption> ());
						WarnBeforeChangingSceneMenuOption mainmenuwbcsmo = mainmenu.AddComponent<WarnBeforeChangingSceneMenuOption> () as WarnBeforeChangingSceneMenuOption;
						mainmenuwbcsmo.SetWarningController (this);
						mainmenuwbcsmo.sceneName = "MainMenu";
						mainmenuwbcsmo.ConfigureOption ("Dial", "Main Menu", "Quit your current game and go back to the main menu.");
						mainmenu.transform.SetParent (md.transform, false);
						md.AddOption (mainmenuwbcsmo);
						//edit towers
						GameObject teditobj = GameObject.Instantiate (Resources.Load ("Prefabs/Menus/MenuOption")) as GameObject;
						GameObject.Destroy (teditobj.GetComponent<MenuOption> ());
						WarnBeforeChangingSceneMenuOption tedit = teditobj.AddComponent<WarnBeforeChangingSceneMenuOption> () as WarnBeforeChangingSceneMenuOption;
						tedit.SetWarningController (this);
						tedit.sceneName = "TowerSelect";
						tedit.ConfigureOption ("Tower6", "Edit Towers", "Quit your current game and head to the tower editor to change your abilities.");
						teditobj.transform.SetParent (md.transform, false);
						md.AddOption (tedit);
						//options
						GameObject options = GameObject.Instantiate (Resources.Load ("Prefabs/Menus/MenuOption")) as GameObject;
						GameObject.Destroy (options.GetComponent<MenuOption> ());
						InGameOptionsMenuOption igomo = options.AddComponent<InGameOptionsMenuOption> () as InGameOptionsMenuOption;
						igomo.SetOptionsController (this);
						igomo.ConfigureOption ("PieceNormalDrop", "Options", "Change your game's settings without leaving the current game.");
						options.transform.SetParent (md.transform, false);
						md.AddOption (igomo);
						//return to game
						GameObject unpause = GameObject.Instantiate (Resources.Load ("Prefabs/Menus/MenuOption")) as GameObject;
						GameObject.Destroy (unpause.GetComponent<MenuOption> ());
						ReturnToGameOption rtgo = unpause.AddComponent<ReturnToGameOption> () as ReturnToGameOption;
						rtgo.ConfigureOption ("EnemyBase", "Return to Game", "Leave this menu and resume your current game.");
						rtgo.transform.SetParent (md.transform, false);
						md.AddOption (rtgo);
				}
		}
		public void DisplayWarning(string sceneToTransitionTo){
			transitionSceneName = sceneToTransitionTo;
			warningPanel.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
			//Debug.Log("warning displayed");
		}
		public void DismissWarning(){
			warningPanel.GetComponent<RectTransform>().anchoredPosition = warningLoc;
		}
		public void LeaveScene(){
			GamePause.paused = false;
			Pause.paused = false;
			Application.LoadLevel(transitionSceneName);
		}
		public void SummonOptions(){
			optionsPanel.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
			//Debug.Log("options summoned");
		}
		public void DismissOptions(){
			optionsPanel.GetComponent<RectTransform>().anchoredPosition = optionsLoc;
		}
}