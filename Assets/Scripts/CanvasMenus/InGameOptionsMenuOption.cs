using UnityEngine;

public class InGameOptionsMenuOption : MenuOption{

		InGameMenu optionsController = null;

		public override void WhenChosen (){
			if (optionsController == null) {
				Debug.LogError("options controller not set!");
				return;
			}
			optionsController.SummonOptions();
		}
		public void SetOptionsController(InGameMenu igm){
			optionsController = igm;
		}
}


