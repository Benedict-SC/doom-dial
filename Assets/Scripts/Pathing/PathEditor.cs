using UnityEngine;
using UnityEngine.UI;
using MiniJSON;
using System.IO;
using System.Collections.Generic;

public class PathEditor : MonoBehaviour{

	float onscreenRadius = 400f;
	
	int pointCount = 0;

	GameObject pointParent;
	GameObject linkParent;
	GameObject previewParent;
	GameObject center;
	
	GameObject savepanel;
	InputField saveText;
	GameObject error;
	
	Steering ai;
	
	MoverToPathConverter mtpc;
	
	void Start(){
		pointParent = GameObject.Find("Points");
		linkParent = GameObject.Find("Links");
		previewParent = GameObject.Find("PreviewPoints");
		center = GameObject.Find("CenterAnchor");
		savepanel = GameObject.Find ("SavePanel");
		saveText = savepanel.transform.FindChild("InputField").GetComponent<InputField>();
		error = savepanel.transform.Find("Invalid").gameObject;
		error.SetActive(false);
		
		ai = GameObject.Find("AI").GetComponent<Steering>();
		
		mtpc = GetComponent<MoverToPathConverter>();
	}
	
	public void SpawnNewPointVoid(){
		GameObject newpoint = new GameObject();
		Image img = newpoint.AddComponent<Image>() as Image;
		newpoint.transform.SetParent(pointParent.transform,false);
		newpoint.GetComponent<RectTransform>().sizeDelta = new Vector2(15f,15f);
		img.color = new Color(.8f,.8f,1f);
		pointCount++;
		newpoint.name = "Point" + pointCount;
	}
	public GameObject SpawnNewPoint(){
		GameObject newpoint = new GameObject();
		Image img = newpoint.AddComponent<Image>() as Image;
		newpoint.transform.SetParent(pointParent.transform,false);
		newpoint.GetComponent<RectTransform>().sizeDelta = new Vector2(15f,15f);
		img.color = new Color(.8f,.8f,1f);
		pointCount++;
		newpoint.name = "Point" + pointCount;
		return newpoint;
	}
	public void DisconnectPoints(){
		List<GameObject> links = GetLinks ();
		foreach(GameObject link in links){
			Destroy (link);
		}
	}
	public void ConnectPoints(){
		List<GameObject> points = GetPoints();
		for(int i = 0; i < points.Count -1;i++){
			GameObject pointA = points[i];
			GameObject pointB = points[i+1];
			
			GameObject newlink = new GameObject();
			Image img = newlink.AddComponent<Image>() as Image;
			PathLink pl = newlink.AddComponent<PathLink>() as PathLink;
			newlink.transform.SetParent(linkParent.transform,false);
			pl.pointA = pointA;
			pl.pointB = pointB;
		}
	}
	public void ClearPath(){
		List<GameObject> points = GetPoints();
		foreach(GameObject point in points){
			Destroy (point);
		}
		DisconnectPoints();
		pointCount = 0;
	}
	List<GameObject> GetPoints(){
		List<GameObject> points = new List<GameObject>();
		for(int i=0;i<pointParent.transform.childCount;i++){
			points.Add (pointParent.transform.GetChild(i).gameObject);
		}
		return points;
	}
	List<GameObject> GetLinks(){
		List<GameObject> links = new List<GameObject>();
		for(int i=0;i<linkParent.transform.childCount;i++){
			links.Add (linkParent.transform.GetChild(i).gameObject);
		}
		return links;
	}
	
	Dictionary<string,System.Object> GetPathDict(){
		Dictionary<string,System.Object> result = new Dictionary<string,System.Object>();
		List<System.Object> pointlist = new List<System.Object>();
		List<GameObject> points = GetPoints();
		Vector2 centerpos = center.GetComponent<RectTransform>().anchoredPosition;
		foreach(GameObject point in points){
			Vector2 pos = point.GetComponent<RectTransform>().anchoredPosition;
			float theta = Mathf.Atan2 (pos.y-centerpos.y,pos.x-centerpos.x) - (Mathf.PI/2f);
			if(theta < 0.001f && theta > -0.001f)
				theta = 0.0f;
			float radius = (pos-centerpos).magnitude / onscreenRadius; //0 to 1, percent away from center
			if(radius < 0.001f && radius > -0.001f)
				radius = 0.0f;
			Dictionary<string,System.Object> pointobj = new Dictionary<string,System.Object>();
			pointobj.Add("theta",theta);
			pointobj.Add("radius",radius);
			
			pointlist.Add(pointobj);
		}
		result.Add("polarPoints",pointlist);
		return result;
	}
	public void PrintPathJSON(){
		Debug.Log (Json.Serialize(GetPathDict()));
	}
	public void SavePathJSON(string filename){
		FileLoader dest = new FileLoader (Application.persistentDataPath,"Paths",filename);
		dest.Write(Json.Serialize(GetPathDict()));
	}
	public void ShowPath(float angle){
		string json = Json.Serialize(GetPathDict ());
		AIPath p = new AIPath(json);
		p.SetAngle(angle);
		p.SetCenter(center.GetComponent<RectTransform>().anchoredPosition);
		p.SetTrackRadius(onscreenRadius);
		List<Vector2> pathpoints = p.GetPathAsListOfVectors();
		for(int i = 0; i < pathpoints.Count;i++){
			Vector2 pointA = pathpoints[i];
			
			GameObject newpreview = new GameObject();
			newpreview.transform.SetParent(previewParent.transform,false);
			Image img = newpreview.AddComponent<Image>() as Image;
			RectTransform prt = newpreview.GetComponent<RectTransform>();
			prt.sizeDelta = new Vector2(12f,12f);
			img.color = new Color(1f,.7f,.8f);
			prt.anchoredPosition = pointA;
		}
	}
	public void FollowPath(){
		string json = Json.Serialize(GetPathDict ());
		AIPath p = new AIPath(json);
		p.SetContext(center.GetComponent<RectTransform>().anchoredPosition,0f,onscreenRadius);
		ai.StartFollowingPath(p);
	}
	public void GeneratePathFromMover(){
		mtpc.ProducePath(this);
	}
	public void GeneratePathFromVectorArray(Vector2[] pointset){
		for(int i = 0; i < pointset.Length; i++){
			GameObject point = SpawnNewPoint();
			point.transform.SetParent(center.transform,false);
			point.GetComponent<RectTransform>().anchoredPosition = pointset[i]*2f*(1f/.82222f);//scaled to editor size
			point.transform.SetParent(pointParent.transform);
			point.GetComponent<RectTransform>().anchoredPosition -= new Vector2(0f,54f); //not sure why it's offset wrong
		}
		ConnectPoints();
	}
	
	public void OpenSaveScreen(){
		savepanel.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
	}
	public void CloseSaveScreen(){
		error.SetActive(false);
		savepanel.GetComponent<RectTransform>().anchoredPosition = new Vector2(-570f,0f);
	}
	public void SavePath(){
		string filename = saveText.text;
		if(filename.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0 || filename.Length == 0){
			error.SetActive(true);
			return;
		}else{
			CloseSaveScreen();
			SavePathJSON(filename);
			return;
		}
	}

}
