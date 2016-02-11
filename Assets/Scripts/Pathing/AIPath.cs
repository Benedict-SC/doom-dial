using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.IO;
using MiniJSON;

public class AIPath{

	class Node{//currently just a wrapper for a Vector2- may contain special attributes later
		public Vector2 pos;
		public Node(float x,float y){
			pos = new Vector2(x,y);
		}
		public Node(Vector2 npos){
			pos = npos;
		}
	}
	
	List<Node> nodes;
	List<Vector2> polarNodes; //x and y are theta and radius
	
	Vector2 center = Vector2.zero;
	float angleInDegrees = 0f; //counterclockwise from y axis
	float trackRadius = 1f;
	
	public AIPath(string json){
		polarNodes = new List<Vector2>();
		Dictionary<string,System.Object> pathdict = Json.Deserialize(json) as Dictionary<string,System.Object>;
		List<System.Object> polarPoints = pathdict["polarPoints"] as List<System.Object>;
		foreach(System.Object o in polarPoints){
			Dictionary<string,System.Object> pointdict = o as Dictionary<string,System.Object>;
			float theta = (float)(double)pointdict["theta"];
			float radius = (float)(double)pointdict["radius"];
			polarNodes.Add(new Vector2(theta,radius));
		}
		polarNodes.Add(Vector2.zero);
	}
	
	#region Calibration (handling data that converts polar to canvas coordinates)
	public void RefreshNodeValues(){
		nodes = new List<Node>();
		foreach(Vector2 polarCoords in polarNodes){
			float mathTheta = polarCoords.x + (Mathf.PI/2f);//to mathf rotation from unity rotation
			mathTheta += Mathf.Deg2Rad * angleInDegrees;//offset by where the path is
			float x = Mathf.Cos(mathTheta) * polarCoords.y * trackRadius;
			float y = Mathf.Sin(mathTheta) * polarCoords.y * trackRadius;
			Node n = new Node(x+center.x,y+center.y);//offset by center
			nodes.Add(n);
		}
	}
	public void SetCenter(Vector2 ncenter){
		center = ncenter;
		RefreshNodeValues();
	}
	public void SetAngle(float degreesCCFromYAxis){
		angleInDegrees = degreesCCFromYAxis;
		RefreshNodeValues();
	}
	public void SetTrackRadius(float tr){
		trackRadius = tr;
		RefreshNodeValues();
	}
	public void SetContext(Vector2 ncenter, float degreesCCFromYAxis, float tr){
		center = ncenter;
		angleInDegrees = degreesCCFromYAxis;
		trackRadius = tr;
		RefreshNodeValues();
	}
	public void SetDialDimensions(Vector2 ncenter, float tr){
		center = ncenter;
		trackRadius = tr;
		RefreshNodeValues();
	}
	public float GetAngle(){
		return angleInDegrees;
	}
	#endregion

	#region Navigation (returning information about nodes to things that need it)
	Node ClosestNodeTo(Vector2 location){ //assumes at least one node
		float lowestDistance = 100000f;
		Node closest = nodes[0];
		foreach(Node n in nodes){
			Vector2 gap = n.pos - location;
			float dist = gap.magnitude;
			if(dist < lowestDistance){
				lowestDistance = dist;
				closest = n;
			}
		}
		return closest;
	}
	public Vector2 ClosestPointTo(Vector2 location){
		return ClosestNodeTo(location).pos;
	}
	public Vector2 PathFollowingTarget(Vector2 location, int lookahead){
		Node n = ClosestNodeTo(location);
		int nodecode = nodes.IndexOf(n);
		//Debug.Log ("orig nodecode: " + nodecode);
		nodecode += lookahead;
		if(nodecode >= nodes.Count)
			nodecode = nodes.Count - 1;
		//Debug.Log ("2nd nodecode: " + nodecode);
		return nodes[nodecode].pos;
	}
	#endregion
	
	public List<Vector2> GetPathAsListOfVectors(){
		List<Vector2> result = new List<Vector2>();
		foreach(Node n in nodes){
			result.Add(n.pos);
		}
		return result;
	}
	
	public static AIPath CreatePathFromJSONFilename(string filename){
		FileLoader pathsrc = new FileLoader ("JSONData" + Path.DirectorySeparatorChar + "Paths",filename);
		string json = pathsrc.Read();
		//Debug.Log(json);
		return new AIPath(json);
	}
	
}