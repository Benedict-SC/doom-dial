using UnityEngine;
using UnityEngine.UI;
using MiniJSON;
using System.IO;
using System.Collections.Generic;

public class PathEditor : MonoBehaviour{

	float onscreenRadius = 400f;

	GameObject pointParent;
	GameObject linkParent;
	GameObject center;
	void Start(){
		pointParent = GameObject.Find("Points");
		linkParent = GameObject.Find("Links");
		center = GameObject.Find("CenterAnchor");
	}

	public void SpawnNewPoint(){
		GameObject newpoint = new GameObject();
		Image img = newpoint.AddComponent<Image>() as Image;
		newpoint.transform.SetParent(pointParent.transform,false);
		newpoint.GetComponent<RectTransform>().sizeDelta = new Vector2(15f,15f);
		img.color = new Color(.8f,.8f,1f);
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
			float radius = (pos-centerpos).magnitude / onscreenRadius; //0 to 1, percent away from center
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

}
