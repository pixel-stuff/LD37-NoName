﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : MonoBehaviour {

	public EditorNode m_startNode;
	public Libs.Graph.Graph currentGraph;
    public Node currentNode;

    public string fileName;

    public WhisperTalkManager m_whisperTalk;
    public bool isOnBar = false;
    public Vector3 finalPlace;

    private bool m_isWaitingForClick = false;

    Character()
    {
        currentGraph = new Libs.Graph.Graph(new Node());
    }

    public Libs.Graph.GraphNode CreateGraphNode(Libs.Graph.JSONNode _node)
    {
        char hourminutdelimiter = ':';
        Debug.Log(_node.hourminut);
        string[] hourminut = _node.hourminut.Split(hourminutdelimiter);
        int day = -1;
        int hour = -1;
        int minut = -1;
        int lifetime = 0;
        System.Int32.TryParse(_node.day, out day);
        if (hourminut.Length > 1)
        {
            System.Int32.TryParse(hourminut[0], out hour);
            System.Int32.TryParse(hourminut[1], out minut);
        }
        System.Int32.TryParse(_node.lifetime, out lifetime);
        return new Node(
            day,
            hour,
            minut,
            lifetime,
            _node.text,
            _node.minitext,
            (Node.eTextMiniType)System.Enum.Parse(typeof(Node.eTextMiniType), _node.textminitype, true),
            (Node.eMood)System.Enum.Parse(typeof(Node.eMood), _node.mood, true)
            );
    }

    public Libs.Graph.GraphEdge CreateGraphEdge(Libs.Graph.JSONEdge _edge, Libs.Graph.GraphNode from, Libs.Graph.GraphNode to)
    {
        Edge.Condition condition = new Edge.Condition((Edge.Condition.ENUM)System.Enum.Parse(typeof(Edge.Condition.ENUM), _edge.type));
        return new Edge(from, to, condition);
    }

    // Use this for initialization
    void Start ()
	{
		currentGraph = new Libs.Graph.Graph("Assets/Data/"+fileName, CreateGraphNode, CreateGraphEdge);

		PrintGraph(currentGraph.GetCurrentNode());

		m_whisperTalk.m_tickDisplayOver += DisplayWhisperStop;
		m_isWaitingForClick = false;

	}

	private void PrintGraph(Libs.Graph.GraphNode _node, List<Edge.Condition> _conditions)
	{
		Libs.Graph.GraphNode currentNode = _node;
		print(currentNode.ToString());
		foreach (Edge.Condition c in _conditions)
		{
			Libs.Graph.GraphNode transition = currentNode.Transition(c);
			if (transition != currentNode)
			{
				PrintGraph(transition, _conditions);
			}
		}
    }

    private void PrintGraph(Libs.Graph.GraphNode _node)
    {
        Libs.Graph.GraphNode currentNode = _node;
        print(currentNode.ToString()+" "+currentNode.Edges.Count);
        foreach (Edge e in currentNode.Edges)
        {
            PrintGraph(e.GetExitNode());
        }
    }

    void Update()
    {
        if (currentNode != (Node)currentGraph.GetCurrentNode())
        {
            // Node non changer
        }
        else
        {
            currentNode = (Node)currentGraph.GetCurrentNode();
            if (!isOnBar)
            {
                // Appear On Door
                return;
            }
        }
    }

	void DisplayWhisper(string text, bool displayOnRight = true)
    {
        m_isWaitingForClick = true;
		m_whisperTalk.StartDisplayWhisper(text,displayOnRight);
    }

	void DisplayWhisperStop(){

	}

    public void OnCharacEnter()
    {
        //PLAY DING DING SOUND
    }

    public void OnEnterFinished()
    {
        this.gameObject.transform.position = finalPlace;
    }

    public void OnMouseUp()
    {
        if (m_isWaitingForClick)
        {
            m_isWaitingForClick = false;
            m_whisperTalk.StopDisplayWhisper();
            MainTalkManager.m_instance.StartDisplayAnimation("Jeremy a un tout petit zizi");
            //TODO: Change State
        }
    }
}