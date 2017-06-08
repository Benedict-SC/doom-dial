/*Thom*/

using UnityEngine;
using UnityEngine.UI;
using System.IO;
using MiniJSON;
using System.Collections.Generic;

public class EnemyStatsPanel : MonoBehaviour {

    Text enName;
    Text enStats;
    Text enDesc;
    Image enSprite;

    string currentFilename; //the selected enemy's filename

    string defaultName = "???";
    string defaultStats = "[*WRITE DEFAULT STATS STRING*]";
    string defaultDesc = "Description not available.";
    string defaultImage = "Placeholder";
    //and we need something for the default image shown

    FileLoader fl;

    //for currently selected enemy
    long enTimesSeen;
    long enTimesKilled;
    long enTimesHitBy;

    // Use this for initialization
    void Start () {
        GameObject nameObj = transform.Find("Name").gameObject;
        GameObject statsObj = transform.Find("Stats").gameObject;
        GameObject descObj = transform.Find("Description").gameObject;
        GameObject imageObj = transform.Find("Image").gameObject;

        enName = nameObj.GetComponent<Text>();
        enStats = statsObj.GetComponent<Text>();
        enDesc = descObj.GetComponent<Text>();
        enSprite = imageObj.GetComponent<Image>();

        enName.text = "ERROR NAME NOT SET";
        enStats.text = "ERROR STATS NOT SET";
        enDesc.text = "ERROR DESCRIPTION NOT SET";
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void SetCurrentEnemy(string fn)
    {
        currentFilename = fn;

        //update panel info from file!
        fl = new FileLoader("JSONData" + Path.DirectorySeparatorChar + "Bestiary", fn);
        string enemyjson = fl.Read();
        Dictionary<string, System.Object> enemyStats = (Dictionary<string, System.Object>)Json.Deserialize(enemyjson);

        FileLoader logfl = FileLoader.GetSaveDataLoader("Bestiary", "bestiary_logging");
        string logjson = logfl.Read();
        Dictionary<string, System.Object> logDict = (Dictionary<string, System.Object>)Json.Deserialize(logjson);
        List<System.Object> logList = logDict["enemyLogs"] as List<System.Object>;
        foreach (System.Object enemy in logList)
        {
            Dictionary<string, System.Object> edata = enemy as Dictionary<string, System.Object>;
            string filename = edata["name"] as string;
            if (filename.Equals(fn)) //find the correct enemy corresponding to enFilename
            {
                enTimesSeen = (long)edata["timesSeen"]; //now we've finally got the timesSeen value :P
                enTimesKilled = (long)edata["timesKilled"];
                enTimesHitBy = (long)edata["timesHitBy"];
                break;
            }
        }

        UpdateName((string)enemyStats["name"]);
        UpdateDesc((string)enemyStats["desc"]);
        if (enTimesSeen > 0)
        {
            UpdateSprite((string)enemyStats["imgfile"]);
        }
        else
        {
            UpdateSprite(defaultImage);
        }
        UpdateStats(((float)(double)enemyStats["maxHP"]), ((float)(double)enemyStats["maxShields"]),
                    ((float)(double)enemyStats["damage"]), (float)enTimesKilled);
        //REMEMBER TO ADD THE REST OF THE UPDATE METHODS
        //ONCE I KNOW WHAT THESE STRINGS WILL BE
    }

    //GUIDELINES FOR STAT DISPLAY CONDITIONS

    //After being SEEN once, add its image to the bestiary
    //First time you kill an enemy, reveal its HP and base shield stat in bestiary
    //After 10 kills you get its description/ability/other stats
    //50 kills: get emblem for towers
    //100 kills: ability to use it in level editor
    //After getting hit once by an enemy, reveal its damage stat in bestiary

    void UpdateName(string name)
    {
        if (enTimesSeen > 0)
        {
            enName.text = name;
        }
        else
        {
            enName.text = defaultName;
        }
    }

    void UpdateStats(float hpValue, float shieldValue,
                     float dmgValue, float killsValue)
    {
        string shieldStat = "Shield: ???";
        string hpStat = "HP: ???";
        string dmgStat = "Attack: ???";
        string killCountStat = "No. Killed: ???";
        string iconAchieved = "Tower Icon: No";
        string makerAchieved = "In Editor: No";

        if (enTimesKilled > 0)
        {
            shieldStat = "Shield: " + shieldValue;
            hpStat = "HP: " + hpValue;
            killCountStat = "No. Killed: " + killsValue;
        }
        if (enTimesKilled >= 10)
        {
            dmgStat = "Attack: " + dmgValue;
        }
        if (enTimesKilled >= 50)
        {
            iconAchieved = "Tower Icon: Yes!";
        }
        if (enTimesKilled >= 100)
        {
            makerAchieved = "In Editor: Yes!";
        }

        enStats.text = hpStat + "\n" + shieldStat + "\n" + dmgStat + "\n" + killCountStat
                       + "\n" + iconAchieved + "\n" + makerAchieved;
    }

    void UpdateDesc(string desc)
    {
        if (enTimesSeen >= 10)
        {
            enDesc.text = desc;
        }
        else
        {
            enDesc.text = defaultDesc;
        }
    }

    void UpdateSprite(string sprFilename)
    {
        Texture2D decal = Resources.Load<Texture2D>("Sprites/EnemySprites/" + sprFilename);
        if (decal == null)
            Debug.Log("decal is null");
        enSprite.sprite = UnityEngine.Sprite.Create(
            decal,
            new Rect(0, 0, decal.width, decal.height),
            new Vector2(0.5f, 0.5f),
            100f);
    }
}
