using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using MiniJSON;

public class LoadPanel : MonoBehaviour{
	
    List<string> filenames;
    WaveEditorController wec;
    public string pickedFilename;
    RectTransform contentRT;
    float minHeight;
    readonly int optionHeight = 57;
	public void Start(){
		wec = GameObject.Find("EditorManager").GetComponent<WaveEditorController>();
        contentRT = GameObject.Find("LevelContent").GetComponent<RectTransform>();
        minHeight = contentRT.sizeDelta.y;
        FillNames();
	}
	public void Update(){
		
	}
    public void Dismiss(){
        GetComponent<RectTransform>().anchoredPosition = new Vector2(0f,500f);
        wec.panelOpen = false;
    }
    public void Summon(){
        GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
        FillNames();
        wec.panelOpen = true;
    }
    public void FillNames(){
        //first remove old stuff
        List<Transform> kids = new List<Transform>();
        foreach(Transform t in contentRT){
            kids.Add(t);
        }
        for(int i = kids.Count - 1; i >= 0; i--){
			Destroy(kids[i].gameObject);
		}

        //now load stuff
        FileLoader levelRegistry = new FileLoader (Application.persistentDataPath,"UserLevels","levelRegistry");
		string contents = levelRegistry.Read();
        filenames = new List<string>();
		if(contents.Equals("ERROR")){
			
		}else{
			Dictionary<string,System.Object> registry = Json.Deserialize (contents) as Dictionary<string,System.Object>;
			List<System.Object> levelsList = registry ["levels"] as List<System.Object>;
            float newheight = levelsList.Count * optionHeight;
            if(minHeight > newheight){
                newheight = minHeight;
            }
            contentRT.sizeDelta = new Vector2(contentRT.sizeDelta.x,newheight);
			for(int i=0;i<levelsList.Count;i++){
				string s = (string)levelsList[i];
                filenames.Add(s);
                GameObject option = Instantiate (Resources.Load ("Prefabs/WaveMenuOption")) as GameObject;
                option.transform.Find("Text").GetComponent<Text>().text = s;
                option.GetComponent<RectTransform>().anchoredPosition = new Vector2(0,-i*(optionHeight+1));
                option.transform.SetParent(contentRT,false);
			}
		}
    }
    public void Load(){
        wec.LoadLevel(pickedFilename);
        Dismiss();
    }
    public void Play(){
        wec.PlaySpecificLevel(pickedFilename);
    }
    public void PlayNow(){
        wec.PlayLevel();
    }

}
