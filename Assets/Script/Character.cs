﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Libs;
using System;

public class Character : MonoBehaviour {

	public Sprite mainSprite;
	private EditorNode m_startNode;
	public Libs.Graph.Graph currentGraph;
    public Node currentNode;

    public string fileName;

    public WhisperTalkManager m_whisperTalk;
    public bool isOnBar = false;
	public bool isOnAnimation = false;
    public Vector3 finalPlace;
	public Vector3 doorPlace;
	public int tickTimeout = -1;
	GameTime currentGameTime;

	TextStruct textStruct;

	public bool TVisOn = false;
	public bool BubbleAlreadyDisplayed = false;
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
        Node.eTextMiniType textMiniType = Node.eTextMiniType.DEFAULT;
        if(_node.textminitype!="")
        {
            textMiniType = (Node.eTextMiniType)System.Enum.Parse(typeof(Node.eTextMiniType), _node.textminitype, true);
        }
		Node.eMood mood = Node.eMood.DEFAULT;
		if (_node.mood != "") {
			mood = (Node.eMood)System.Enum.Parse (typeof(Node.eMood), _node.mood, true);
		}
        return new Node(
            day,
            hour,
            minut,
            lifetime,
            _node.text,
            _node.minitext,
            textMiniType,
			mood
            );
    }

    public Libs.Graph.GraphEdge CreateGraphEdge(Libs.Graph.JSONEdge _edge, Libs.Graph.GraphNode from, Libs.Graph.GraphNode to)
    {
        Edge.Condition condition = new Edge.Condition((Edge.Condition.ENUM)System.Enum.Parse(typeof(Edge.Condition.ENUM), _edge.type));
        return new Edge(from, to, condition, _edge.label);
    }

    // Use this for initialization
    void Start ()
	{
		currentGraph = new Libs.Graph.Graph("Assets/Data/"+fileName, CreateGraphNode, CreateGraphEdge);

		PrintGraph(currentGraph.GetCurrentNode());

		TVEvent.m_mainTrigger += TvIsTrigger;
		m_whisperTalk.m_tickDisplayOver += DisplayWhisperStop;
		TimeManager.OnTicTriggered += OnTick;
		m_isWaitingForClick = false;
	}

	void subcribeAll(){
		if(DraughtEvent.m_mainTrigger != null)
		foreach (Delegate d in DraughtEvent.m_mainTrigger.GetInvocationList())
			DraughtEvent.m_mainTrigger -= (d as Action);

		DraughtEvent.m_mainTrigger += OnBeerReady;
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
		if (currentNode != (Node)currentGraph.GetCurrentNode ()) {
			//ChangeNode
			currentNode = (Node)currentGraph.GetCurrentNode();
			tickTimeout = currentNode.GetTicksDuration ();
			BubbleAlreadyDisplayed = false;
			m_whisperTalk.StopDisplayWhisper ();
		}

		// check StartTime
		if(currentNode.GetDay() == -1 ||
			(currentNode.GetDay() == currentGameTime.day &&
				((currentNode.GetHour()*100 + currentNode.GetMinut()) <  (currentGameTime.hours *100 + currentGameTime.minutes)))){

			if (!isOnBar) {
				if (!isOnAnimation) {
					tickTimeout += 2;
					this.gameObject.transform.position = doorPlace;
					this.GetComponent<Animator> ().SetTrigger ("EnterBar");
					isOnAnimation = true;
				}
				return;
			}

			//check Transition (ETAT)
			if(TVisOn)
				currentGraph.Transition(new Edge.Condition(Edge.Condition.ENUM.TV));
			if(!TVisOn)
				currentGraph.Transition(new Edge.Condition(Edge.Condition.ENUM.TVOFF));

			if (currentNode.GetTextMiniType() == Node.eTextMiniType.CHARACTEREXIT) {// if exitState, lancer l'animation exit
				if (!isOnAnimation) {
					this.GetComponent<Animator> ().SetTrigger ("ExitBar");
					isOnAnimation = true;
				}
				return;
			}

			//display text
			if(!BubbleAlreadyDisplayed){
				if (currentNode.GetMiniText () != "") {
					DisplayWhisper (currentNode.GetMiniText ());
				}
				textStruct = TextManager.m_instance.GetTextStruc(currentNode.GetTextMiniType ());
				DisplayWhisper (textStruct.m_whisper);

			}
		}
    }

	void DisplayWhisper(string text, bool displayOnRight = true)
    {
		BubbleAlreadyDisplayed = true;
        m_isWaitingForClick = true;
		m_whisperTalk.StartDisplayWhisper(text,displayOnRight);
    }

	void DisplayWhisperStop(){
		BubbleAlreadyDisplayed = false;
	}

    public void OnCharacEnter()
    {
        //PLAY DING DING SOUND
		DisplayWhisper(TextManager.m_instance.GetTextStruc(Node.eTextMiniType.CHARACTERENTRY).m_whisper);
    }

    public void OnEnterFinished()
    {
        this.gameObject.transform.position = finalPlace;
		isOnBar = true;
		isOnAnimation = false;
    }

	public void OnGoToDoor(){
		this.gameObject.transform.position = doorPlace;
		m_whisperTalk.StartDisplayWhisper (Node.eTextMiniType.CHARACTEREXIT.ToString()); //Node.eTextMiniType.CHARACTEREXIT
	}


	public void OnLeaveBar(){
		m_whisperTalk.StopDisplayWhisper();
		isOnAnimation = false;
		isOnBar = false;
	}

    public void OnMouseUp()
    {
        if (m_isWaitingForClick)
        {
            m_isWaitingForClick = false;

				if (currentNode.GetText() != "" || textStruct.m_mainTalk != "") { // OU PRECONSTRUIT TEXT
	            m_whisperTalk.StopDisplayWhisper();
				BubbleAlreadyDisplayed = false;
				MainTalkManager.m_instance.StartDisplayAnimation((currentNode.GetText() != "") ? currentNode.GetText() : textStruct.m_mainTalk,mainSprite);
				subcribeAll ();
	            //TODO: Change State
						}
        }
    }

	void TvIsTrigger(bool isOn){
		TVisOn = isOn;
	}

	void OnBeerReady(){
		currentGraph.Transition(new Edge.Condition(Edge.Condition.ENUM.BEER));
	}

	void OnTick(GameTime gametime){
		currentGameTime = gametime;
		if(!isOnAnimation)
			tickTimeout--;
		if (tickTimeout <= 0) {
			currentGraph.Transition(new Edge.Condition(Edge.Condition.ENUM.TIMEOUT));
		}
	}

	//currentNode.Transition(new Edge.Condition(Edge.Condition.ENUM.TV));
}
