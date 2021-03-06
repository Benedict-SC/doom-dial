using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using MiniJSON;
using System.Collections.Generic;
using System.IO;

public class Drop : MonoBehaviour{
	
	int rarity = 0;
	float superRareChance = 0.1f;
	List<string> pieceTypes;
    List<string> omnitechTypes;

    RectTransform rt;

    //Sabotage risk stuff
    float sabotageTime = 2f; //how long until piece explodes
    float sabotageAoeScale = 5f;
    float sabotageAoeDamage = 30f;
    bool sabotageIsOn = false;
    bool timerIsSet = false;
    Timer explodeTimer;

    //Tougher Enemies risk / omnitech stuff
    bool tougherEnemiesIsOn = false;
    float omniBoost = 0f;
    bool mustBeOmnitech;
	
	// Use this for initialization
	void Start () {
        //Debug.Log("Drop.cs called Start()");
        rt = GetComponent<RectTransform>();
        if (PlayerPrefsInfo.Int2Bool(PlayerPrefs.GetInt(PlayerPrefsInfo.s_sabotage)))
        {
            sabotageIsOn = true;
        }
        if (PlayerPrefsInfo.Int2Bool(PlayerPrefs.GetInt(PlayerPrefsInfo.s_tougherEnemies)))
        {
            tougherEnemiesIsOn = true;
            omniBoost = PlayerPrefsInfo.GetOmnitechDropRateBoost(); //value between 0 and 1
        }

        omnitechTypes = new List<string> { "cooldown", "energyGain", "comboKey", "selfRepair" };
	}
	
	// Update is called once per frame
	void Update () {
		if (sabotageIsOn)
        {
            if (!timerIsSet)
            {
                explodeTimer = new Timer();
                timerIsSet = true;
            }
            else
            {
                //Debug.Log("current timer: " + explodeTimer.TimeElapsedSecs());
                if (explodeTimer.TimeElapsedSecs() > sabotageTime)
                {
                    Explode();
                }
            }
        }
	}
	
	public void SetTypes(string filename){
		FileLoader fl = new FileLoader ("JSONData" + Path.DirectorySeparatorChar + "Bestiary",filename);
		string json = fl.Read ();
		Dictionary<string,System.Object> data = (Dictionary<string,System.Object>)Json.Deserialize (json);
		List<System.Object> castable = data["pieceTypes"] as List<System.Object>;
		pieceTypes = new List<string>();
		foreach(System.Object s in castable){
			pieceTypes.Add((string)s);
		}
	}
	public void MakeRare(){
		System.Random r = new System.Random ();
		if(r.NextDouble() > superRareChance){
			rarity = 1;
			Image img = gameObject.GetComponent<Image> ();
			Texture2D decal = Resources.Load<Texture2D> ("Sprites/" + "PieceRareDrop");
			if (decal == null) {
				Debug.Log("decal is null");
			}
			img.sprite = UnityEngine.Sprite.Create (
				decal,
				new Rect(0,0,decal.width,decal.height),
				new Vector2(0.5f,0.5f),
				img.sprite.rect.width/img.sprite.bounds.size.x);
		}else{
			rarity = 2;
			Image img = gameObject.GetComponent<Image> ();
			Texture2D decal = Resources.Load<Texture2D> ("Sprites/" + "PieceSuperRareDrop");
			if (decal == null) {
				Debug.Log("decal is null");
			}
			img.sprite = UnityEngine.Sprite.Create (
				decal,
				new Rect(0,0,decal.width,decal.height),
				new Vector2(0.5f,0.5f),
				img.sprite.rect.width/img.sprite.bounds.size.x);
		}
	}

    //forces this drop to be an omnitech
    public void MakeOmnitech()
    {
        mustBeOmnitech = true;
    }

	public void AddPieceToInventory(){
		//decide on a piece to get
		string piecetype = "damage"; //placeholder
		string piecerarity = "_normal";
		if(rarity == 0){
			piecerarity = "_normal";
		}else if(rarity == 1){
			piecerarity = "_rare";
		}else if(rarity == 2){
			piecerarity = "_super";
		}
		System.Random r = new System.Random ();
		double dindex = r.NextDouble() * pieceTypes.Count;
		int index = (int)dindex;
		if(index == pieceTypes.Count){
			index--;
		}
		piecetype = pieceTypes[index];

        //Omnitech chance if a relevant Risk is on
        if (tougherEnemiesIsOn)
        {
            float rando = UnityEngine.Random.value;
            if (mustBeOmnitech || rando <= omniBoost)
            {
                Debug.Log("dropping an omnitech piece due to enemy stat or Risk reward");
                //choose a random omnitech piece
                double drando = r.NextDouble() * omnitechTypes.Count;
                int oIndex = (int)drando;
                if (oIndex == omnitechTypes.Count)
                {
                    oIndex--;
                }
                piecetype = omnitechTypes[oIndex];
            }
        }

		Debug.Log("type is " + piecetype + piecerarity);
		
		FileLoader fl = new FileLoader (Application.persistentDataPath,"Inventory","inventory");
		string json = fl.Read ();
		Dictionary<string,System.Object> data = (Dictionary<string,System.Object>)Json.Deserialize (json);
		List<System.Object> pieces = data["pieces"] as List<System.Object>;
		foreach(System.Object piece in pieces){
			Dictionary<string,System.Object> pdata = piece as Dictionary<string,System.Object>;
			string filename = pdata["filename"] as string;
			if(filename.Equals(piecetype + piecerarity)){
				int count = (int)(long)pdata["owned"];
				count++;
				pdata["owned"] = (long)count;
				break;
			}
		}
		
		string filedata = Json.Serialize(data);
		fl.Write(filedata);

		GameEvent ge = new GameEvent("piece_obtained");
		ge.addArgument(piecetype + piecerarity);
		EventManager.Instance().RaiseEvent(ge);
	}

    void Explode()
    {
        GameObject splashCircle = Instantiate(Resources.Load("Prefabs/MainCanvas/SplashCircle")) as GameObject;
        splashCircle.GetComponent<RectTransform>().anchoredPosition = rt.anchoredPosition;
        splashCircle.transform.SetParent(Dial.spawnLayer.transform, false);
        AoE ac = splashCircle.GetComponent<AoE>();
        ac.scale = sabotageAoeScale;
        ac.aoeDamage = sabotageAoeDamage;
        ac.parent = "PieceDrop";
        ac.canDamageDial = true;
        Destroy(gameObject);
    }

	public int GetRarity(){
		return rarity;
	}
}

