using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuRisks : MonoBehaviour {

    public Toggle skinnyFarSectionToggle;
    public Toggle skinnyMidSectionToggle;
    public Toggle skinnyNearSectionToggle;
    public Toggle inverseDialToggle;
    public Toggle useLockToggle;
    public Toggle rotLockToggle;
    public Toggle vampireToggle;
    public Toggle ambushToggle;
    public Toggle sabotageToggle;
    public Toggle tougherEnemiesToggle;

	// Use this for initialization
	void Start () {
        skinnyFarSectionToggle.onValueChanged.AddListener(SetSkinnyFarSection);
        skinnyMidSectionToggle.onValueChanged.AddListener(SetSkinnyMidSection);
        skinnyNearSectionToggle.onValueChanged.AddListener(SetSkinnyNearSection);
        inverseDialToggle.onValueChanged.AddListener(SetInverseDial);
        useLockToggle.onValueChanged.AddListener(SetUseLock);
        rotLockToggle.onValueChanged.AddListener(SetRotLock);
        vampireToggle.onValueChanged.AddListener(SetVampire);
        ambushToggle.onValueChanged.AddListener(SetAmbush);
        sabotageToggle.onValueChanged.AddListener(SetSabotage);
        tougherEnemiesToggle.onValueChanged.AddListener(SetTougherEnemies);

        SetToggleValues();
	}
	
	// Update is called once per frame
	void Update () {
        
	}

    void SetToggleValues()
    {
        if (Int2Bool(PlayerPrefs.GetInt(PlayerPrefsInfo.s_skinnyFarLane)))
        {
            skinnyFarSectionToggle.isOn = true;
        }
        else skinnyFarSectionToggle.isOn = false;
        if (Int2Bool(PlayerPrefs.GetInt(PlayerPrefsInfo.s_skinnyMidLane)))
        {
            skinnyMidSectionToggle.isOn = true;
        }
        else skinnyMidSectionToggle.isOn = false;
        if (Int2Bool(PlayerPrefs.GetInt(PlayerPrefsInfo.s_skinnyNearLane)))
        {
            skinnyNearSectionToggle.isOn = true;
        }
        else skinnyNearSectionToggle.isOn = false;
        if (Int2Bool(PlayerPrefs.GetInt(PlayerPrefsInfo.s_inverseDialSpin)))
        {
            inverseDialToggle.isOn = true;
        }
        else inverseDialToggle.isOn = false;
        if (Int2Bool(PlayerPrefs.GetInt(PlayerPrefsInfo.s_useLock)))
        {
            useLockToggle.isOn = true;
        }
        else useLockToggle.isOn = false;
        if (Int2Bool(PlayerPrefs.GetInt(PlayerPrefsInfo.s_rotLock)))
        {
            rotLockToggle.isOn = true;
        }
        else skinnyFarSectionToggle.isOn = false;
        if (Int2Bool(PlayerPrefs.GetInt(PlayerPrefsInfo.s_vampire)))
        {
            vampireToggle.isOn = true;
        }
        else vampireToggle.isOn = false;
        if (Int2Bool(PlayerPrefs.GetInt(PlayerPrefsInfo.s_ambush)))
        {
            ambushToggle.isOn = true;
        }
        else ambushToggle.isOn = false;
        if (Int2Bool(PlayerPrefs.GetInt(PlayerPrefsInfo.s_sabotage)))
        {
            sabotageToggle.isOn = true;
        }
        else sabotageToggle.isOn = false;
        if (Int2Bool(PlayerPrefs.GetInt(PlayerPrefsInfo.s_tougherEnemies)))
        {
            tougherEnemiesToggle.isOn = true;
        }
        else tougherEnemiesToggle.isOn = false;
    }

    void SetSkinnyFarSection(bool b)
    {
        PlayerPrefs.SetInt(PlayerPrefsInfo.s_skinnyFarLane, Bool2Int(b));
        PlayerPrefsInfo.CalculateDropRateBoost();
    }

    void SetSkinnyMidSection(bool b)
    {
        PlayerPrefs.SetInt(PlayerPrefsInfo.s_skinnyMidLane, Bool2Int(b));
        PlayerPrefsInfo.CalculateDropRateBoost();
    }

    void SetSkinnyNearSection(bool b)
    {
        PlayerPrefs.SetInt(PlayerPrefsInfo.s_skinnyNearLane, Bool2Int(b));
        PlayerPrefsInfo.CalculateDropRateBoost();
    }

    void SetInverseDial(bool b)
    {
        PlayerPrefs.SetInt(PlayerPrefsInfo.s_inverseDialSpin, Bool2Int(b));
        PlayerPrefsInfo.CalculateDropRateBoost();
    }

    void SetUseLock(bool b)
    {
        PlayerPrefs.SetInt(PlayerPrefsInfo.s_useLock, Bool2Int(b));
        PlayerPrefsInfo.CalculateRarityRateBoost();
    }

    void SetRotLock(bool b)
    {
        PlayerPrefs.SetInt(PlayerPrefsInfo.s_rotLock, Bool2Int(b));
        PlayerPrefsInfo.CalculateRarityRateBoost();
    }

    void SetVampire(bool b)
    {
        PlayerPrefs.SetInt(PlayerPrefsInfo.s_vampire, Bool2Int(b));
        PlayerPrefsInfo.CalculateRarityRateBoost();
    }

    void SetAmbush(bool b)
    {
        PlayerPrefs.SetInt(PlayerPrefsInfo.s_ambush, Bool2Int(b));
        PlayerPrefsInfo.CalculateDropRateBoost();
    }

    void SetSabotage(bool b)
    {
        PlayerPrefs.SetInt(PlayerPrefsInfo.s_sabotage, Bool2Int(b));
        PlayerPrefsInfo.CalculateDropRateBoost();
    }

    void SetTougherEnemies(bool b)
    {
        PlayerPrefs.SetInt(PlayerPrefsInfo.s_tougherEnemies, Bool2Int(b));
        PlayerPrefsInfo.CalculateOmnitechRateBoost();
    }

    static int Bool2Int(bool b)
    {
        if (b) return 1;
        else return 0;
    }

    static bool Int2Bool(int i)
    {
        if (i == 0) return false;
        else return true;
    }
}
