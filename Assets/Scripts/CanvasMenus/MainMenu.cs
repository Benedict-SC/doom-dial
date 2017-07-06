using UnityEngine;
using UnityEngine.UI;
using System.IO;
using MiniJSON;
using System.Collections.Generic;

public class MainMenu : MonoBehaviour
{

    MenuDial md;
    public Text buttonText;
    MenuOption selected = null;

    public void Awake()
    {
        md = GameObject.Find("MenuDial").gameObject.GetComponent<MenuDial>();
    }
    public void Start()
    {
        GameObject playobj = GameObject.Instantiate(Resources.Load("Prefabs/Menus/MenuOption")) as GameObject;
        GameObject.Destroy(playobj.GetComponent<MenuOption>());
        LoadSceneMenuOption play = playobj.AddComponent<LoadSceneMenuOption>() as LoadSceneMenuOption;
        play.sceneName = "WorldSelectCanvas";
        play.ConfigureOption("Menu_Play", "", "Start the game!");
        playobj.transform.SetParent(md.transform, false);
        play.SizeImage(80f);
        md.AddOption(play);

        GameObject wavemakerobj = GameObject.Instantiate(Resources.Load("Prefabs/Menus/MenuOption")) as GameObject;
        GameObject.Destroy(wavemakerobj.GetComponent<MenuOption>());
        LoadSceneMenuOption wavemaker = wavemakerobj.AddComponent<LoadSceneMenuOption>() as LoadSceneMenuOption;
        wavemaker.sceneName = "WaveEditorAdmin";
        wavemaker.ConfigureOption("Menu_WaveMaker", "", "Build a custom wave!");
        wavemakerobj.transform.SetParent(md.transform, false);
        wavemaker.SizeImage(80f);
        md.AddOption(wavemaker);

        GameObject settingsobj = GameObject.Instantiate(Resources.Load("Prefabs/Menus/MenuOption")) as GameObject;
        GameObject.Destroy(settingsobj.GetComponent<MenuOption>());
        LoadSceneMenuOption settings = settingsobj.AddComponent<LoadSceneMenuOption>() as LoadSceneMenuOption;
        settings.sceneName = "SettingsMenu";
        settings.ConfigureOption("Menu_Settings", "", "Tweak the game options!");
        settingsobj.transform.SetParent(md.transform, false);
        settings.SizeImage(80f);
        md.AddOption(settings);

        GameObject risksobj = GameObject.Instantiate(Resources.Load("Prefabs/Menus/MenuOption")) as GameObject;
        GameObject.Destroy(risksobj.GetComponent<MenuOption>());
        LoadSceneMenuOption risks = risksobj.AddComponent<LoadSceneMenuOption>() as LoadSceneMenuOption;
        risks.sceneName = "RiskMenu";
        risks.ConfigureOption("Menu_Risks", "", "Turn up the heat!");
        risksobj.transform.SetParent(md.transform, false);
        risks.SizeImage(80f);
        md.AddOption(risks);

        GameObject towerobj = GameObject.Instantiate(Resources.Load("Prefabs/Menus/MenuOption")) as GameObject;
        GameObject.Destroy(towerobj.GetComponent<MenuOption>());
        LoadSceneMenuOption tower = towerobj.AddComponent<LoadSceneMenuOption>() as LoadSceneMenuOption;
        tower.sceneName = "TowerSelect";
        tower.ConfigureOption("Menu_Towers", "", "Arm your towers!");
        towerobj.transform.SetParent(md.transform, false);
        tower.SizeImage(80f);
        md.AddOption(tower);

        GameObject bestiaryobj = GameObject.Instantiate(Resources.Load("Prefabs/Menus/MenuOption")) as GameObject;
        GameObject.Destroy(bestiaryobj.GetComponent<MenuOption>());
        LoadSceneMenuOption bestiary = bestiaryobj.AddComponent<LoadSceneMenuOption>() as LoadSceneMenuOption;
        bestiary.sceneName = "EnemyIndex";
        bestiary.ConfigureOption("Menu_Library", "", "Know your enemy!");
        bestiaryobj.transform.SetParent(md.transform, false);
        bestiary.SizeImage(80f);
        md.AddOption(bestiary);

        md.RearrangeOptions();
        foreach(MenuOption mo in md.GetOptions())
        {
            mo.GetComponent<MenuScaleEffectCanvas>().RefreshRotOffset();
        }
    }
    public void Update()
    {

        MenuOption newselected = md.GetSelectedOption();
        if (newselected != selected)
        {
            selected = newselected;
            buttonText.text = selected.GetButtonText();
        }
    }
}
