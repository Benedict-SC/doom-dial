using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using MiniJSON;

public class SavePanel : MonoBehaviour{
	
    InputField textbox;
    Text filenamesDisplay;
    List<string> filenames;
    WaveEditorController wec;
	public void Start(){
		textbox = transform.Find("LevelNameField").GetComponent<InputField>();
        filenamesDisplay = transform.Find("FilenameList").GetComponent<Text>();
		wec = GameObject.Find("EditorManager").GetComponent<WaveEditorController>();
	}
	public void Update(){
		
	}
    public void Dismiss(){
        GetComponent<RectTransform>().anchoredPosition = new Vector2(0f,500f);
        textbox.text = "";
        wec.panelOpen = false;
    }
    public void Summon(){
        GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
        wec.panelOpen = true;
    }
    public void FillNames(){
        FileLoader levelRegistry = new FileLoader (Application.persistentDataPath,"UserLevels","levelRegistry");
		string contents = levelRegistry.Read();
        filenames = new List<string>();
        filenamesDisplay.text = "";
		if(contents.Equals("ERROR")){
			filenamesDisplay.text = "[no saved levels found]";
		}else{
			Dictionary<string,System.Object> registry = Json.Deserialize (contents) as Dictionary<string,System.Object>;
			List<System.Object> levelsList = registry ["levels"] as List<System.Object>;
			for(int i=0;i<levelsList.Count;i++){
				string s = (string)levelsList[i];
				filenamesDisplay.text += s + "\n";
                filenames.Add(s);
			}
		}
    }
    public void Save(){
        wec.SaveLevel(textbox.text);
        Dismiss();
    }
    public void Load(){
        if(textbox.text.Equals("")){
            wec.PlayLevel();
        }else if(!filenames.Contains(textbox.text)){
            Debug.Log("invalid filename");
        }else{
            wec.PlaySpecificLevel(textbox.text);
        }
    }

}
