using UnityEngine;
using UnityEngine.UI;

public class SettingsMenu : MonoBehaviour
{
    Image fixedGunsImg;
    Text fixedGunsText;
    bool fixedGuns;

    public void Start(){
        GameObject fixed_ = GameObject.Find("FixedGuns");
        fixedGunsImg = fixed_.transform.Find("Button").GetComponent<Image>();
        fixedGunsText = fixed_.transform.Find("Button").Find("Text").GetComponent<Text>();
        fixedGuns = PlayerPrefs.GetInt("static_guns",0) == 1;
        fixedGunsImg.color = fixedGuns ? new Color(0f,216f,0f) : new Color(176f,0f,0f);
        fixedGunsText.text = fixedGuns ? "On" : "Off";
    }
    public void ToggleFixedButtons(){
        fixedGuns = !fixedGuns;
        fixedGunsImg.color = fixedGuns ? new Color(0f,216f,0f) : new Color(176f,0f,0f);
        fixedGunsText.text = fixedGuns ? "On" : "Off";
        PlayerPrefs.SetInt("static_guns",fixedGuns ? 1 : 0);
    }
}