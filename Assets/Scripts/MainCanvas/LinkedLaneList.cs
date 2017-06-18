using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LinkedLaneList : MonoBehaviour {

    Node laneZero; //the "root" - lane 0 of the dial
    Node finalNode; //the end of the list
    int N = 0; //size of list
    List<int> ids; //list of all the ids contained in the list

	// Use this for initialization
	void Awake () {
        SetUpList();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public class Node
    {
        public Node next;
        public Node prev;
        public int id;

        public Node(int i)
        {
            id = i;
        }
    }

    //adds a new node with an id of i to the end of the list
    public void AddNode(int i)
    {
        if (laneZero == null)
        {
            laneZero = new Node(i);
            finalNode = laneZero;
        }
        else
        {
            Node n = new Node(i);
            finalNode.next = n;
            n.prev = finalNode;
            finalNode = n;
        }
        ids.Add(i);
        N++;
    }

    //sets up this LLL with the necessary info
    public void SetUpList()
    {
        //Debug.Log("running lll setup");
        ids = new List<int>();
        //Debug.Log(ids == null);
        //create nodes
        for (int i = 0; i <= 5; i++)
        {
            AddNode(i);
        }
        //link 0 and 5
        finalNode.next = laneZero;
        laneZero.prev = finalNode;
        //Debug.Log("LinkedLaneList contents: " + ToString());
    }

    //returns the first node, if it exists, with a given value as its id
    public Node GetNode(int value)
    {
        if (!ids.Contains(value)) return null;
        Node x = laneZero;
        int i = 0;
        while (i < N)
        {
            if (x != null)
            {
                if (x.id == value) return x;
                x = x.next;
                i++;
            }
            else break;
        }
        return null;
    }

    //returns the lane ID a given distance away from source
    public int GetRelativeLane(int source, int offset)
    {
        if (offset == 0) return source;
        int result = -1;
        Node x = GetNode(source);
        if (offset > 0) //moving clockwise
        {
            for (int i = 0; i < offset; i++)
            {
                x = x.next;
            }
        }
        else if (offset < 0) //moving cc-wise
        {
            for (int i = 0; i < Mathf.Abs(offset); i++)
            {
                x = x.prev;
            }
        }
        result = x.id;
        if (result == -1) Debug.Log("GetRelativeLane() in LinkedLaneList failed");
        //Debug.Log("GetRelativeLane(): source " + source + ", offset " + offset + ", result " + result);
        return result;
    }

    /// <summary>
    /// returns the clockwise (negative) difference between source and dest.
    /// returns a NEGATIVE int to correspond to unit circle angles
    /// </summary>
    public int ClockwiseDifBetween(int source, int dest)
    {
        int result = 0;
        if (!ids.Contains(dest))
        {
            Debug.Log("called difBetween on dest that doesn't exist");
            return -1;
        }
        if (source == dest) return result;
        Node x = GetNode(source);
        while (x.id != dest)
        {
            x = x.next;
            result++;
        }
        //Debug.Log("ClockwiseDifBetween(): source " + source + ", dest " + dest + ", result " + -result);
        return -result;
    }

    /// <summary>
    /// returns the cc-wise (positive) difference between source and dest.
    /// returns a POSITIVE int to correspond to unit circle angles
    /// </summary>
    public int CounterCWiseDifBetween(int source, int dest)
    {
        int result = 0;
        if (!ids.Contains(dest))
        {
            Debug.Log("called difBetween on dest that doesn't exist");
            return -1;
        }
        if (source == dest) return result;
        Node x = GetNode(source);
        while (x.id != dest)
        {
            x = x.prev;
            result++;
        }
        //Debug.Log("CounterCWiseDifBetween(): source " + source + ", dest " + dest + ", result " + result);
        return result;
    }

    /// <summary>
    /// returns the smallest difference between source and dest
    /// returns a NEGATIVE int for clockwise and POSITIVE for cc-wise (unit circle angles)
    /// if both dists are equal it'll return the positive one
    /// </summary>
    public int MinDistanceBetween(int source, int dest)
    {
        int result = 0;
        if (source == dest) return result;
        int cwDist = ClockwiseDifBetween(source, dest); //this one should be negative
        int ccwDist = CounterCWiseDifBetween(source, dest); //positive
        Debug.Log("cwDist: " + cwDist);
        Debug.Log("ccwDist: " + ccwDist);
        if (Mathf.Abs(cwDist) < ccwDist) result = cwDist;
        else if (ccwDist < Mathf.Abs(cwDist)) result = ccwDist;
        else if (Mathf.Abs(cwDist) == ccwDist) result = ccwDist;
        //Debug.Log("MinDistanceBetween(): source " + source + ", dest " + dest + ", result " + result);
        return result;
    }

    public override string ToString()
    {
        string result = "LinkedLaneList is empty";
        if (laneZero != null)
        {
            result = "LinkedLaneList size is " + N + ", contents: ";
            Node x = laneZero;
            int i = 0;
            while(i < N)
            {
                if (x != null)
                {
                    result += x.id + ", ";
                    x = x.next;
                    i++;
                }
                else break;
            }
        }
        return result;
    }
}
