using UnityEngine;

public class WarnBeforeChangingSceneMenuOption : MenuOption{

		public string sceneName = "DebugLoader";
		InGameMenu warningController = null;

		public override void WhenChosen (){
			if (warningController == null) {
				Debug.LogError("warning controller not set!");
				return;
			}
			warningController.DisplayWarning (sceneName);
		}
		public void SetWarningController(InGameMenu igm){
			warningController = igm;
		}
}

